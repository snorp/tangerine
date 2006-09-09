
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
        private string dbpath;
        private object refreshLock = new object ();
        private DateTime lastChange = DateTime.MinValue;
        
        public BansheePlugin () {
            tracks = new Dictionary<int, Track> ();
            
            dbpath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"),
                                   ".gnome2/banshee/banshee.db");
            conn = new SqliteConnection("Version=3,URI=file://" + dbpath);
            conn.Open ();

            RefreshTracks ();
            
            if (Inotify.Enabled) {
                Inotify.Subscribe (Path.GetDirectoryName (dbpath), OnBansheeChanged, Inotify.EventType.Modify);

                Thread refreshThread = new Thread (RefreshLoop);
                refreshThread.Start ();
            }
        }

        private void RefreshTracks () {
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

        public void Dispose () {
            lock (refreshLock) {
                lastChange = DateTime.MaxValue;
                Monitor.Pulse (refreshLock);
            }
            
            if (conn != null) {
                conn.Close ();
                conn = null;
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
                            Daemon.Server.Commit ();
                        } catch (Exception e) {
                            Daemon.LogError ("Failed to refresh tracks", e);
                        }

                        lastChange = DateTime.MinValue;
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
