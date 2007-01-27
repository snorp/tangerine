
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Nini;
using DAAP;
using log4net;
using Db4objects.Db4o;

[assembly: Tangerine.Plugin ("file", typeof (Tangerine.Plugins.FilePlugin))]

namespace Tangerine.Plugins {

    public class FilePlugin : IDisposable {
        private Dictionary<string, Track> trackHash = new Dictionary<string, Track> ();
        private Dictionary<string, Playlist> playlistHash = new Dictionary<string, Playlist> ();
        private List<string> playlistFiles = new List<string> ();
        private object commitLock = new object ();
        private DateTime lastChange = DateTime.MinValue;
        private string[] directories;
        private bool running = true;

        private IObjectContainer odb;

        private Server server;
        private Database db;
        private ILog log;
        
        public FilePlugin () {

#if LINUX || MACOSX
            string defaultDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Music");
            char splitChar = ':';
#else
            string defaultDir = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);
            char splitChar = ';';
#endif
            if (Daemon.ConfigSource.Configs["FilePlugin"] == null) {
                directories = new string[] { defaultDir };
            } else {
                directories = Daemon.ConfigSource.Configs["FilePlugin"].Get ("directories", defaultDir).Split (splitChar);
            }

            server = Daemon.Server;
            db = Daemon.DefaultDatabase;
            log = Daemon.Log;

#if LINUX
            if (Inotify.Enabled) {
                log.Info ("Using inotify to watch for changes");
            } else {
                log.Warn ("inotify is not available, filesystem changes will not be observed");
            }
#endif

            LoadFromDatabase ();
            ScanDirectories ();

            Thread commitThread = new Thread (CommitLoop);
            commitThread.Start ();

            log.Info ("Finished adding songs");
        }

        public void Dispose () {
            running = false;
            
            lock (commitLock) {
                Monitor.Pulse (commitLock);
            }

            odb.Close ();
        }

        private void CommitLoop () {
            TimeSpan threshold = TimeSpan.FromSeconds (5);
            
            while (true) {
                lock (commitLock) {
                    if (!Monitor.Wait (commitLock, threshold) && lastChange != DateTime.MinValue &&
                        DateTime.Now - lastChange >= threshold) {
                        try {
                            server.Commit ();
                        } catch (Exception e) {
                            Daemon.LogError ("Failed to commit changes", e);
                        }

                        lastChange = DateTime.MinValue;
                    } else if (!running) {
                        break;
                    }
                }
            }
        }

        private bool IsInDirectories (string file) {
            foreach (string dir in directories) {
                if (file.StartsWith (dir)) {
                    return true;
                }
            }

            return false;
        }

        private void LoadFromDatabase () {
            if (odb == null) {
                if (!Directory.Exists (Daemon.ConfigDirectory)) {
                    Directory.CreateDirectory (Daemon.ConfigDirectory);
                }

                Db4oFactory.Configure().AllowVersionUpdates (true);
                odb = Db4oFactory.OpenFile (Path.Combine (Daemon.ConfigDirectory, "tracks.db"));
            }

            IObjectSet result = odb.Get (typeof (Track));
            log.DebugFormat ("{0} songs in database", result.Count);
            
            foreach (Track song in result) {
                if (!File.Exists (song.FileName) || !IsInDirectories (song.FileName)) {
                    log.Debug ("Ignoring song from db: " + song);
                    odb.Delete (song);
                    continue;
                }
                
                db.AddTrack (song);
                trackHash[song.FileName] = song;
            }
        }

        private void ScanDirectories () {
            foreach (string dir in directories) {
                log.InfoFormat ("Adding songs in '{0}'", dir);
                AddDirectory (dir);
            }

            foreach (string plfile in playlistFiles) {
                AddPlaylist (plfile);
            }

            playlistFiles.Clear ();
            server.Commit ();
        }

        private void AddDirectory (string dir) {
            if (!Directory.Exists (dir)) {
                log.ErrorFormat ("Directory '{0}' does not exist", dir);
                return;
            }

#if LINUX
            Inotify.Subscribe (dir, OnDirectoryEvent,
                               Inotify.EventType.CloseWrite | Inotify.EventType.MovedFrom |
                               Inotify.EventType.MovedTo | Inotify.EventType.Delete | Inotify.EventType.Unmount);
#endif

            foreach (string file in Directory.GetFiles (dir)) {
                if (Path.GetExtension (file) == ".m3u") {
                    playlistFiles.Add (file);
                } else {
                    AddTrack (file);
                }
            }

            foreach (string childDir in Directory.GetDirectories (dir)) {
                AddDirectory (childDir);
            }
        }

