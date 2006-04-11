//
// Inotify.cs
//
// Copyright (C) 2004 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

// WARNING: This is not portable to Win32

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Mono.Unix;

namespace Tangerine {

    public class Inotify {

        public delegate void InotifyCallback (Watch watch, string path, string subitem, string srcpath, EventType type);

        public interface Watch {
            void Unsubscribe ();
            void ChangeSubscription (EventType new_mask);
        }
        /////////////////////////////////////////////////////////////////////////////////////

        [Flags]
        public enum EventType : uint {
            Access         = 0x00000001, // File was accessed
            Modify         = 0x00000002, // File was modified
            Attrib         = 0x00000004, // File changed attributes
            CloseWrite     = 0x00000008, // Writable file was closed
            CloseNoWrite   = 0x00000010, // Non-writable file was close
            Open           = 0x00000020, // File was opened
            MovedFrom      = 0x00000040, // File was moved from X
            MovedTo        = 0x00000080, // File was moved to Y
            Create         = 0x00000100, // Subfile was created
            Delete         = 0x00000200, // Subfile was deleted			
            DeleteSelf     = 0x00000400, // Self was deleted
            Unmount        = 0x00002000, // Backing fs was unmounted
            QueueOverflow  = 0x00004000, // Event queue overflowed
            Ignored        = 0x00008000, // File is no longer being watched

            IsDirectory    = 0x40000000, // Event is against a directory
            OneShot        = 0x80000000, // Watch is one-shot

            // For forward compatibility, define these explicitly
            All            = (EventType.Access | EventType.Modify | EventType.Attrib |
                              EventType.CloseWrite | EventType.CloseNoWrite | EventType.Open |
                              EventType.MovedFrom | EventType.MovedTo | EventType.Create |
                              EventType.Delete | EventType.DeleteSelf)
        }

        // Events that we want internally, even if the handlers do not
        static private EventType base_mask =  EventType.MovedFrom | EventType.MovedTo;

        /////////////////////////////////////////////////////////////////////////////////////

        [StructLayout (LayoutKind.Sequential)]
        private struct inotify_event {
            public int       wd;
            public EventType mask;
            public uint      cookie;
            public uint      len;
        }

        [DllImport ("libtangglue")]
        static extern int inotify_glue_init ();

        [DllImport ("libtangglue")]
        static extern int inotify_glue_watch (int fd, string filename, EventType mask);

        [DllImport ("libtangglue")]
        static extern int inotify_glue_ignore (int fd, int wd);

        [DllImport ("libtangglue")]
        static extern unsafe void inotify_snarf_events (int fd,
                                                        out int nr,
                                                        out IntPtr buffer);

        [DllImport ("libtangglue")]
        static extern void inotify_snarf_cancel ();

        /////////////////////////////////////////////////////////////////////////////////////

        static public bool Verbose = false;
        static private int inotify_fd = -1;

        static Inotify ()
        {
            if (Environment.GetEnvironmentVariable ("TANGERINE_DISABLE_INOTIFY") != null) {
                return;
            }

            if (Environment.GetEnvironmentVariable ("TANGERINE_INOTIFY_VERBOSE") != null)
                Inotify.Verbose = true;

            try {
                inotify_fd = inotify_glue_init ();
            } catch (EntryPointNotFoundException) {
                Daemon.Log.Error ("could not find inotify glue library");
                return;
            }

            if (inotify_fd == -1)
                Daemon.Log.Error ("failed to initialize inotify");

        }

