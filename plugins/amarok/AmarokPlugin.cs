
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Mono.Data.SqliteClient;
using Nini;
using DAAP;
using log4net;

namespace Tangerine.Plugins {

    [Plugin ("amarok")]
    public class AmarokPlugin : IDisposable {

        private IDbConnection conn;
        private Dictionary<string, Track> tracks;
        private Dictionary<string, Playlist> playlists;
        private string dbpath;
        private string pldir;
        private string amarokDir;
        private object commitLock = new object ();
        private DateTime lastChange = DateTime.MinValue;
        private CreationWatcher watcher;
        
        public AmarokPlugin () {
            tracks = new Dictionary<string, Track> ();
            playlists = new Dictionary<string, Playlist> ();
            
            dbpath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"),
                                   ".kde/share/apps/amarok/collection.db");
            pldir = Path.Combine (Environment.GetEnvironmentVariable ("HOME"),
                                  ".kde/share/apps/amarok/playlists");

            amarokDir = Path.GetDirectoryName (dbpath);
            
            if (Directory.Exists (amarokDir)) {
                Init ();
            } else {
                watcher = new CreationWatcher (amarokDir);
                watcher.Created += delegate {
                    watcher.Dispose ();
                    watcher = null;
                    Init ();
                };
            }
        }

        private void Init () {
            RefreshTracks ();

            if (Inotify.Enabled) {
                Inotify.Subscribe (amarokDir, OnAmarokChanged, Inotify.EventType.Create |
                                   Inotify.EventType.CloseWrite);

                Thread commitThread = new Thread (CommitLoop);
                commitThread.Start ();
            }

            if (Directory.Exists (pldir)) {
                InitPlaylists ();
            }
        }

        private void InitPlaylists () {
            RefreshPlaylists ();

            if (Inotify.Enabled) {
                Inotify.Subscribe (pldir, OnPlaylistsChanged, Inotify.EventType.CloseWrite |
                                   Inotify.EventType.Delete | Inotify.EventType.MovedFrom |
                                   Inotify.EventType.MovedTo);
            }
        }

