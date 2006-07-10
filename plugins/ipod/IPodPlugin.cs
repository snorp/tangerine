
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
        private Hashtable trackHash = new Hashtable ();
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
                    Daemon.LogError (String.Format ("Failed to add tracks from device '{0}'", dev.Name), e);
                }
            }

            server.Commit ();
        }

        private void AddDevice (IPod.Device device) {
            ArrayList tracks = new ArrayList ();
            ArrayList playlists = new ArrayList ();
            
            foreach (IPod.Track itrack in device.TrackDatabase.Tracks) {
                Track track = AddTrack (itrack);
                if (track != null)
                    tracks.Add (track);
            }

            foreach (IPod.Playlist ipl in device.TrackDatabase.Playlists) {
                playlists.Add (AddPlaylist (ipl));
            }

            foreach (IPod.Playlist ipl in device.TrackDatabase.OnTheGoPlaylists) {
                playlists.Add (AddPlaylist (ipl));
            }

            trackHash[device.VolumeId] = tracks;
            playlistHash[device.VolumeId] = playlists;
            devices.Add (device);

            log.InfoFormat ("Added device '{0}'", device.Name);
        }

        private void RemoveDevice (IPod.Device device) {
            ArrayList tracks = trackHash[device.VolumeId] as ArrayList;
            ArrayList playlists = playlistHash[device.VolumeId] as ArrayList;

            foreach (Track track in tracks) {
                db.RemoveTrack (track);
            }

            foreach (Playlist pl in playlists) {
                db.RemovePlaylist (pl);
            }

            trackHash.Remove (device.VolumeId);
            playlistHash.Remove (device.VolumeId);
            devices.Remove (device);
            
            log.InfoFormat ("Removed device '{0}'", device.Name);
        }

        // pretty lame, but it should avoid most duplicates
        private Track LookupTrack (IPod.Track itrack) {
            foreach (Track track in db.Tracks) {
                if (track.Title == itrack.Title && track.Size == itrack.Size) {
                    return track;
                }
            }

            return null;
        }

        private Track AddTrack (IPod.Track itrack) {
            if (LookupTrack (itrack) != null)
                return null;
            
            Track track = new Track ();
            db.AddTrack (track);

            track.Artist = itrack.Artist;
            track.Album = itrack.Album;
            track.Title = itrack.Title;
            track.Duration = itrack.Duration;
            track.FileName = itrack.FileName;
            track.Format = Path.GetExtension (itrack.FileName).Substring (1);
            track.Genre = itrack.Genre;

            FileInfo info = new FileInfo (itrack.FileName);
            track.Size = (int) info.Length;
            track.TrackCount = itrack.TotalTracks;
            track.TrackNumber = itrack.TrackNumber;
            track.Year = itrack.Year;
            track.BitRate = (short) itrack.BitRate;

            return track;
        }

        private Playlist AddPlaylist (IPod.Playlist ipl) {
            Playlist pl = new Playlist (ipl.Name);

            foreach (IPod.Track itrack in ipl.Tracks) {
                Track track = LookupTrack (itrack);
                if (track != null)
                    pl.AddTrack (track);
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
