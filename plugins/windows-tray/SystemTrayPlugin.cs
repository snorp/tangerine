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
            icon.Text = Daemon.Server.Name;
            icon.Visible = true;
        }

        public void Dispose () {
            if (icon != null) {
                icon.Dispose ();
                icon = null;
            }
        }
    }
}
