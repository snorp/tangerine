
using System;
using System.IO;
using System.Collections;
using System.Data;
using Nini;
using DAAP;
using log4net;
using Mono.Data.SqliteClient;

namespace Tangerine.Plugins {

    [Plugin ("banshee")]
    public class BansheePlugin {
        private Server server;
        private Database db;
        private ILog log;

        private Hashtable songs = new Hashtable ();
        private ArrayList playlists = new ArrayList ();
        private string dbpath;
        
        public BansheePlugin () {
            server = Daemon.Server;
            db = Daemon.DefaultDatabase;
            log = Daemon.Log;

            dbpath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/.gnome2/banshee/banshee.db";

            Refresh ();

            log.Info ("Finished adding songs from banshee");
        }

        public void Clear () {
            foreach (Song song in songs.Values) {
                db.RemoveSong (song);
            }

            foreach (Playlist pl in playlists) {
                db.RemovePlaylist (pl);
            }

            songs.Clear ();
            playlists.Clear ();
        }

        private void AddSong (IDataReader reader) {
            int id = Int32.Parse (reader.GetString (0));
            
            Song song = new Song ();
            song.FileName = new Uri (reader.GetString (1)).LocalPath;
            song.Artist = reader.GetString (2);
            song.Album = reader.GetString (3);
            song.Title = reader.GetString (4);
            song.Genre = reader.GetString (5);
            song.Year = Int32.Parse (reader.GetString (6));
            song.TrackNumber = Int32.Parse (reader.GetString (7));
            song.TrackCount = Int32.Parse (reader.GetString (8));
            song.Duration = TimeSpan.FromSeconds (Int32.Parse (reader.GetString (9)));
            
            FileInfo info = new FileInfo (song.FileName);
            song.Size = (int) info.Length;

            songs[id] = song;
            db.AddSong (song);
        }

        private void RefreshSongs (IDbConnection conn) {
            IDbCommand cmd = conn.CreateCommand ();
            cmd.CommandText = "SELECT TrackID, Uri, Artist, AlbumTitle, Title, Genre, Year, TrackNumber, " +
                "TrackCount, Duration FROM Tracks";

            IDataReader reader = cmd.ExecuteReader ();

            while (reader.Read ()) {
                try {
                    AddSong (reader);
                } catch (Exception e) {
                    Daemon.LogError ("Failed to add banshee song", e);
                }
            }
        }

        private void RefreshPlaylist (IDbConnection conn, Playlist pl, int id) {
            IDbCommand cmd = conn.CreateCommand ();
            cmd.CommandText = "SELECT TrackID FROM PlaylistEntries WHERE PlaylistID = " + id;

            IDataReader reader = cmd.ExecuteReader ();

            while (reader.Read ()) {
                pl.AddSong ((Song) songs[Int32.Parse (reader.GetString (0))]);
            }
        }

        private void RefreshPlaylists (IDbConnection conn) {
            IDbCommand cmd = conn.CreateCommand ();
            cmd.CommandText = "SELECT PlaylistID, Name FROM Playlists";

            IDataReader reader = cmd.ExecuteReader ();

            while (reader.Read ()) {
                int id = Int32.Parse (reader.GetString (0));
                Playlist pl = new Playlist (reader.GetString (1));

                RefreshPlaylist (conn, pl, id);
                db.AddPlaylist (pl);
                playlists.Add (pl);
            }
        }

        public void Refresh () {
            Clear ();
            
            IDbConnection conn = new SqliteConnection ("Version=3,URI=file:" + dbpath);
            conn.Open ();
            
            RefreshSongs (conn);
            RefreshPlaylists (conn);

            conn.Close ();

            server.Commit ();
        }
    }
}
