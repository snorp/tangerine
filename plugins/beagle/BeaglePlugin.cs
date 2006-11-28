
using System;
using System.IO;
using System.Collections;
using System.Threading;
using Nini;
using DAAP;
using log4net;
using Beagle;

namespace Tangerine.Plugins {

    [Plugin ("beagle")]
    public class BeaglePlugin : IDisposable {

        private Server server;
        private Database db;
        private ILog log;

        private Query query;
        private Hashtable trackHash = new Hashtable ();
        
        private static string[] supportedMimeTypes = new string [] {
            "audio/mpeg",
            "audio/x-vorbis+ogg",
            "audio/x-m4a"
        };

        public BeaglePlugin () {
            server = Daemon.Server;
            db = Daemon.DefaultDatabase;
            log = Daemon.Log;

            query = new Query ();
            query.AddDomain (QueryDomain.Neighborhood);
            query.MaxHits = 10000;
            
            QueryPart_Property filePart = new QueryPart_Property ();
            filePart.Type = PropertyType.Keyword;
            filePart.Key = "beagle:HitType";
            filePart.Value = "File";
            query.AddPart (filePart);

            QueryPart_Or queryUnion = new QueryPart_Or ();

            foreach (string mt in supportedMimeTypes) {
                QueryPart_Property part = new QueryPart_Property ();
                part.Type = PropertyType.Keyword;
                part.Key = "beagle:MimeType";
                part.Value = mt;
                queryUnion.Add (part);
            }

            query.AddPart (queryUnion);
            query.HitsAddedEvent += OnHitsAdded;
            query.HitsSubtractedEvent += OnHitsSubtracted;
            query.FinishedEvent += OnFinished;

            int attempts = 0;
            while (true) {
                try {
                    query.SendAsync ();
                    break;
                } catch (Exception e) {
                    if (attempts++ >= 5) {
                        log.Warn ("Failed to initialize beagle plugin");
                        query = null;
                        break;
                    }

                    // something bad happened, wait a sec and try again
                    log.Debug ("Sending query failed: " + e.Message);
                    log.Debug ("Waiting 3 seconds...");
                    Thread.Sleep (3000);
                }
            }
        }

        private int GetHitInteger (Hit hit, string key) {
            try {
                return Int32.Parse (hit.GetFirstProperty (key));
            } catch {
                return 0;
            }
        }

        private Track GetTrackFromFile (string file) {
            try {
                Track track = new Track ();
                FilePlugin.UpdateTrack (track, file);
                return track;
            } catch {
                return null;
            }
        }

        private void OnHitsAdded (HitsAddedResponse response) {
            foreach (Hit hit in response.Hits) {
                if (hit.Uri.Scheme != Uri.UriSchemeFile)
                    continue;

                Track track;
                
                if (hit.GetFirstProperty ("fixme:title") != null) {
                    track = new Track ();
                    track.TrackNumber = GetHitInteger (hit, "fixme:tracknumber");
                    track.TrackCount = GetHitInteger (hit, "fixme:trackcount");
                    track.Year = GetHitInteger (hit, "fixme:year");
                    track.Album = hit.GetFirstProperty ("fixme:album");
                    track.Artist = hit.GetFirstProperty ("fixme:artist");
                    track.Title = hit.GetFirstProperty ("fixme:title");
                    track.Genre = hit.GetFirstProperty ("fixme:genre");
                    track.FileName = hit.Uri.LocalPath;
                    track.Format = Path.GetExtension (track.FileName).Substring (1);
                } else {
                    track = GetTrackFromFile (hit.Uri.LocalPath);
                }

                if (track != null && track.Title != null && track.Title != String.Empty) {
                    db.AddTrack (track);
                    trackHash[track.FileName] = track;
                }
            }
        }

        private void OnHitsSubtracted (HitsSubtractedResponse response) {
            foreach (Uri uri in response.Uris) {
                if (uri.Scheme != Uri.UriSchemeFile)
                    continue;

                string path = uri.LocalPath;
                Track track = trackHash[path] as Track;
                if (track != null) {
                    trackHash.Remove (path);
                    db.RemoveTrack (track);
                }
            }
        }
        
        private void OnFinished (FinishedResponse response) {
            server.Commit ();
        }

        public void Dispose () {
            if (query != null) {
                query.HitsAddedEvent -= OnHitsAdded;
                query.FinishedEvent -= OnFinished;
                query.Close ();
                query = null;
            }

            foreach (Track track in trackHash.Values) {
                db.RemoveTrack (track);
            }
        }
    }
}
