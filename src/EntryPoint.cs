
using System;
using System.IO;

namespace Tangerine {

    public class EntryPoint {

        public static int Main (string[] args) {
            string configFile;
            
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
                Daemon.ParseConfig (configFile);
            } catch (Exception e) {
                Console.Error.WriteLine ("Failed to parse configuration: " + e.Message);
                return 1;
            }
            
            Daemon.Run ();
            return 0;
        }
    }
}
