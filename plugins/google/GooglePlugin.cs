using System;
using System.Collections.Generic;
using System.Text;
using GoogleDesktopQueryAPILib;
using GoogleDesktopAPILib;
using Microsoft.Win32;
using DAAP;

namespace Tangerine.Plugins {

    [Plugin ("google")]
    public class GooglePlugin : IDisposable {
        
        public GooglePlugin () {
            int cookie;

            RegistryKey key = Registry.CurrentUser.OpenSubKey ("tangerine-google", true);
            if (key == null) {
                key = Registry.CurrentUser.CreateSubKey ("tangerine-google");
            }

            object val = key.GetValue ("cookie");
            if (val == null) {
                string guid = "{8534E033-3D7C-4753-AC8C-35B289607165}";
                GoogleDesktopRegistrar reg = new GoogleDesktopRegistrarClass ();
                reg.StartComponentRegistration (guid, new object[] {
                    "Title", "Tangerine Google Plugin",
                    "Description", "Google plugin for the Tangerine music server",
                    "Icon", ""
                });

                IGoogleDesktopRegisterQueryPlugin qreg = (IGoogleDesktopRegisterQueryPlugin) reg.GetRegistrationInterface ("GoogleDesktop.QueryRegistration");
                cookie = qreg.RegisterPlugin (guid, true);
                reg.FinishComponentRegistration ();

                Console.WriteLine ("Got cookie: " + cookie);
                key.SetValue ("cookie", cookie);
            } else {
                cookie = (int) val;
            }

            GoogleDesktopQueryAPIClass query = new GoogleDesktopQueryAPIClass ();
            IGoogleDesktopQueryResultSet results = query.Query (cookie, "mp3", "file", 1);

            IGoogleDesktopQueryResultItem item;
            while ((item = results.Next ()) != null) {
                if (item.schema == "Google.Desktop.MediaFile") {
                    try {
                        AddTrack (item);
                    } catch (Exception e) {
                        Console.WriteLine ("Busted: " + e.Message);
                    }
                }
            }
        }

        private void AddTrack (IGoogleDesktopQueryResultItem item) {
            Uri uri = new Uri ((string) item.GetProperty ("uri"));

            string artist = (string) item.GetProperty ("artist");
            string title = (string) item.GetProperty ("title");
            string album = (string) item.GetProperty ("album_title");
            ulong duration = (ulong) item.GetProperty ("length");
            uint bitrate = (uint) item.GetProperty ("bit_rate");
            string genre = (string) item.GetProperty ("genre");
            uint trackNum = (uint) item.GetProperty ("track_number");

            Track track = new Track ();
            track.Artist = artist;
            track.Album = album;
            track.Title = title;
            track.FileName = uri.LocalPath;
            track.Duration = TimeSpan.FromMilliseconds ((double) duration / (double) 10000);
            track.BitRate = (short) bitrate;
            track.Genre = genre;
            track.TrackNumber = (int) trackNum;

            Daemon.DefaultDatabase.AddTrack (track);
        }

        public void Dispose () {
        }
    }
}