        private void RemoveDirectory (string dir) {
            if (dir == null)
                return;
            
            foreach (string file in new List<string> (trackHash.Keys)) {
                if (file.StartsWith (dir)) {
                    RemoveTrack (file);
                }
            }
        }

        public static bool UpdateTrack (Track track, string file) {
            TagLib.File af;

            FileInfo info = new FileInfo (file);
            if((int) info.Length >= 0) {
                track.Size = (int) info.Length;
            } else {
                return false;
            }

            try {
                af = TagLib.File.Create (file);
            } catch {
                return false;
            }

            if(af.AudioProperties.Duration.TotalSeconds >= 1) {
                track.Duration = af.AudioProperties.Duration;
            } else {
                return false;
            }

            if((short) af.AudioProperties.Bitrate >=0) {
                track.BitRate = (short) af.AudioProperties.Bitrate;
            }else{
                return false;
            }

            if (af.Tag.Artists != null && af.Tag.Artists.Length > 0) {
                track.Artist = af.Tag.Artists[0];
            } else {
                track.Artist = String.Empty;
            }
            
            track.Album = af.Tag.Album;
            if (track.Artist != String.Empty || (af.Tag.Title != null && af.Tag.Title != String.Empty) || (af.Tag.Album != null && af.Tag.Album != String.Empty)) {
                track.Title = af.Tag.Title;
            } else {
                track.Title = info.Name;
            }
            track.FileName = file;
            track.Format = Path.GetExtension (file).Substring (1);

            if (af.Tag.Genres != null && af.Tag.Genres.Length > 0) {
                track.Genre = af.Tag.Genres[0];
            } else {
                track.Genre = String.Empty;
            }
            
            track.TrackCount = (int) af.Tag.TrackCount;
            track.TrackNumber = (int) af.Tag.Track;
            track.Year = (int) af.Tag.Year;

            return true;
        }

        private void AddTrack (string file) {
            if (trackHash.ContainsKey (file))
                return;
            
            Track track = new Track ();;
            bool gotInfo = false;

            try{
                gotInfo = UpdateTrack (track, file);
            } catch {
                gotInfo = false;
            }
               
            if(gotInfo) { 
                db.AddTrack (track);   
                trackHash[file] = track;
                odb.Set (track);
            }
        }

        private void RemoveTrack (string file) {
            if (!trackHash.ContainsKey (file))
                return;

            db.RemoveTrack (trackHash[file]);
            odb.Delete (trackHash[file]);
            trackHash.Remove (file);
        }

        private void AddPlaylist (string file) {
            Playlist pl = new Playlist (Path.GetFileNameWithoutExtension (file));

            string dir = Path.GetDirectoryName (file);
            
            using (StreamReader reader = new StreamReader (File.Open (file, FileMode.Open, FileAccess.Read))) {
                string line = null;

                while ((line = reader.ReadLine ()) != null) {
                    if (line.StartsWith ("#EXTM3U") || line.StartsWith ("#EXTINF:"))
                        continue;

                    string songFile = Path.Combine (dir, line);

                    if (trackHash.ContainsKey (songFile)) {
                        pl.AddTrack (trackHash[songFile]);
                    } else {
                        log.WarnFormat ("Failed to find song {0} for playlist {1}", line, pl.Name);
                    }
                }
            }

            playlistHash[file] = pl;
            db.AddPlaylist (pl);

            log.InfoFormat ("Added playlist '{0}'", pl.Name);
        }

        private void RemovePlaylist (string file) {
            if (!playlistHash.ContainsKey (file))
                return;

            db.RemovePlaylist (playlistHash[file]);
            playlistHash.Remove (file);
        }

#if LINUX
        private void OnDirectoryEvent (Inotify.Watch watch, string path, string subitem,
                                       string srcpath, Inotify.EventType type) {

            string file = Path.Combine (path, subitem);
            
            if ((type & Inotify.EventType.IsDirectory) > 0) {

                // something happened to a directory
                if ((type & Inotify.EventType.Delete) > 0 || (type & Inotify.EventType.MovedFrom) > 0) {
                    RemoveDirectory (file);
                } else if ((type & Inotify.EventType.Create) > 0) {
                    AddDirectory (file);
                } else if ((type & Inotify.EventType.MovedTo) > 0) {
                    RemoveDirectory (srcpath);
                    AddDirectory (file);
                }
                
            } else {
                if ((type & Inotify.EventType.Delete) > 0 || (type & Inotify.EventType.MovedFrom) > 0) {
                    RemoveTrack (file);
                } else if ((type & Inotify.EventType.CloseWrite) > 0 ||
                           (type & Inotify.EventType.MovedTo) > 0) {
                    AddTrack (file);
                }
            }

            lock (commitLock) {
                lastChange = DateTime.Now;
                Monitor.Pulse (commitLock);
            }
        }
#endif
    }
}
