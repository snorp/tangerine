
using System;
using System.IO;
using System.Collections;
using System.Threading;
using Nini;
using DAAP;
using log4net;

namespace Tangerine.Plugins {

    [Plugin ("ipod")]
    public class IPodPlugin {
        private Server server;
        private Database db;
        private ILog log;
        private IPod.DeviceEventListener listener;
        private Hashtable songHash = new Hashtable ();
        private Hashtable playlistHash = new Hashtable ();
        private ArrayList devices = new ArrayList ();
        
        public IPodPlugin () {
            server = Daemon.Server;
            db = Daemon.DefaultDatabase;
            log = Daemon.Log;

            listener = new IPod.DeviceEventListener ();
            
            AddDevices ();

            listener.DeviceAdded += OnDeviceAdded;
            listener.DeviceRemoved += OnDeviceRemoved;
            
            log.Info ("Finished adding iPod devices");
        }

        private void AddDevices () {
            foreach (IPod.Device dev in IPod.Device.ListDevices ()) {
                try {
                    AddDevice (dev);
                } catch (Exception e) {
                    Daemon.LogError (String.Format ("Failed to add songs from device '{0}'", dev.Name), e);
                }
            }

            server.Commit ();
        }

        private void AddDevice (IPod.Device device) {
            ArrayList songs = new ArrayList ();
            ArrayList playlists = new ArrayList ();
            
            foreach (IPod.Track itrack in device.TrackDatabase.Tracks) {
                Song song = AddSong (itrack);
                if (song != null)
                    songs.Add (song);
            }

            foreach (IPod.Playlist ipl in device.TrackDatabase.Playlists) {
                playlists.Add (AddPlaylist (ipl));
            }

            foreach (IPod.Playlist ipl in device.TrackDatabase.OnTheGoPlaylists) {
                playlists.Add (AddPlaylist (ipl));
            }

            songHash[device.VolumeId] = songs;
            playlistHash[device.VolumeId] = playlists;
            devices.Add (device);

            log.InfoFormat ("Added device '{0}'", device.Name);
        }

        private void RemoveDevice (IPod.Device device) {
            ArrayList songs = songHash[device.VolumeId] as ArrayList;
            ArrayList playlists = playlistHash[device.VolumeId] as ArrayList;

            foreach (Song song in songs) {
                db.RemoveSong (song);
            }

            foreach (Playlist pl in playlists) {
                db.RemovePlaylist (pl);
            }

            songHash.Remove (device.VolumeId);
            playlistHash.Remove (device.VolumeId);
            devices.Remove (device);
            
            log.InfoFormat ("Removed device '{0}'", device.Name);
        }

        // pretty lame, but it should avoid most duplicates
        private Song LookupSong (IPod.Track track) {
            foreach (Song song in db.Songs) {
                if (song.Title == track.Title && song.Size == track.Size) {
                    return song;
                }
            }

            return null;
        }

        private Song AddSong (IPod.Track track) {
            if (LookupSong (track) != null)
                return null;
            
            Song song = new Song ();
            db.AddSong (song);

            song.Artist = track.Artist;
            song.Album = track.Album;
            song.Title = track.Title;
            song.Duration = track.Duration;
            song.FileName = track.FileName;
            song.Format = Path.GetExtension (song.FileName).Substring (1);
            song.Genre = track.Genre;

            FileInfo info = new FileInfo (song.FileName);
            song.Size = (int) info.Length;
            song.TrackCount = track.TotalTracks;
            song.TrackNumber = track.TrackNumber;
            song.Year = track.Year;
            song.BitRate = (short) track.BitRate;

            return song;
        }

        private Playlist AddPlaylist (IPod.Playlist ipl) {
            Playlist pl = new Playlist (ipl.Name);

            foreach (IPod.Track itrack in ipl.Tracks) {
                Song song = LookupSong (itrack);
                if (song != null)
                    pl.AddSong (song);
            }

            db.AddPlaylist (pl);
            return pl;
        }

        private void OnDeviceAdded (object o, IPod.DeviceAddedArgs args) {
            try {
                AddDevice (new IPod.Device (args.Udi));
                server.Commit ();
            } catch (Exception e) {
                Daemon.LogError (String.Format ("Failed to add device '{0}'", args.Udi), e);
            }
        }

        private void OnDeviceRemoved (object o, IPod.DeviceRemovedArgs args) {
            foreach (IPod.Device dev in devices) {
                if (dev.VolumeId == args.Udi) {
                    RemoveDevice (dev);
                    server.Commit ();
                    break;
                }
            }
        }
    }
}