        static public bool Enabled {
            get { return inotify_fd >= 0; }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        static private ArrayList event_queue = new ArrayList ();

        private class QueuedEvent {
            public int       Wd;
            public EventType Type;
            public string    Filename;
            public uint      Cookie;

            public bool        Analyzed;
            public bool        Dispatched;
            public DateTime    HoldUntil;
            public QueuedEvent PairedMove;

            // Measured in milliseconds; 57ms is totally random
            public const double DefaultHoldTime = 57;

            public QueuedEvent ()
            {
                // Set a default HoldUntil time
                HoldUntil = DateTime.Now.AddMilliseconds (DefaultHoldTime);
            }

            public void AddMilliseconds (double x)
            {
                HoldUntil = HoldUntil.AddMilliseconds (x);
            }

            public void PairWith (QueuedEvent other)
            {
                this.PairedMove = other;
                other.PairedMove = this;
				
                if (this.HoldUntil < other.HoldUntil)
                    this.HoldUntil = other.HoldUntil;
                other.HoldUntil = this.HoldUntil;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////

        private class WatchInternal : Watch {
            private InotifyCallback callback;
            private EventType mask;
            private WatchInfo watchinfo;
            private bool is_subscribed;

            public InotifyCallback Callback {
                get { return callback; }
            }

            public EventType Mask {
                get { return mask; }
                set { mask = value; }
            }

            public WatchInternal (InotifyCallback callback, EventType mask, WatchInfo watchinfo)
            {
                this.callback = callback;
                this.mask = mask;
                this.watchinfo = watchinfo;
                this.is_subscribed = true;
            }

            public void Unsubscribe ()
            {
                if (!this.is_subscribed)
                    return;

                Inotify.Unsubscribe (watchinfo, this);
                this.is_subscribed = false;
            }

            public void ChangeSubscription (EventType mask)
            {
                if (! this.is_subscribed)
                    return;

                this.mask = mask;
                CreateOrModifyWatch (this.watchinfo);
            }

        }

        private class WatchInfo {
            public int       Wd = -1;
            public string    Path;
            public bool      IsDirectory;
            public EventType Mask;

            public EventType FilterMask;
            public EventType FilterSeen;

            public ArrayList Children;
            public WatchInfo Parent;

            public ArrayList Subscribers;
        }

        static Hashtable watched_by_wd = new Hashtable ();
        static Hashtable watched_by_path = new Hashtable ();
        static WatchInfo last_watched = null;

        private class PendingMove {
            public WatchInfo Watch;
            public string    SrcName;
            public DateTime  Time;
            public uint      Cookie;

            public PendingMove (WatchInfo watched, string srcname, DateTime time, uint cookie) {
                Watch = watched;
                SrcName = srcname;
                Time = time;
                Cookie = cookie;
            }
        }

        static public int WatchCount {
            get { return watched_by_wd.Count; }
        }

        static public bool IsWatching (string path)
        {
            path = Path.GetFullPath (path);
            return watched_by_path.Contains (path);
        }

        // Filter WatchInfo items when we do the Lookup.
        // We do the filtering here to avoid having to acquire
        // the watched_by_wd lock yet again.
        static private WatchInfo Lookup (int wd, EventType event_type)
        {
            lock (watched_by_wd) {
                WatchInfo watched;
                if (last_watched != null && last_watched.Wd == wd)
                    watched = last_watched;
                else {
                    watched = watched_by_wd [wd] as WatchInfo;
                    if (watched != null)
                        last_watched = watched;
                }

                if (watched != null && (watched.FilterMask & event_type) != 0) {
                    watched.FilterSeen |= event_type;
                    watched = null;
                }

                return watched;
            }
        }

        // The caller has to handle all locking itself
        static private void Forget (WatchInfo watched)
        {
            if (last_watched == watched)
                last_watched = null;
            if (watched.Parent != null)
                watched.Parent.Children.Remove (watched);
            watched_by_wd.Remove (watched.Wd);
            watched_by_path.Remove (watched.Path);
        }

        static public Watch Subscribe (string path, InotifyCallback callback, EventType mask, EventType initial_filter)
        {
            WatchInternal watch;
            WatchInfo watched;
            EventType mask_orig = mask;

            if (!Path.IsPathRooted (path))
                path = Path.GetFullPath (path);

            bool is_directory = false;
            if (Directory.Exists (path))
                is_directory = true;
            else if (! File.Exists (path))
                throw new IOException (path);

            lock (watched_by_wd) {
                watched = watched_by_path [path] as WatchInfo;

                if (watched == null) {
                    // We need an entirely new WatchInfo object
                    watched = new WatchInfo ();
                    watched.Path = path;
                    watched.IsDirectory = is_directory;
                    watched.Subscribers = new ArrayList ();
                    watched.Children = new ArrayList ();
                    DirectoryInfo dir = new DirectoryInfo (path);
                    watched.Parent = watched_by_path [dir.Parent.ToString ()] as WatchInfo;
                    if (watched.Parent != null)
                        watched.Parent.Children.Add (watched);
                    watched_by_path [watched.Path] = watched;
                }

                watched.FilterMask = initial_filter;
                watched.FilterSeen = 0;

                watch = new WatchInternal (callback, mask_orig, watched);
                watched.Subscribers.Add (watch);

                CreateOrModifyWatch (watched);
                watched_by_wd [watched.Wd] = watched;
            }

            return watch;
        }
		
        static public Watch Subscribe (string path, InotifyCallback callback, EventType mask)
        {
            return Subscribe (path, callback, mask, 0);
        }

        static public EventType Filter (string path, EventType mask)
        {
            EventType seen = 0;

            path = Path.GetFullPath (path);

            lock (watched_by_wd) {
                WatchInfo watched;
                watched = watched_by_path [path] as WatchInfo;

                seen = watched.FilterSeen;
                watched.FilterMask = mask;
                watched.FilterSeen = 0;
            }

            return seen;
        }

        static private void Unsubscribe (WatchInfo watched, WatchInternal watch)
        {
            watched.Subscribers.Remove (watch);

            // Other subscribers might still be around			
            if (watched.Subscribers.Count > 0) {
                // Minimize it
                CreateOrModifyWatch (watched);
                return;
            }

            int retval = inotify_glue_ignore (inotify_fd, watched.Wd);
            if (retval < 0) {
                string msg = String.Format ("Attempt to ignore {0} failed!", watched.Path);
                throw new IOException (msg);
            }

            Forget (watched);
            return;
        }

        // Ensure our watch exists, meets all the subscribers requirements,
        // and isn't matching any other events that we don't care about.
        static private void CreateOrModifyWatch (WatchInfo watched)
        {
            EventType new_mask = base_mask;
            foreach (WatchInternal watch in watched.Subscribers)
                new_mask |= watch.Mask;

            if (watched.Wd >= 0 && watched.Mask == new_mask)
                return;

            // We rely on the behaviour that watching the same inode twice won't result
            // in the wd value changing.
            // (no need to worry about watched_by_wd being polluted with stale watches)
			
            int wd = -1;
            wd = inotify_glue_watch (inotify_fd, watched.Path, new_mask);
            if (wd < 0) {
                string msg = String.Format ("Attempt to watch {0} failed!", watched.Path);
                throw new IOException (msg);
            }
            if (watched.Wd >= 0 && watched.Wd != wd) {
                string msg = String.Format ("Watch handle changed unexpectedly!", watched.Path);	
                throw new IOException (msg);
            }

            watched.Wd = wd;
            watched.Mask = new_mask;
        }

        /////////////////////////////////////////////////////////////////////////////////////

        static Thread snarf_thread = null;
        static Thread dispatch_thread = null;
        static bool   running = false;

        static public void Start ()
        {
            if (! Enabled)
                return;

            lock (event_queue) {
                if (snarf_thread != null)
                    return;

                running = true;

                snarf_thread = new Thread (new ThreadStart (SnarfWorker));
                snarf_thread.Start ();

                dispatch_thread = new Thread (new ThreadStart (DispatchWorker));
                dispatch_thread.Start ();
            }
        }

        static public void Stop ()
        {
            if (! Enabled)
                return;

            lock (event_queue) {
                running = false;
                Monitor.Pulse (event_queue);
            }

            inotify_snarf_cancel ();
        }

        static unsafe void SnarfWorker ()
        {
            Encoding filename_encoding = Encoding.UTF8;
            int event_size = Marshal.SizeOf (typeof (inotify_event));

            while (running) {

                IntPtr buffer;
                int nr;

                // Will block while waiting for events, but with a 1s timeout.
                inotify_snarf_events (inotify_fd, 
                                      out nr,
                                      out buffer);

                if (!running)
                    break;

                if (nr == 0)
                    continue;

                ArrayList new_events = new ArrayList ();

                bool saw_overflow = false;
                while (nr > 0) {

                    // Read the low-level event struct from the buffer.
                    inotify_event raw_event;
                    raw_event = (inotify_event) Marshal.PtrToStructure (buffer, typeof (inotify_event));
                    buffer = (IntPtr) ((long) buffer + event_size);

                    if ((raw_event.mask & EventType.QueueOverflow) != 0)
                        saw_overflow = true;

                    // Now we convert our low-level event struct into a nicer object.
                    QueuedEvent qe = new QueuedEvent ();
                    qe.Wd = raw_event.wd;
                    qe.Type = raw_event.mask;
                    qe.Cookie = raw_event.cookie;

                    // Extract the filename payload (if any) from the buffer.
                    byte [] filename_bytes = new byte[raw_event.len];
                    Marshal.Copy (buffer, filename_bytes, 0, (int) raw_event.len);
                    buffer = (IntPtr) ((long) buffer + raw_event.len);
                    int n_chars = 0;
                    while (n_chars < filename_bytes.Length && filename_bytes [n_chars] != 0)
                        ++n_chars;
                    qe.Filename = "";
                    if (n_chars > 0)
                        qe.Filename = filename_encoding.GetString (filename_bytes, 0, n_chars);
                    
                    new_events.Add (qe);
                    nr -= event_size + (int) raw_event.len;
                }

                lock (event_queue) {
                    event_queue.AddRange (new_events);
                    Monitor.Pulse (event_queue);
                }
            }
        }


        // Update the watched_by_path hash and the path stored inside the watch
        // in response to a move event.
        static private void MoveWatch (WatchInfo watch, string name)		
        {
            lock (watched_by_wd) {

                watched_by_path.Remove (watch.Path);
                watch.Path = name;
                watched_by_path [watch.Path] = watch;
            }
        }

        // A directory we are watching has moved.  We need to fix up its path, and the path of
        // all of its subdirectories, their subdirectories, and so on.
        static private void HandleMove (string srcpath, string dstparent, string dstname)
        {
            string dstpath = Path.Combine (dstparent, dstname);
            lock (watched_by_wd) {

                WatchInfo start = watched_by_path [srcpath] as WatchInfo;	// not the same as src!
                if (start == null) {
                    return;
                }

                // Queue our starting point, then walk its subdirectories, invoking MoveWatch() on
                // each, repeating for their subdirectories.  The relationship between start, child
                // and dstpath is fickle and important.
                Queue queue = new Queue();
                queue.Enqueue (start);
                do {
                    WatchInfo target = queue.Dequeue () as WatchInfo;
                    for (int i = 0; i < target.Children.Count; i++) {
                        WatchInfo child = target.Children[i] as WatchInfo;

                        string name = Path.Combine (dstpath, child.Path.Substring (srcpath.Length + 1));
                        MoveWatch (child, name);
                        queue.Enqueue (child);
                    }
                } while (queue.Count > 0);
				
                // Ultimately, fixup the original watch, too
                MoveWatch (start, dstpath);
                if (start.Parent != null)
                    start.Parent.Children.Remove (start);
                start.Parent = watched_by_path [dstparent] as WatchInfo;
                if (start.Parent != null)
                    start.Parent.Children.Add (start);				
            }
        }

        static private void SendEvent (WatchInfo watched, string filename, string srcpath, EventType mask)
        {
            // Does the watch care about this event?
            if ((watched.Mask & mask) == 0)
                return;

            bool isDirectory = false;
            if ((mask & EventType.IsDirectory) != 0)
                isDirectory = true;

            if (watched.Subscribers == null)
                return;

            foreach (WatchInternal watch in watched.Subscribers)
                try {
                    if (watch.Callback != null && (watch.Mask & mask) != 0)
                        watch.Callback (watch, watched.Path, filename, srcpath, mask);
                } catch (Exception e) {
                    Daemon.LogError ("Exception in inotify callback", e);
                }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        // Dispatch-time operations on the event queue
	       
        static Hashtable pending_move_cookies = new Hashtable ();

        // Clean up the queue, removing dispatched objects.
        // We assume that the called holds the event_queue lock.
        static void CleanQueue_Unlocked ()
        {
            int first_undispatched = 0;
            while (first_undispatched < event_queue.Count) {
                QueuedEvent qe = event_queue [first_undispatched] as QueuedEvent;
                if (! qe.Dispatched)
                    break;
				
                if (qe.Cookie != 0)
                    pending_move_cookies.Remove (qe.Cookie);
				
                ++first_undispatched;
            }

            if (first_undispatched > 0)
                event_queue.RemoveRange (0, first_undispatched);

        }

        // Apply high-level processing to the queue.  Pair moves,
        // coalesce events, etc.
        // We assume that the caller holds the event_queue lock.
        static void AnalyzeQueue_Unlocked ()
        {
            int first_unanalyzed = event_queue.Count;
            while (first_unanalyzed > 0) {
                --first_unanalyzed;
                QueuedEvent qe = event_queue [first_unanalyzed] as QueuedEvent;
                if (qe.Analyzed) {
                    ++first_unanalyzed;
                    break;
                }
            }
            if (first_unanalyzed == event_queue.Count)
                return;

            // Walk across the unanalyzed events...
            for (int i = first_unanalyzed; i < event_queue.Count; ++i) {
                QueuedEvent qe = event_queue [i] as QueuedEvent;

                // Pair off the MovedFrom and MovedTo events.
                if (qe.Cookie != 0) {
                    if ((qe.Type & EventType.MovedFrom) != 0) {
                        pending_move_cookies [qe.Cookie] = qe;
                        // This increases the MovedFrom's HoldUntil time,
                        // giving us more time for the matching MovedTo to
                        // show up.
                        // (512 ms is totally arbitrary)
                        qe.AddMilliseconds (512); 
                    } else if ((qe.Type & EventType.MovedTo) != 0) {
                        QueuedEvent paired_move = pending_move_cookies [qe.Cookie] as QueuedEvent;
                        if (paired_move != null) {
                            paired_move.Dispatched = true;
                            qe.PairedMove = paired_move;
                        }
                    }
                }

                qe.Analyzed = true;
            }
        }

        static void DispatchWorker ()
        {
            while (running) {
                QueuedEvent next_event = null;

                // Until we find something we want to dispatch, we will stay
                // inside the following block of code.
                lock (event_queue) {

                    while (running) {
                        CleanQueue_Unlocked ();

                        AnalyzeQueue_Unlocked ();

                        // Now look for an event to dispatch.
                        DateTime min_hold_until = DateTime.MaxValue;
                        DateTime now = DateTime.Now;
                        foreach (QueuedEvent qe in event_queue) {
                            if (qe.Dispatched)
                                continue;
                            if (qe.HoldUntil <= now) {
                                next_event = qe;
                                break;
                            }
                            if (qe.HoldUntil < min_hold_until)
                                min_hold_until = qe.HoldUntil;
                        }

                        // If we found an event, break out of this block
                        // and dispatch it.
                        if (next_event != null)
                            break;
						
                        // If we didn't find an event to dispatch, we can sleep
                        // (1) until the next hold-until time
                        // (2) until the lock pulses (which means something changed, so
                        //     we need to check that we are still running, new events
                        //     are on the queue, etc.)
                        // and then we go back up and try to find something to dispatch
                        // all over again.
                        if (min_hold_until == DateTime.MaxValue)
                            Monitor.Wait (event_queue);
                        else
                            Monitor.Wait (event_queue, min_hold_until - now);
                    }
                }

                // If "running" gets set to false, we might get a null next_event as the above
                // loop terminates
                if (next_event == null)
                    return;

                // Now we have an event, so we release the event_queue lock and do
                // the actual dispatch.
				
                // Before we get any further, mark it
                next_event.Dispatched = true;

                WatchInfo watched;
                watched = Lookup (next_event.Wd, next_event.Type);
                if (watched == null)
                    continue;

                string srcpath = null;

                // If this event is a paired MoveTo, there is extra work to do.
                if ((next_event.Type & EventType.MovedTo) != 0 && next_event.PairedMove != null) {
                    WatchInfo paired_watched;
                    paired_watched = Lookup (next_event.PairedMove.Wd, next_event.PairedMove.Type);

                    if (paired_watched != null) {
                        // Set the source path accordingly.
                        srcpath = Path.Combine (paired_watched.Path, next_event.PairedMove.Filename);
						
                        // Handle the internal rename of the directory.
                        if ((next_event.Type & EventType.IsDirectory) != 0)
                            HandleMove (srcpath, watched.Path, next_event.Filename);
                    }
                }

                SendEvent (watched, next_event.Filename, srcpath, next_event.Type);

                // If a directory we are watching gets ignored, we need
                // to remove it from the watchedByFoo hashes.
                if ((next_event.Type & EventType.Ignored) != 0) {
                    lock (watched_by_wd)
                        Forget (watched);
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////

        #if INOTIFY_TEST
        static void Main (string [] args)
        {
            Queue to_watch = new Queue ();
            bool recursive = false;

            foreach (string arg in args) {
                if (arg == "-r" || arg == "--recursive")
                    recursive = true;
                else {
                    // Our hashes work without a trailing path delimiter
                    string path = arg.TrimEnd ('/');
                    to_watch.Enqueue (path);
                }
            }

            while (to_watch.Count > 0) {
                string path = (string) to_watch.Dequeue ();

                Console.WriteLine ("Watching {0}", path);
                Inotify.Subscribe (path, null, Inotify.EventType.All);

                if (recursive) {
                    foreach (string subdir in DirectoryWalker.GetDirectories (path))
                        to_watch.Enqueue (subdir);
                }
            }

            Inotify.Start ();
            Inotify.Verbose = true;

            while (Inotify.Enabled && Inotify.WatchCount > 0)
                Thread.Sleep (1000);

            if (Inotify.WatchCount == 0)
                Console.WriteLine ("Nothing being watched.");

            // Kill the event-reading thread so that we exit
            Inotify.Stop ();
        }
        #endif
    }
}
