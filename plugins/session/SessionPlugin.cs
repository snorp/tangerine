
using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;

[assembly: Tangerine.Plugin ("session", typeof (Tangerine.Plugins.SessionPlugin))]

namespace Tangerine.Plugins {

    public class SessionPlugin : IDisposable {

        [DllImport ("libsessionglue")]
        private static extern void run_session ();

        [DllImport ("libsessionglue")]
        private static extern void close_session ();
        
        public SessionPlugin () {
            Thread thread = new Thread (run_session);
            thread.IsBackground = true;
            thread.Start ();
        }

        public void Dispose () {
            close_session ();
        }
    }
}
