
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Runtime.InteropServices;
using DAAP;
using log4net;

namespace Tangerine.Plugins {

    internal delegate void SpotlightHelperDelegate (IntPtr notification);

    [Plugin ("spotlight")]
    public class SpotlightPlugin : IDisposable {

        private Dictionary<string, Track> tracks = new Dictionary<string, Track> ();
        private Dictionary<string, Track> updatedTracks = new Dictionary<string, Track> ();
        private SpotlightHelperDelegate cb;

        [DllImport ("SpotlightHelper.dylib")]
        private static extern void tangerine_run_music_query (SpotlightHelperDelegate cb);

        [DllImport ("SpotlightHelper.dylib")]
        private static extern void tangerine_stop_music_query ();

        [DllImport ("SpotlightHelper.dylib")]
        private static extern string tangerine_item_get_path (IntPtr item);

        [DllImport ("SpotlightHelper.dylib")]
        private static extern string tangerine_item_get_title (IntPtr item);

        [DllImport ("SpotlightHelper.dylib")]
        private static extern string tangerine_item_get_album (IntPtr item);

        [DllImport ("SpotlightHelper.dylib")]
        private static extern string tangerine_item_get_artist (IntPtr item);

        [DllImport ("SpotlightHelper.dylib")]
        private static extern short tangerine_item_get_bitrate (IntPtr item);

        [DllImport ("SpotlightHelper.dylib")]
        private static extern int tangerine_item_get_duration (IntPtr item);

        public SpotlightPlugin () {
            cb = new SpotlightHelperDelegate (OnSpotlightCallback);
            Thread thread = new Thread(QueryLoop);
            thread.IsBackground = true;
            thread.Start ();
        }

        private void QueryLoop () {
            tangerine_run_music_query (cb);			
        }

        private void OnSpotlightCallback (IntPtr item) {
            if (item == IntPtr.Zero) {
                // that's all for now, commit the changes

                foreach (string key in tracks.Keys) {
                    if (!updatedTracks.ContainsKey (key)) {
                        lock (Daemon.DefaultDatabase) {
                            Daemon.DefaultDatabase.RemoveTrack (tracks[key]);
                        }	
                    }
                }

                if (updatedTracks.Values.Count == tracks.Values.Count) {
                    updatedTracks.Clear ();
                    return;
                }
                
                Daemon.Log.DebugFormat ("Committing {0} total tracks", updatedTracks.Count);
                
                tracks = updatedTracks;
                updatedTracks = new Dictionary<string, Track> ();

                // that's all for now
                lock (Daemon.Server) {
                    Daemon.Server.Commit ();
                }

                return;
            }

            string path = tangerine_item_get_path (item);
            if (path == null || path == String.Empty) {
                return;
            }

            Track track;
            if (tracks.ContainsKey (path)) {
                track = tracks[path];				
            } else {
                track = new Track ();

                track.FileName = path;
                track.Title = tangerine_item_get_title (item);
                track.Album = tangerine_item_get_album (item);
                track.Artist = tangerine_item_get_artist (item);
                track.Duration = TimeSpan.FromSeconds (tangerine_item_get_duration(item));
                track.BitRate = tangerine_item_get_bitrate (item);

                lock (Daemon.DefaultDatabase) {
                    Daemon.DefaultDatabase.AddTrack (track);
                }
            }

            updatedTracks[path] = track;
        }

        public void Dispose () {
            tangerine_stop_music_query ();
        }
    }
}
