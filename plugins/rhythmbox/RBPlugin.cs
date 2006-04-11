
using System;
using System.IO;
using System.Collections;
using System.Xml;
using Nini;
using DAAP;
using log4net;

namespace Tangerine.Plugins {

    [Plugin ("rhythmbox")]
    public class RBPlugin {
        private Server server;
        private Database db;
        private ILog log;

        private string dbpath;
        private ArrayList songs = new ArrayList ();
        
        public RBPlugin () {
            server = Daemon.Server;
            db = Daemon.DefaultDatabase;
            log = Daemon.Log;

            dbpath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/.gnome2/rhythmbox/rhythmdb.xml";

            Refresh ();

            log.Info ("Finished adding songs from rhythmbox");
        }

        public void Clear () {
            foreach (Song song in songs) {
                db.RemoveSong (song);
            }
        }

        public void Refresh () {
            Clear ();

            if (!File.Exists (dbpath))
                return;
            
            using (StreamReader sreader = new StreamReader (File.Open (dbpath, FileMode.Open, FileAccess.Read))) {
                XmlTextReader reader = new XmlTextReader (sreader);

                Song song = null;
                
                while (reader.Read ()) {
                    string tag = reader.LocalName;

                    if (tag == "entry") {
                        if (reader.NodeType == XmlNodeType.Element) {
                            if (reader["type"] == null || reader["type"] == "song") {
                                song = new Song ();
                            }
                        } else if (reader.NodeType == XmlNodeType.EndElement && song != null) {
                            db.AddSong (song);
                            song = null;
                        }
                    }

                    if (song == null) {
                        continue;
                    }

                    switch (tag) {
                    case "title":
                        song.Title = reader.ReadString ();
                        break;
                    case "genre":
                        song.Genre = reader.ReadString ();
                        break;
                    case "artist":
                        song.Artist = reader.ReadString ();
                        break;
                    case "track-number":
                        song.TrackNumber = Int32.Parse (reader.ReadString ());
                        break;
                    case "duration":
                        song.Duration = TimeSpan.FromSeconds (Int32.Parse (reader.ReadString ()));
                        break;
                    case "file-size":
                        song.Size = Int32.Parse (reader.ReadString ());
                        break;
                    case "location":
                        song.FileName = new Uri (reader.ReadString ()).LocalPath;
                        break;
                    case "bitrate":
                        song.BitRate = Int16.Parse (reader.ReadString ());
                        break;
                    default:
                        break;
                    }
                }
            }

            server.Commit ();
        }
    }
}
