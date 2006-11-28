
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

    [Plugin ("banshee")]
    public class BansheePlugin : IDisposable {

        private IDbConnection conn;
        private Dictionary<int, Track> tracks;
        private Dictionary<int, Playlist> playlists;
        private string dbpath;
        private string bansheeDir;
        private object refreshLock = new object ();
        private DateTime lastChange = DateTime.MinValue;
        private CreationWatcher watcher;
        
        public BansheePlugin () {
            tracks = new Dictionary<int, Track> ();
            playlists = new Dictionary<int, Playlist> ();
            
            dbpath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"),
                                   ".gnome2/banshee/banshee.db");
            bansheeDir = Path.GetDirectoryName (dbpath);
            
            if (Directory.Exists (bansheeDir)) {
                Init ();
            } else {
                watcher = new CreationWatcher (bansheeDir);
                watcher.Created += delegate {
                    watcher.Dispose ();
                    watcher = null;
                    Init ();
                };
            }
        }

        private void Init () {
            RefreshTracks ();
            RefreshPlaylists ();
            
            if (Inotify.Enabled) {
                Inotify.Subscribe (Path.GetDirectoryName (dbpath), OnBansheeChanged, Inotify.EventType.Modify);

                Thread refreshThread = new Thread (RefreshLoop);
                refreshThread.Start ();
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

        private void RefreshTracks () {
            if (!OpenConnection ())
                return;
            
            IDbCommand cmd = conn.CreateCommand ();
            cmd.CommandText = "SELECT TrackID, Uri, Artist, AlbumTitle, ReleaseDate, Title, Genre, Year, " +
                "TrackNumber, TrackCount, Duration FROM Tracks";

            List<int> ids = new List<int> ();
            
            IDataReader reader = cmd.ExecuteReader ();
            int count = 0;
            
            while (reader.Read ()) {
                int id = (int) reader[0];
                ids.Add (id);
                
                if (tracks.ContainsKey (id))
                    continue;
                
                Uri uri = new Uri ((string) reader[1]);
                if (!uri.IsFile)
                    continue;

                Track track = new Track ();
                track.FileName = uri.LocalPath;
                track.Artist = (string) reader[2];
                track.Album = (string) reader[3];
                track.Title = (string) reader[5];
                track.Genre = (string) reader[6];
                track.Year = (int) reader[7];
                track.TrackNumber = (int) reader[8];
                track.TrackCount = (int) reader[9];
                track.Duration = TimeSpan.FromSeconds ((int) reader[10]);
                track.Format = Path.GetExtension (track.FileName).Substring (1);
                
                Daemon.DefaultDatabase.AddTrack (track);
                tracks[id] = track;
                count++;
            }

            reader.Close ();

            Daemon.Log.DebugFormat ("Added {0} tracks", count);
            count = 0;

            foreach (int id in new List<int> (tracks.Keys)) {
                if (!ids.Contains (id)) {
                    Daemon.DefaultDatabase.RemoveTrack (tracks[id]);
                    tracks.Remove (id);
                    count++;
                }
            }

            Daemon.Log.DebugFormat ("Removed {0} tracks", count);
        }

        private void RefreshPlaylists () {
            if (!OpenConnection ())
                return;
            
            IDbCommand cmd = conn.CreateCommand ();
            cmd.CommandText = "SELECT PlaylistID, Name FROM Playlists";

            List<int> ids = new List<int> ();
            
            IDataReader reader = cmd.ExecuteReader ();
            while (reader.Read ()) {
                int id = (int) reader[0];
                ids.Add (id);

                if (playlists.ContainsKey (id))
                    continue;
                
                Playlist pl = new Playlist ((string) reader[1]);
                
                Daemon.DefaultDatabase.AddPlaylist (pl);
                playlists[id] = pl;
            }

            reader.Close ();

            // remove the deleted playlists, if any
            foreach (int id in new List<int> (playlists.Keys)) {
                if (!ids.Contains (id)) {
                    Daemon.DefaultDatabase.RemovePlaylist (playlists[id]);
                    playlists.Remove (id);
                }
            }

            // clear all the playlists
            foreach (Playlist pl in playlists.Values) {
                pl.Clear ();
            }

            cmd = conn.CreateCommand ();
            cmd.CommandText = "SELECT PlaylistID, TrackID FROM PlaylistEntries";
            reader = cmd.ExecuteReader ();

            while (reader.Read ()) {
                try {
                    Playlist pl = playlists[(int) reader[0]];
                    pl.AddTrack (tracks[(int) reader[1]]);
                } catch (KeyNotFoundException e) {
                    // something is not consistent, but nothing we can do about it
                }
            }

            reader.Close ();
        }

        public void Dispose () {
            lock (refreshLock) {
                lastChange = DateTime.MaxValue;
                Monitor.Pulse (refreshLock);
            }
            
            if (conn != null) {
                conn.Close ();
                conn = null;
            }

            if (watcher != null) {
                watcher.Dispose ();
                watcher = null;
            }
        }

        private void RefreshLoop () {
            TimeSpan threshold = TimeSpan.FromSeconds (5);
            
            while (true) {
                lock (refreshLock) {
                    if (!Monitor.Wait (refreshLock, threshold) && lastChange != DateTime.MinValue &&
                        DateTime.Now - lastChange >= threshold) {
                        try {
                            RefreshTracks ();
                            RefreshPlaylists ();
                            Daemon.Server.Commit ();
                            lastChange = DateTime.MinValue;
                        } catch (Exception e) {
                            // ignore errors here, they are usually due to the database being locked or whatever
                        }
                    } else if (lastChange == DateTime.MaxValue) {
                        break;
                    }
                }
            }
        }

        private void OnBansheeChanged (Inotify.Watch watch, string path, string subitem,
                                       string srcpath, Inotify.EventType type) {

            string file = Path.Combine (path, subitem);
            if (file != dbpath)
                return;

            lock (refreshLock) {
                lastChange = DateTime.Now;
                Monitor.Pulse (refreshLock);
            }
        }
    }
}
