using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using DAAP;

[assembly: Tangerine.Plugin ("system-tray", typeof (Tangerine.Plugins.SystemTrayPlugin))]

namespace Tangerine.Plugins {

    public class SystemTrayPlugin : IDisposable {
        private const string DontAskRegKey = @"Software\Tangerine";
        private NotifyIcon icon;

        public SystemTrayPlugin () {
            icon = new NotifyIcon ();
            icon.Icon = new Icon (typeof (SystemTrayPlugin), "tangerine.ico");

            MenuItem prefsItem = new MenuItem ("Preferences...", delegate {
                Process.Start ("tangerine-preferences.exe");
            });

            MenuItem quitItem = new MenuItem ("Quit", OnQuit);

                

            icon.ContextMenu = new ContextMenu (new MenuItem[] { prefsItem, quitItem });
            icon.Visible = true;

            UpdateIconText ();

            Daemon.Server.UserLogin += delegate {
                UpdateIconText ();
            };

            Daemon.Server.UserLogout += delegate {
                UpdateIconText ();
            };
        }

        private void OnQuit (object o, EventArgs args) {
            RegistryKey key = Registry.CurrentUser.OpenSubKey (DontAskRegKey, true);
            if (key == null) {
                key = Registry.CurrentUser.CreateSubKey (DontAskRegKey);
            }

            if ((int) key.GetValue ("NoAskQuit", 0) == 1) {
                Daemon.Stop ();
                return;
            }
               
            Tangerine.src.QuitDialog dialog = new Tangerine.src.QuitDialog ();
            DialogResult result = dialog.ShowDialog ();
            if (dialog.NoAsk) {
                key.SetValue ("NoAskQuit", 1);
            }

            if (result == DialogResult.OK) {
                Daemon.DisableAutostart ();
            }

            Daemon.Stop ();
        }

        private void UpdateIconText () {
            if (icon != null) {
                icon.Text = String.Format ("{0}: {1} users", Daemon.Server.Name,
                                           Daemon.Server.Users.Count);
            }
        }

        public void Dispose () {
            if (icon != null) {
                icon.Dispose ();
                icon = null;
            }
        }
    }
}
