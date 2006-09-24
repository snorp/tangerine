
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using DAAP;
using log4net;

namespace Tangerine.Plugins {

    [Plugin ("itunes")]
    public class iTunesPlugin : IDisposable {

        private Dictionary<long, Track> tracks = new Dictionary<long,Track> ();
        private Dictionary<long, Playlist> playlists = new Dictionary<long, Playlist> ();
        private string dbpath;
        private string itunesDir;
        private CreationWatcher watcher;
        
        public iTunesPlugin () {
            dbpath = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic) + @"\iTunes\iTunes Music Library.xml";

            itunesDir = Path.GetDirectoryName (dbpath);

            if (Directory.Exists (itunesDir)) {
                Init ();
            } else {
                watcher = new CreationWatcher (itunesDir);
                watcher.Created += delegate {
                    watcher.Dispose ();
                    watcher = null;
                    Init ();
                };
            }
        }

        private void Init () {
            RefreshTracks ();
        }

        private void SetTrackProperty (Track track, string key, string value) {
            switch (key) {
            case "Name":
                track.Title = value;
                break;
            case "Artist":
                track.Artist = value;
                break;
            case "Album":
                track.Album = value;
                break;
            case "Genre":
                track.Genre = value;
                break;
            case "Total Time":
                track.Duration = TimeSpan.FromMilliseconds (Int32.Parse (value));
                break;
            case "Bit Rate":
                track.BitRate = Int16.Parse (value);
                break;
            case "Location":
                Uri uri = new Uri (value);
                if (uri.IsFile) {
                    track.FileName = uri.LocalPath;
                    if (track.FileName.StartsWith(@"\\localhost\")) {
                        track.FileName = track.FileName.Substring(12);
                    }
                }
                break;
            default:
                break;
            }
        }

        private void ClearTracks () {
            foreach (Playlist pl in playlists.Values) {
                Daemon.DefaultDatabase.RemovePlaylist (pl);
            }

            foreach (Track track in tracks.Values) {
                Daemon.DefaultDatabase.RemoveTrack (track);
            }

            playlists.Clear ();
            tracks.Clear ();
        }

        private List<Dictionary<string, object>> ParseDictionaryArray (XmlNodeList children) {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>> ();

            foreach (XmlNode child in children) {
                list.Add (ParseDictionary (child));
            }

            return list;
        }

        private Dictionary<string, object> ParseDictionary (XmlNode node) {
            Dictionary<string, object> dict = new Dictionary<string, object> ();

            string key = null;

            foreach (XmlNode child in node.ChildNodes) {
                switch (child.LocalName) {
                case "key":
                    key = child.InnerText;
                    break;
                case "string":
                case "date":
                    dict[key] = child.InnerText;
                    break;
                case "integer":
                    dict[key] = Int64.Parse (child.InnerText);
                    break;
                case "array":
                    dict[key] = ParseDictionaryArray (child.ChildNodes);
                    break;
                case "dict":
                    dict[key] = ParseDictionary (child);
                    break;
                case "true":
                    dict[key] = true;
                    break;
                case "false":
                    dict[key] = false;
                    break;
                default:
                    break;
                }
            }

            return dict;
        }

        private T GetDictValue<T> (Dictionary<string, object> dict, string key) {
            if (dict.ContainsKey (key)) {
                return (T) dict[key];
            } else {
                return default (T);
            }
        }

        private void RefreshTracks () {
            ClearTracks ();
            
            if (!File.Exists (dbpath)) {
                return;
            }

            XmlDocument doc = new XmlDocument ();
            doc.Load (dbpath);

            // parse the tracks
            foreach (XmlNode node in doc.SelectNodes("/plist/dict/key[text()='Tracks']/following-sibling::*[1]//dict")) {
                Dictionary<string, object> dict = ParseDictionary (node);

                Uri uri = new Uri ((string) dict["Location"]);
                if (!uri.IsFile)
                    continue;

                Track track = new Track ();
                track.FileName = uri.LocalPath;
                if (track.FileName.StartsWith (@"\\localhost\")) {
                    track.FileName = track.FileName.Substring (12);
                }

                track.Artist = GetDictValue<string> (dict, "Artist");
                track.Album = GetDictValue<string> (dict, "Album");
                track.Title = GetDictValue<string> (dict, "Name");
                track.Genre = GetDictValue<string> (dict, "Genre");

                if (dict.ContainsKey ("Total Time")) {
                    track.Duration = TimeSpan.FromSeconds ((long) dict["Total Time"]);
                }

                if (dict.ContainsKey ("Bit Rate")) {
                    track.BitRate = Convert.ToInt16 ((long) dict["Bit Rate"]);
                }

                tracks[(long) dict["Track ID"]] = track;
                Daemon.DefaultDatabase.AddTrack (track);
            }

            // parse the playlists
            XmlNode arrayNode = doc.SelectNodes ("/plist/dict/key[text()='Playlists']/following-sibling::*[1]")[0];
            List<Dictionary<string, object>> list = ParseDictionaryArray (arrayNode.ChildNodes);

            foreach (Dictionary<string, object> dict in list) {
                if (dict.ContainsKey ("Visible")) {
                    bool visible = (bool) dict["Visible"];
                    if (!visible)
                        continue;
                }

                Playlist pl = new Playlist ((string) dict["Name"]);
                long plid = (long) dict["Playlist ID"];

                if (dict.ContainsKey ("Playlist Items")) {
                    List<Dictionary<string, object>> items = (List<Dictionary<string, object>>) dict["Playlist Items"];

                    foreach (Dictionary<string, object> itemDict in items) {
                        long id = (long) itemDict["Track ID"];
                        if (tracks.ContainsKey (id)) {
                            pl.AddTrack (tracks[id]);
                        }
                    }
                }

                playlists[plid] = pl;
                Daemon.DefaultDatabase.AddPlaylist (pl);
            }
        }

        public void Dispose () {
            if (watcher != null) {
                watcher.Dispose ();
                watcher = null;
            }
        }
    }
}
