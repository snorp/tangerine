
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Nini;
using DAAP;
using log4net;

namespace Tangerine.Plugins {

    [Plugin ("itunes")]
    public class iTunesPlugin : IDisposable {

        private List<Track> tracks = new List<Track> ();
        private Dictionary<int, Playlist> playlists = new Dictionary<int, Playlist> ();
        private string dbpath;
        private string itunesDir;
        private CreationWatcher watcher;
        
        public iTunesPlugin () {
            dbpath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), "itunes.xml");
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
                }
                break;
            default:
                break;
            }
        }

        private void ClearTracks () {
            foreach (Track track in tracks) {
                Daemon.DefaultDatabase.RemoveTrack (track);
            }

            tracks.Clear ();
        }

        private void RefreshTracks () {
            ClearTracks ();
            
            if (!File.Exists (dbpath)) {
                return;
            }
            
            XmlTextReader reader = new XmlTextReader (dbpath);

            Track track = null;

            string key = null;
            
            while (reader.Read ()) {
                switch (reader.LocalName) {
                case "dict":
                    if (reader.NodeType == XmlNodeType.Element && reader.Depth == 3) {
                        track = new Track ();
                    } else if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == 3) {
                        if (track.FileName != null) {
                            Daemon.DefaultDatabase.AddTrack (track);
                            tracks.Add (track);
                        }
                    }
                    break;
                case "key":
                    key = reader.ReadString ();
                    break;
                case "integer":
                case "string":
                case "date":
                    SetTrackProperty (track, key, reader.ReadString ());
                    break;
                default:
                    break;
                }
            }

            reader.Close ();
        }

        public void Dispose () {
            if (watcher != null) {
                watcher.Dispose ();
                watcher = null;
            }
        }
    }
}
