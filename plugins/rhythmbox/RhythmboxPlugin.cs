
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Nini;
using DAAP;
using log4net;

namespace Tangerine.Plugins {

    [Plugin ("rhythmbox")]
    public class RhythmboxPlugin : IDisposable {

        private Dictionary<string, Track> tracks = new Dictionary<string, Track> ();
        private List<Playlist> playlists = new List<Playlist> ();
        private string dbpath;
        private string plpath;
        
        public RhythmboxPlugin () {
            dbpath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".gnome2/rhythmbox/rhythmdb.xml");
            plpath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".gnome2/rhythmbox/playlists.xml");
                
            RefreshTracks ();
            RefreshPlaylists ();
        }

        private void ClearTracks () {
            foreach (Track track in tracks.Values) {
                Daemon.DefaultDatabase.RemoveTrack (track);
            }

            foreach (Playlist pl in playlists) {
                Daemon.DefaultDatabase.RemovePlaylist (pl);
            }

            tracks.Clear ();
            playlists.Clear ();
        }

        private void ClearPlaylists () {
            foreach (Playlist pl in playlists) {
                Daemon.DefaultDatabase.RemovePlaylist (pl);
            }

            playlists.Clear ();
        }

        private void RefreshTracks () {
            ClearTracks ();
            
            if (!File.Exists (dbpath)) {
                return;
            }
            
            XmlTextReader reader = new XmlTextReader (dbpath);
            Track track = null;
            
            while (reader.Read ()) {
                switch (reader.LocalName) {
                case "entry":
                    if (reader.NodeType == XmlNodeType.EndElement && track != null && track.FileName != null) {
                        Daemon.DefaultDatabase.AddTrack (track);
                        tracks[track.FileName] = track;
                    } else if (reader.NodeType == XmlNodeType.Element) {
                        track = new Track ();
                    }
                    break;
                case "title":
                    track.Title = reader.ReadString ();
                    break;
                case "genre":
                    track.Genre = reader.ReadString ();
                    break;
                case "artist":
                    track.Artist = reader.ReadString ();
                    break;
                case "album":
                    track.Album = reader.ReadString ();
                    break;
                case "track-number":
                    track.TrackNumber = Int32.Parse (reader.ReadString ());
                    break;
                case "duration":
                    track.Duration = TimeSpan.FromSeconds (Int32.Parse (reader.ReadString ()));
                    break;
                case "location":
                    Uri uri = new Uri (reader.ReadString ());
                    if (uri.IsFile) {
                        track.FileName = uri.LocalPath;
                    }
                    break;
                case "bitrate":
                    track.BitRate = Int16.Parse (reader.ReadString ());
                    break;
                default:
                    break;
                }
            }

            reader.Close ();
        }

        private void RefreshPlaylists () {
            ClearPlaylists ();

            if (!File.Exists (plpath)) {
                return;
            }

            XmlTextReader reader = new XmlTextReader (plpath);
            Playlist pl = null;
            
            while (reader.Read ()) {
                switch (reader.LocalName) {
                case "playlist":
                    if (reader.NodeType == XmlNodeType.EndElement && pl != null) {
                        Daemon.DefaultDatabase.AddPlaylist (pl);
                        playlists.Add (pl);
                    } else if (reader.NodeType == XmlNodeType.Element &&
                               reader["type"] == "static") {
                        pl = new Playlist (reader["name"]);
                    }
                    break;
                case "location":
                    if (pl != null) {
                        Uri uri = new Uri (reader.ReadString ());

                        if (uri.IsFile && tracks.ContainsKey (uri.LocalPath)) {
                            pl.AddTrack (tracks[uri.LocalPath]);
                        }
                    }
                    
                    break;
                }
            }

            reader.Close ();
        }

        public void Dispose () {

        }
    }
}