        private bool OpenConnection () {
            if (conn != null)
                return true;

            if (!File.Exists (dbpath))
                return false;

            try {
                conn = new SqliteConnection("Version=3,URI=file://" + dbpath);
                conn.Open ();
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        // I am apparently too dumb to figure out the SQL join needed to avoid this
        private Dictionary<int, string> FetchStrings (string table) {
            IDbCommand cmd = conn.CreateCommand ();
            cmd.CommandText = String.Format ("SELECT id, name FROM {0}", table);

            IDataReader reader = cmd.ExecuteReader ();

            Dictionary<int, string> dict = new Dictionary<int, string> ();
            while (reader.Read ()) {
                dict[(int) reader[0]] = (string) reader[1];
            }

            reader.Close ();

            return dict;
        }

        private void RefreshTracks () {
            if (!OpenConnection ())
                return;

            Dictionary<int, string> albums = FetchStrings ("album");
            Dictionary<int, string> artists = FetchStrings ("artist");
            Dictionary<int, string> genres = FetchStrings ("genre");
            Dictionary<int, string> years = FetchStrings ("year");
            
            IDbCommand cmd = conn.CreateCommand ();
            cmd.CommandText = "SELECT url, album, artist, genre, title, year, track, bitrate, length FROM tags";

            List<string> paths = new List<string> ();
            
            IDataReader reader = cmd.ExecuteReader ();
            int count = 0;
            
            while (reader.Read ()) {
                string path = (string) reader[0];
                if (!path.StartsWith ("/"))
                    continue;

                paths.Add (path);
                
                if (tracks.ContainsKey (path))
                    continue;

                Track track = new Track ();
                track.FileName = path;
                track.Artist = artists[(int) reader[2]];
                track.Album = albums[(int) reader[1]];
                track.Title = (string) reader[4];
                track.Genre = genres[(int) reader[3]];
                track.Year = Int32.Parse (years[(int) reader[5]]);
                track.TrackNumber = Convert.ToInt32 ((long) reader[6]);
                track.BitRate = Convert.ToInt16 ((int) reader[7]);
                track.Duration = TimeSpan.FromSeconds ((int) reader[8]);
                
                Daemon.DefaultDatabase.AddTrack (track);
                tracks[path] = track;
                count++;
            }

            reader.Close ();

            Daemon.Log.DebugFormat ("Added {0} tracks", count);
            count = 0;

            foreach (string path in new List<string> (tracks.Keys)) {
                if (!paths.Contains (path)) {
                    Daemon.DefaultDatabase.RemoveTrack (tracks[path]);
                    tracks.Remove (path);
                    count++;
                }
            }

            Daemon.Log.DebugFormat ("Removed {0} tracks", count);
        }

        private void RefreshPlaylists () {
            if (!Directory.Exists (pldir))
                return;

            foreach (string plfile in Directory.GetFiles (pldir, "*.m3u")) {
                RefreshPlaylist (plfile);
            }
        }

        private void RefreshPlaylist (string plfile) {
            Playlist pl;
            
            if (playlists.ContainsKey (plfile)) {
                pl = playlists[plfile];
                pl.Clear ();
            } else {
                pl = new Playlist (Path.GetFileNameWithoutExtension (plfile));
                Daemon.DefaultDatabase.AddPlaylist (pl);
                playlists[plfile] = pl;
            }
            
            using (StreamReader reader = new StreamReader (File.OpenRead (plfile))) {
                string line;

                while ((line = reader.ReadLine ()) != null) {
                    if (!line.StartsWith ("/"))
                        continue;
                    
                    if (tracks.ContainsKey (line)) {
                        pl.AddTrack (tracks[line]);
                    }
                }
            }
        }

        private void RemovePlaylist (string plfile) {
            if (!playlists.ContainsKey (plfile))
                return;
            
            Playlist pl = playlists[plfile];
            Daemon.DefaultDatabase.RemovePlaylist (pl);
            playlists.Remove (plfile);
        }

        public void Dispose () {
            lock (commitLock) {
                lastChange = DateTime.MaxValue;
                Monitor.Pulse (commitLock);
            }

            if (watcher != null) {
                watcher.Dispose ();
                watcher = null;
            }
            
            if (conn != null) {
                conn.Close ();
                conn = null;
            }
        }

        private void CommitLoop () {
            TimeSpan threshold = TimeSpan.FromSeconds (5);
            
            while (true) {
                lock (commitLock) {
                    if (!Monitor.Wait (commitLock, threshold) && lastChange != DateTime.MinValue &&
                        DateTime.Now - lastChange >= threshold) {
                        try {
                            Daemon.Server.Commit ();
                            lastChange = DateTime.MinValue;
                        } catch (Exception e) {
                            Daemon.Log.Error ("Failed to commit changes", e);
                        }
                    } else if (lastChange == DateTime.MaxValue) {
                        break;
                    }
                }
            }
        }

        private void OnAmarokChanged (Inotify.Watch watch, string path, string subitem,
                                       string srcpath, Inotify.EventType type) {

            string file = Path.Combine (path, subitem);

            if ((type & Inotify.EventType.CloseWrite) > 0 && file == dbpath) {
                try {
                    RefreshTracks ();
                } catch (Exception e) {
                    // sometimes we get some crappy random sqlite errors (race somewhere?); eat them
                }
            } else if ((type & Inotify.EventType.Create) > 0 &&
                       (type & Inotify.EventType.IsDirectory) > 0 &&
                       file == pldir) {
                InitPlaylists ();
            }

            lock (commitLock) {
                lastChange = DateTime.Now;
                Monitor.Pulse (commitLock);
            }
        }

        private void OnPlaylistsChanged (Inotify.Watch watch, string path, string subitem,
                                         string srcpath, Inotify.EventType type) {

            string file = Path.Combine (path, subitem);

            if (!file.EndsWith ("m3u"))
                return;

            if ((type & Inotify.EventType.CloseWrite) > 0) {
                RefreshPlaylist (file);
            } else if ((type & Inotify.EventType.MovedTo) > 0) {
                RemovePlaylist (srcpath);
                RefreshPlaylist (file);
            } else if (((type & Inotify.EventType.Delete) > 0 || (type & Inotify.EventType.MovedFrom) > 0) &&
                       playlists.ContainsKey (file)) {
                RemovePlaylist (file);
            }

            lock (commitLock) {
                lastChange = DateTime.Now;
                Monitor.Pulse (commitLock);
            }
        }
    }
}
