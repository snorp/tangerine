
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
            query.SendAsync ();
        }

        private int GetHitInteger (Hit hit, string key) {
            try {
                return Int32.Parse (hit.GetFirstProperty (key));
            } catch {
                return 0;
            }
        }

        private void OnHitsAdded (HitsAddedResponse response) {
            foreach (Hit hit in response.Hits) {
                if (hit.Uri.Scheme != Uri.UriSchemeFile)
                    continue;

                Song song = new Song ();
                song.TrackNumber = GetHitInteger (hit, "fixme:tracknumber");
                song.TrackCount = GetHitInteger (hit, "fixme:trackcount");
                song.Year = GetHitInteger (hit, "fixme:year");
                song.Album = hit.GetFirstProperty ("fixme:album");
                song.Artist = hit.GetFirstProperty ("fixme:artist");
                song.Title = hit.GetFirstProperty ("fixme:title");
                song.Genre = hit.GetFirstProperty ("fixme:genre");
                song.FileName = hit.Uri.LocalPath;

                // gotta have at least a title
                if (song.Title != null && song.Title != String.Empty) {
                    db.AddSong (song);
                }
            }
        }

        private void OnHitsSubtracted (HitsSubtractedResponse response) {
            foreach (Uri uri in response.Uris) {
                if (uri.Scheme != Uri.UriSchemeFile)
                    continue;

                string path = uri.LocalPath;
                foreach (Song song in db.Songs) {
                    if (song.FileName == path) {
                        db.RemoveSong (song);
                        break;
                    }
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
        }
    }
}
