using System;
using System.Threading;
using System.Collections.Generic;

namespace Tangerine {

    public class CreationWatcher : IDisposable {

        private string dir;
        private object watcherLock = new object ();
        private bool disposed = false;

        public event EventHandler Created;

        public string Directory {
            get { return dir; }
        }
        
        public CreationWatcher (string dir) {
            this.dir = dir;

            Thread thread = new Thread (WatcherLoop);
            thread.Start ();
        }

        public void Dispose () {
            if (disposed)
                return;
            
            disposed = true;
            lock (watcherLock) {
                Monitor.Pulse (watcherLock);
            }
        }

        private void WatcherLoop () {
            lock (watcherLock) {
                while (!disposed) {
                    Monitor.Wait (watcherLock, TimeSpan.FromMinutes (1));
                    if (disposed)
                        break;

                    if (System.IO.Directory.Exists (dir)) {
                        EventHandler handler = Created;
                        if (handler != null)
                            handler (this, new EventArgs ());

                        break;
                    }
                }
            }
        }
    }
}
