
using System;
using System.IO;
using System.Collections;
using System.Threading;
using Gnome;

namespace Tangerine.Plugins {

    [Plugin ("session")]
    public class SessionPlugin : IDisposable {

        public SessionPlugin () {
            Program program = new Program ("tangerine", "1.0", Gnome.Modules.UI, new string[0]);
        
            Client client = new Client ();

            client.Die += delegate {
                Console.WriteLine ("Die!");
                Daemon.Stop ();
            };
        }

        public void Dispose () {
        }
    }
}
