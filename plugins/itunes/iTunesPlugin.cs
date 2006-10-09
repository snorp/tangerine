
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
        private FileSystemWatcher fsw;
        private DateTime lastChange = DateTime.MinValue;
        private object refreshLock = new object ();
        
        public iTunesPlugin () {
#if WINDOWS
            dbpath = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic) + @"\iTunes\iTunes Music Library.xml";
#else
            dbpath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + @"/Music/iTunes/iTunes Music Library.xml";
#endif

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

            Thread refreshThread = new Thread (RefreshLoop);
            refreshThread.Start ();

            fsw = new FileSystemWatcher (itunesDir);
            fsw.Changed += delegate (object o, FileSystemEventArgs args) {
                if (args.FullPath != dbpath)
                    return;

                lock (refreshLock) {
                    lastChange = DateTime.Now;
                    Monitor.Pulse (refreshLock);
                }
            };
            fsw.EnableRaisingEvents = true;
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
            if (!File.Exists (dbpath)) {
                ClearTracks ();
                return;
            }

            XmlDocument doc = new XmlDocument ();
            doc.Load (dbpath);

            Dictionary<long, Track> newTracks = new Dictionary<long, Track> ();

            // parse the tracks
            foreach (XmlNode node in doc.SelectNodes("/plist/dict/key[text()='Tracks']/following-sibling::*[1]//dict")) {
                Dictionary<string, object> dict = ParseDictionary (node);

                Uri uri = new Uri ((string) dict["Location"]);
                if (!uri.IsFile)
                    continue;

                long trackId = (long) dict["Track ID"];

                Track track;
                if (tracks.ContainsKey (trackId)) {
                    track = tracks[trackId];
                } else {
                    track = new Track ();
                    Daemon.DefaultDatabase.AddTrack (track);
                }

                newTracks[trackId] = track;

                track.FileName = uri.LocalPath;
                if (track.FileName.StartsWith (@"\\localhost\")) {
                    track.FileName = track.FileName.Substring (12);
                }

                track.Artist = GetDictValue<string> (dict, "Artist");
                track.Album = GetDictValue<string> (dict, "Album");
                track.Title = GetDictValue<string> (dict, "Name");
                track.Genre = GetDictValue<string> (dict, "Genre");

                if (dict.ContainsKey ("Total Time")) {
                    track.Duration = TimeSpan.FromMilliseconds ((long) dict["Total Time"]);
                }

                if (dict.ContainsKey ("Bit Rate")) {
                    track.BitRate = Convert.ToInt16 ((long) dict["Bit Rate"]);
                }
            }

            foreach (long id in tracks.Keys) {
                if (!newTracks.ContainsKey (id)) {
                    Daemon.DefaultDatabase.RemoveTrack (tracks[id]);
                }
            }

            tracks = newTracks;

            // parse the playlists
            XmlNode arrayNode = doc.SelectNodes ("/plist/dict/key[text()='Playlists']/following-sibling::*[1]")[0];
            List<Dictionary<string, object>> list = ParseDictionaryArray (arrayNode.ChildNodes);

            foreach (Dictionary<string, object> dict in list) {
                if (dict.ContainsKey ("Visible")) {
                    bool visible = (bool) dict["Visible"];
                    if (!visible)
                        continue;
                }

                long plid = (long) dict["Playlist ID"];

                Playlist pl;
                if (playlists.ContainsKey (plid)) {
                    pl = playlists[plid];
                } else {
                    pl = new Playlist ((string) dict["Name"]);
                    Daemon.DefaultDatabase.AddPlaylist (pl);
                    playlists[plid] = pl;
                }

                pl.Clear ();

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

            Daemon.Log.DebugFormat ("Added {0} tracks and {1} playlists", tracks.Count, playlists.Count);
        }

        private void RefreshLoop () {
            TimeSpan threshold = TimeSpan.FromSeconds (5);

            while (true) {
                lock (refreshLock) {
                    if (!Monitor.Wait (refreshLock, threshold) && lastChange != DateTime.MinValue &&
                        DateTime.Now - lastChange >= threshold) {
                        try {
                            RefreshTracks ();
                            Daemon.Server.Commit ();
                            lastChange = DateTime.MinValue;
                        } catch (Exception e) {
                            // ignore errors here, they are usually due to a crappy race or something
                        }
                    } else if (lastChange == DateTime.MaxValue) {
                        break;
                    }
                }
            }
        }

        public void Dispose () {
            if (watcher != null) {
                watcher.Dispose ();
                watcher = null;
            }

            lock (refreshLock) {
                lastChange = DateTime.MaxValue;
                Monitor.Pulse (refreshLock);
            }

            ClearTracks ();
        }
    }
}
