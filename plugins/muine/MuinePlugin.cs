
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Mono.Unix.Native;
using Nini;
using DAAP;
using log4net;

[assembly: Tangerine.Plugin ("muine", typeof (Tangerine.Plugins.MuinePlugin))]

namespace Tangerine.Plugins {

    public class MuinePlugin : IDisposable {

        private List<Track> tracks = new List<Track> ();
        private string dbpath;
        private string muineDir;
        private CreationWatcher watcher;

        public MuinePlugin () {
            dbpath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".gnome2/muine/songs.db");
            muineDir = Path.GetDirectoryName (dbpath);

            if (Directory.Exists (muineDir)) {
                Init ();
            } else {
                watcher = new CreationWatcher (muineDir);
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
                Inotify.Subscribe (muineDir, OnMuineChanged, Inotify.EventType.CloseWrite);
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

            MuineDatabase db = new MuineDatabase (dbpath, 6);
            db.Load (OnDatabaseKey);

            Daemon.Log.DebugFormat ("Refresh complete, {0} tracks found", tracks.Count);
        }

        private void OnDatabaseKey (string filename, IntPtr data) {
            IntPtr p = data;
            
            Track track = new Track ();
            track.FileName = filename;

            string str;
            string[] strs;
            int num;
                
            p = MuineDatabase.UnpackString (p, out str);
            track.Title = str;

            p = MuineDatabase.UnpackStringArray (p, out strs);
            if (strs != null && strs.Length > 0) {
                track.Artist = strs[0];
            }

            p = MuineDatabase.UnpackStringArray (p, out strs);

            p = MuineDatabase.UnpackString (p, out str);
            track.Album = str;
                
            p = MuineDatabase.UnpackInt (p, out num);
            track.TrackNumber = num;
                
            p = MuineDatabase.UnpackInt (p, out num);
            track.TrackCount = num;

            p = MuineDatabase.UnpackInt (p, out num);
                
            p = MuineDatabase.UnpackString (p, out str);
            try {
                track.Year = Int32.Parse (str);
            } catch {}

            int duration;
            p = MuineDatabase.UnpackInt (p, out duration);
            track.Duration = TimeSpan.FromSeconds (duration);

            Daemon.DefaultDatabase.AddTrack (track);
            tracks.Add (track);
        }

        public void Dispose () {
            if (watcher != null) {
                watcher.Dispose ();
                watcher = null;
            }
        }

        private void OnMuineChanged (Inotify.Watch watch, string path, string subitem,
                                         string srcpath, Inotify.EventType type) {
            string file = Path.Combine (path, subitem);

            if (file == dbpath) {
                RefreshTracks ();
                Daemon.Server.Commit ();
            }
        }
    }
}
