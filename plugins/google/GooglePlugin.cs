using System;
using System.Collections.Generic;
using System.Text;
using GoogleDesktopQueryAPILib;
using GoogleDesktopAPILib;
using Microsoft.Win32;
using DAAP;

[assembly: Tangerine.Plugin ("google", typeof (Tangerine.Plugins.GooglePlugin))]

namespace Tangerine.Plugins {

    public class GooglePlugin : IDisposable {
        private const string RegkeyPath = @"Software\Tangerine";
        
        public GooglePlugin () {
            int cookie;

            RegistryKey key = Registry.CurrentUser.OpenSubKey (RegkeyPath, true);
            if (key == null) {
                key = Registry.CurrentUser.CreateSubKey (RegkeyPath);
            }

            object val = key.GetValue ("google-cookie");
            if (val == null) {
                string guid = "{" + Guid.NewGuid ().ToString () + "}";
                GoogleDesktopRegistrar reg = new GoogleDesktopRegistrarClass ();
                reg.StartComponentRegistration (guid, new object[] {
                    "Title", "Tangerine Google Plugin",
                    "Description", "Google plugin for the Tangerine music server",
                    "Icon", ""
                });

                IGoogleDesktopRegisterQueryPlugin qreg = (IGoogleDesktopRegisterQueryPlugin) reg.GetRegistrationInterface ("GoogleDesktop.QueryRegistration");
                cookie = qreg.RegisterPlugin (guid, true);
                reg.FinishComponentRegistration ();

                key.SetValue ("google-cookie", cookie);
            } else {
                cookie = (int) val;
            }

            GoogleDesktopQueryAPIClass query = new GoogleDesktopQueryAPIClass ();
            IGoogleDesktopQueryResultSet results = query.Query (cookie, "|filetype:mp3 |filetype:aac |filetype:m4a |filetype:m4b |filetype:wma", "file", 1);
           
            IGoogleDesktopQueryResultItem item;
            while ((item = results.Next ()) != null) {
                if (item.schema == "Google.Desktop.MediaFile") {
                    try {
                        AddTrack (item);
                    } catch (Exception e) {
                    }
                }
            }
        }

        private object GetResultProperty (IGoogleDesktopQueryResultItem item, string name) {
            try {
                return item.GetProperty (name);
            } catch (Exception e) {
                return null;
            }
        }

        private void AddTrack (IGoogleDesktopQueryResultItem item) {
            Uri uri = new Uri ((string) item.GetProperty ("uri"));

            string artist = (string) GetResultProperty (item, "artist");
            string title = (string) GetResultProperty (item, "title");
            string album = (string) GetResultProperty (item, "album_title");

            ulong duration = 0;
            object o = GetResultProperty (item, "length");
            if (o != null)
                duration = (ulong) o;

            uint bitrate = 0;
            o = GetResultProperty (item, "bit_rate");
            if (o != null)
                bitrate = (uint) o;

            string genre = (string) GetResultProperty (item, "genre");

            uint trackNum = 0;
            o = GetResultProperty (item, "track_number");
            if (o != null)
                trackNum = (uint) o;

            if (artist == null || title == null)
                return;

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
