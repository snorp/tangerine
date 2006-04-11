
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using log4net.Appender;
using Nini.Config;
using Mono.Unix;
using Mono.Unix.Native;
using DAAP;
using Avahi;

namespace Tangerine {

    // not really a daemon, but whatever
    public class Daemon {

        private static Server server;
        private static Database db;
        private static ILog log = LogManager.GetLogger (typeof (Daemon));
        private static IConfigSource cfgSource;
        private static IntPtr loop;
        
        public static string Name;
        public static string PasswordFile;
        public static bool Debug;
        public static int MaxUsers;
        public static string LogFile;
        public static ushort Port;
        public static string[] PluginNames;

        public static ILog Log {
            get { return log; }
            set { log = value; }
        }

        public static IConfigSource ConfigSource {
            get { return cfgSource; }
        }

        public static string ConfigDirectory {
            get {
                string datadir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
                return Path.Combine (datadir, "tangerine");
            }
        }

        public static Server Server {
            get { return server; }
        }

        public static Database DefaultDatabase {
            get { return db; }
        }

        public static void ParseConfig (string file) {
            cfgSource = new IniConfigSource (file);

            IConfig cfg = cfgSource.Configs["Tangerine"];
            cfg.Alias.AddAlias ("yes", true);
            cfg.Alias.AddAlias ("true", true);
            cfg.Alias.AddAlias ("no", false);
            cfg.Alias.AddAlias ("false", false);

            Name = cfg.Get ("name", String.Format ("{0}'s Tangerine", Environment.UserName));
            PasswordFile = cfg.Get ("password_file");
            Debug = cfg.GetBoolean ("debug", false);
            MaxUsers = cfg.GetInt ("max_users", 0);
            LogFile = cfg.Get ("log_file");
            Port = (ushort) cfg.GetInt ("port", 0);
            string names = cfg.Get ("plugins");

            if (names != null)
                PluginNames = names.Split(',');
        }

        public static void Run () {
            InitializeLogging ();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            UnixSignal.RegisterHandler (Signum.SIGTERM, OnSignal);
            UnixSignal.RegisterHandler (Signum.SIGINT, OnSignal);
            UnixSignal.Start ();
            
            server = new Server (Name);
            server.Port = Port;
            server.Collision += OnCollision;
            db = new Database (Name);
            server.AddDatabase (db);
            server.MaxUsers = MaxUsers;

            log.Info ("Server name: " + Name);

            PluginManager.LoadPlugins (PluginNames);

            if (Inotify.Enabled) {
                Inotify.Start ();
            }
            
            AddUsers ();

            server.Start ();

            RunLoop ();
            log.Warn ("Shutting down");

            if (Inotify.Enabled) {
                Inotify.Stop ();
            }
                
            PluginManager.UnloadPlugins ();
            
            UnixSignal.Stop ();
            server.Stop ();
        }

        public static void Stop () {
            QuitLoop ();
        }

        private static void OnSignal (Signum sig) {
            if (sig == Signum.SIGTERM || sig == Signum.SIGINT) {
                Stop ();
            }
        }

        private static IAppender GetConsoleAppender (ILayout layout) {
            AnsiColorTerminalAppender appender = new AnsiColorTerminalAppender ();
            appender.Layout = layout;

            // error = red
            AnsiColorTerminalAppender.LevelColors colors = new AnsiColorTerminalAppender.LevelColors ();
            colors.ForeColor = AnsiColorTerminalAppender.AnsiColor.Red;
            colors.Level = Level.Error;
            appender.AddMapping (colors);

            // warning = yellow
            colors = new AnsiColorTerminalAppender.LevelColors ();
            colors.ForeColor = AnsiColorTerminalAppender.AnsiColor.Yellow;
            colors.Attributes = AnsiColorTerminalAppender.AnsiAttributes.Bright;
            colors.Level = Level.Warn;
            appender.AddMapping (colors);
            
            appender.ActivateOptions ();
            return appender;
        }

        private static IAppender GetFileAppender (ILayout layout) {
            RollingFileAppender appender = new RollingFileAppender ();
            appender.Layout = layout;
            appender.File = Path.Combine (Environment.CurrentDirectory, LogFile);
            appender.ActivateOptions ();
            return appender;
        }

        private static void InitializeLogging () {
            Hierarchy h = LogManager.GetRepository () as Hierarchy;

            Logger l = h.Root;
            l.Level = Level.Debug;

            PatternLayout layout = new PatternLayout ();
            layout.ConversionPattern = "%date %level %message%newline";
            layout.ActivateOptions ();

            l.AddAppender (GetConsoleAppender (layout));

            if (LogFile != null)
                l.AddAppender (GetFileAppender (layout));

            h.Configured = true;

            log.InfoFormat ("Tangerine started, version {0}", Assembly.GetExecutingAssembly ().GetName ().Version);
        }
        
        private static void AddUsers () {
            if (PasswordFile == null || !File.Exists (PasswordFile))
                return;

            try {
                using (StreamReader reader = new StreamReader (File.Open (PasswordFile, FileMode.Open, FileAccess.Read))) {
                    string line;
                    
                    while ((line = reader.ReadLine ()) != null) {
                        string[] splitLine = line.Split (':');
                        if (splitLine.Length != 2)
                            continue;

                        server.AddCredential (new NetworkCredential (splitLine[0], splitLine[1]));
                    }
                }

                server.AuthenticationMethod = AuthenticationMethod.UserAndPassword;
            } catch (Exception e) {
                LogError ("Problem adding users", e);
            }
        }

        public static void LogError (string msg, Exception e) {
            if (e == null) {
                log.Error (msg);
                return;
            }

            log.ErrorFormat ("{0}: {1}", msg, Debug ? e.ToString () : e.Message);
        }

        private static void OnUnhandledException (object o, UnhandledExceptionEventArgs args) {
            Console.WriteLine (args.ExceptionObject);
            LogError ("Unhandled Exception", (Exception) args.ExceptionObject);

            if (args.IsTerminating) {
                log.Error ("Exiting, due to unhandled exception");
            }
        }

        private static void OnCollision (object o, EventArgs args) {
            string name = EntryGroup.GetAlternativeServiceName (server.Name);

            log.WarnFormat ("The name '{0}' collided with another on the network, trying '{1}'",
                            server.Name, name);
            server.Name = name;
        }

        [DllImport ("glib-2.0")]
        private static extern IntPtr g_main_loop_new (IntPtr context, bool running);

        [DllImport ("glib-2.0")]
        private static extern void g_main_loop_run (IntPtr loop);

        [DllImport ("glib-2.0")]
        private static extern void g_main_loop_quit (IntPtr loop);

        private static void RunLoop () {
            loop = g_main_loop_new (IntPtr.Zero, false);
            g_main_loop_run (loop);
        }

        private static void QuitLoop () {
            g_main_loop_quit (loop);
        }
    }
}
