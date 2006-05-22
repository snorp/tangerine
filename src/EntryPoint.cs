
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Tangerine {

    public class EntryPoint {
 
        [DllImport("libc")]
        private static extern int prctl(int option, byte [] arg2, ulong arg3,
                                        ulong arg4, ulong arg5);

        private static void SetProcessName(string name) {
            try {
                prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(name + "\0"), 0, 0, 0);
            } catch {
            }
        }

        public static int Main (string[] args) {
            string configFile;

            SetProcessName ("tangerine");
            
            if (args.Length > 0) {
                if (args[0] == "-h" || args[0] == "--help") {
                    Console.WriteLine ("Usage: tangerine [<config>]");
                    Console.WriteLine ("If no config file is specified, ~/.tangerine is used");
                    return 0;
                } else {
                    configFile = args[0];
                }
            } else {
                configFile = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                                           ".tangerine");
            }

            try {
                Daemon.ConfigPath = configFile;
                Daemon.ParseConfig ();
            } catch (Exception e) {
                Console.Error.WriteLine ("Failed to parse configuration: " + e);
                return 1;
            }
            
            Daemon.Run ();
            return 0;
        }
    }
}
