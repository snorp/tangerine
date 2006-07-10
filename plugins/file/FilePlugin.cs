
using System;
using System.IO;
using System.Collections;
using System.Threading;
using Entagged;
using Entagged.Audioformats.Exceptions;
using Nini;
using DAAP;
using log4net;
using com.db4o;

namespace Tangerine.Plugins {

    [Plugin ("file")]
    public class FilePlugin : IDisposable {
        private Hashtable songHash = new Hashtable ();
        private Hashtable playlistHash = new Hashtable ();
        private ArrayList playlistFiles = new ArrayList ();
        private object commitLock = new object ();
        private DateTime lastChange = DateTime.MinValue;
        private string[] directories;
        private bool running = true;

        private ObjectContainer odb;

        private Server server;
        private Database db;
        private ILog log;
        
        public FilePlugin () {

#if !WINDOWS
            string defaultDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Music");
#else
            string defaultDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "My Music");
#endif
            if (Daemon.ConfigSource.Configs["FilePlugin"] == null) {
                directories = new string[] { defaultDir };
            } else {
                directories = Daemon.ConfigSource.Configs["FilePlugin"].Get ("directories", defaultDir).Split (':');
            }

            server = Daemon.Server;
            db = Daemon.DefaultDatabase;
            log = Daemon.Log;

#if !WINDOWS
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
                
                odb = Db4o.OpenFile (Path.Combine (Daemon.ConfigDirectory, "songs.db"));
            }

            ObjectSet result = odb.Get (typeof (Track));
            log.DebugFormat ("{0} songs in database", result.Count);
            
            foreach (Track song in result) {
                if (!File.Exists (song.FileName) || !IsInDirectories (song.FileName)) {
                    log.Debug ("Ignoring song from db: " + song);
                    odb.Delete (song);
                    continue;
                }
                
                db.AddTrack (song);
                songHash[song.FileName] = song;
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

#if !WINDOWS
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
            
            ICollection keys = songHash.Keys;
            string[] keyArray = new string[keys.Count];
            keys.CopyTo (keyArray, 0);
            
            foreach (string file in keyArray) {
                if (file.StartsWith (dir)) {
                    RemoveTrack (file);
                }
            }
        }

        private void AddTrack (string file) {
            if (songHash[file] != null)
                return;
            
            AudioFile af;

            try {
                af = new AudioFile (file);
            } catch (Exception e) {
                return;
            }

            Track song = (Track) songHash[file];
            if (song == null) {
                song = new Track ();
                db.AddTrack (song);
            }

            song.Artist = af.Artist;
            song.Album = af.Album;
            song.Title = af.Title;
            song.Duration = af.Duration;
            song.FileName = file;
            song.Format = Path.GetExtension (file).Substring (1);
            song.Genre = af.Genre;

            FileInfo info = new FileInfo (file);
            song.Size = (int) info.Length;
            song.TrackCount = af.TrackCount;
            song.TrackNumber = af.TrackNumber;
            song.Year = af.Year;
            song.BitRate = (short) af.Bitrate;

            songHash[file] = song;

            odb.Set (song);
        }

        private void RemoveTrack (string file) {
            Track song = (Track) songHash[file];
            if (song != null) {
                db.RemoveTrack (song);
                songHash.Remove (file);
                odb.Delete (song);
            }
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
                    
                    Track song = (Track) songHash[songFile];
                    if (song != null) {
                        pl.AddTrack (song);
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
            Playlist pl = (Playlist) playlistHash[file];

            if (pl != null) {
                db.RemovePlaylist (pl);
                playlistHash.Remove (file);
            }
        }

#if !WINDOWS
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
