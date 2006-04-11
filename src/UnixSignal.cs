/*
 * Copyright (c) 2005 Novell, Inc. All Rights Reserved.
 *
 * This program is free software; you can redistribute it and/or 
 * modify it under the terms of version 2 of the GNU General Public License
 * as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, contact Novell, Inc.
 *
 * To contact Novell about this file by physical or electronic mail, 
 * you may find current contact information at www.novell.com.
 *
 */


using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace Tangerine {

    public delegate void SignalHandler (Signum sig);
    public delegate void SignalRestorer ();

    [StructLayout(LayoutKind.Sequential)]
    internal struct SigActionData {
        public SignalHandler handler;
        public UInt64 flags;
        public SignalRestorer restorer;
        public UInt64 mask; // The actual struct has one long[2] type here but this works.
        public UInt64 mask2;
        
        public SigActionData (SignalHandler handler) {
            this.handler = handler;
            flags = 0;
            restorer = null;
            mask = 0;
            mask2 = 0;
        }
    }

    public class UnixSignal {

        private static bool dummy;
        private static Signum lastSignal;
        private static Hashtable handlers = new Hashtable ();
        private static bool stopped = true;
        private static object signalLock = new object ();

        [DllImport ("libc")]
        private static extern int sigaction (int signum, ref SigActionData action, IntPtr blah);

        static UnixSignal () {
            // JIT the handler method now
            dummy = true;
            InternalHandler (0);
            dummy = false;
        }

        private static void SignalEventLoop () {
            while (true) {
                lock (signalLock) {
                    Monitor.Wait (signalLock, TimeSpan.FromSeconds (1));
                }

                if (stopped)
                    break;

                Signum sig = lastSignal;
                lastSignal = 0;

                try {
                    if ((int) sig > 0 && handlers[sig] != null) {
                        SignalHandler handler = (SignalHandler) handlers[sig];
                        handler (sig);
                    }
                } catch (Exception e) {
                    Console.Error.WriteLine ("Exception in signal handler: " + e);
                }
            }
        }
        
        private static void InternalHandler (Signum sig) {
            if (dummy)
                return;

            lastSignal = sig;
        }
        
        public static void RegisterHandler (Signum signal,
                                            SignalHandler handler) {
            handlers[signal] = handler;
            
            SigActionData data = new SigActionData (new SignalHandler (InternalHandler));
            sigaction ((int) signal, ref data, IntPtr.Zero);
        }

        public static void Start () {
            if (stopped) {
                stopped = false;
                Thread eventThread = new Thread (new ThreadStart (SignalEventLoop));
                eventThread.IsBackground = true;
                eventThread.Start ();
            }
        }

        public static void Stop () {
            if (!stopped) {
                stopped = true;

                lock (signalLock) {
                    Monitor.Pulse (signalLock);
                }
            }
        }
    }
}
