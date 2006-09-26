using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using DAAP;

namespace Tangerine.Plugins {

    [Plugin ("system-tray")]
    public class SystemTrayPlugin : IDisposable {

        private NotifyIcon icon;

        public SystemTrayPlugin () {
            icon = new NotifyIcon ();
            icon.Icon = new Icon (typeof (SystemTrayPlugin), "tangerine.ico");

            MenuItem prefsItem = new MenuItem ("Preferences...", delegate {
                Process.Start ("tangerine-preferences.exe");
            });

            MenuItem quitItem = new MenuItem ("Quit", delegate {
                Daemon.Stop ();
            });

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
