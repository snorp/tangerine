
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using log4net.Appender;
using Nini.Config;
using DAAP;

#if !WINDOWS
using Mono.Unix;
using Mono.Unix.Native;
#endif

namespace Tangerine {

    // not really a daemon, but whatever
    public class Daemon {

        private static Server server;
        private static Database db;
        private static ILog log = LogManager.GetLogger (typeof (Daemon));
        private static IniConfigSource cfgSource;
        private static Regex nameRegex = new Regex (@"(.*?).\[([0-9]*)\]$");

        public static string Name;
        public static string PasswordFile;
        public static bool Debug;
        public static int MaxUsers;
        public static string LogFile;
        public static ushort Port;
        public static bool IsPublished;
        public static string[] PluginNames;
        public static string ConfigPath;

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

        public static void ParseConfig () {
            cfgSource = new IniConfigSource ();

            if (File.Exists (ConfigPath)) {
                cfgSource.Load (ConfigPath);
            }

            IConfig cfg = cfgSource.Configs["Tangerine"];
            if (cfg == null) {
                cfg = cfgSource.AddConfig ("Tangerine");
            }
            
            cfg.Alias.AddAlias ("yes", true);
            cfg.Alias.AddAlias ("true", true);
            cfg.Alias.AddAlias ("no", false);
            cfg.Alias.AddAlias ("false", false);

            Name = cfg.Get ("name", String.Format ("{0}'s Music", Environment.UserName));
            PasswordFile = cfg.Get ("password_file");
            Debug = cfg.GetBoolean ("debug", false);
            MaxUsers = cfg.GetInt ("max_users", 0);
            LogFile = cfg.Get ("log_file");
            Port = (ushort) cfg.GetInt ("port", 0);
            IsPublished = (bool) cfg.GetBoolean ("publish", true);
            string names = cfg.Get ("plugins", "file,session");

            if (names != null)
                PluginNames = names.Split(',');
        }

        public static bool IsSaveNeeded () {
            CommitConfig ();

            if (!File.Exists (ConfigPath))
                return true;
            
            IniConfigSource old = new IniConfigSource (ConfigPath);

            foreach (IConfig config in cfgSource.Configs) {
                IConfig oldConfig = old.Configs[config.Name];
                if (oldConfig == null)
                    return true;

                string[] keys = config.GetKeys ();
                if (keys.Length != oldConfig.GetKeys ().Length)
                    return true;
                
                foreach (string key in keys) {
                    if (config.Get (key) != oldConfig.Get (key)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void CommitConfig () {
            IConfig cfg = cfgSource.Configs["Tangerine"];
            cfg.Set ("name", Name);
            cfg.Set ("password_file", PasswordFile);
            cfg.Set ("debug", Debug);
            cfg.Set ("max_users", MaxUsers);
            cfg.Set ("log_file", LogFile);
            cfg.Set ("port", Port);
            cfg.Set ("publish", IsPublished);

            StringBuilder plugins = new StringBuilder ();
            foreach (string plugin in PluginNames) {
                if (plugin != PluginNames[0])
                    plugins.Append (",");

                plugins.Append (plugin);
            }

            cfg.Set ("plugins", plugins.ToString ());
        }

        public static void SaveConfig () {
            CommitConfig ();
            cfgSource.Save (ConfigPath);
        }

        public static void Run () {
            InitializeLogging ();

            if (!File.Exists (ConfigPath)) {
                log.WarnFormat ("Config file '{0}' was not found, using defaults", ConfigPath);
            }
            
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

#if !WINDOWS
            UnixSignal.RegisterHandler (Signum.SIGTERM, OnSignal);
            UnixSignal.RegisterHandler (Signum.SIGINT, OnSignal);
            UnixSignal.Start ();
#endif
            
            server = new Server (Name);
            server.Port = Port;
            server.IsPublished = IsPublished;
            server.Collision += OnCollision;
            db = new Database (Name);
            server.AddDatabase (db);
            server.MaxUsers = MaxUsers;

            server.TrackRequested += OnTrackRequested;
            
            log.Info ("Server name: " + Name);

            PluginManager.LoadPlugins (PluginNames);

#if !WINDOWS
            if (Inotify.Enabled) {
                Inotify.Start ();
            }
#endif
            
            AddUsers ();

            try {
                server.Start ();
            } catch (Exception e) {
                LogError ("Failed to start server", e);
                Shutdown ();
            }

            RunLoop ();
            Shutdown ();
        }

        private static void Shutdown () {
            log.Warn ("Shutting down");

#if !WINDOWS
            if (Inotify.Enabled) {
                Inotify.Stop ();
            }
#endif
                
            PluginManager.UnloadPlugins ();
            
#if !WINDOWS
            UnixSignal.Stop ();

            server.Stop ();

            Syscall.exit (0);
#endif
        }

        private static void OnTrackRequested (object o, TrackRequestedArgs args) {
            if (args.UserName == null) {
                log.DebugFormat ("Host '{0}' requested song '{1}'", args.Host, args.Track.Title);
            } else {
                log.DebugFormat ("Host '{0}' ({1}) requested song '{2}'", args.Host, args.UserName, args.Track.Title);
            }
        }

        public static void Stop () {
            if (loop != null && loop.IsRunning) {
                QuitLoop ();
            } else {
                // haven't finished starting up yet
                // might be hung or something, so just die.

                Environment.Exit (0);
            }
        }

#if !WINDOWS
        private static void OnSignal (Signum sig) {
            if (sig == Signum.SIGTERM || sig == Signum.SIGINT) {
                Stop ();
            }
        }
#endif

        private static IAppender GetConsoleAppender (ILayout layout) {
#if WINDOWS
            ConsoleAppender appender = new ConsoleAppender ();
            appender.Layout = layout;
            return appender;
#else
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
#endif
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

            if (Debug) {
                l.Level = Level.Debug;
            } else {
                l.Level = Level.Info;
            }

            PatternLayout layout = new PatternLayout ();
            layout.ConversionPattern = "%date %level %message%newline";
            layout.ActivateOptions ();

            l.AddAppender (GetConsoleAppender (layout));

            if (LogFile != null)
                l.AddAppender (GetFileAppender (layout));

            h.Configured = true;

            log.InfoFormat ("Tangerine started");
        }
        
        private static void AddUsers () {
            if (PasswordFile == null || !File.Exists (PasswordFile))
                return;

            try {
                using (StreamReader reader = new StreamReader (File.Open (PasswordFile, FileMode.Open, FileAccess.Read))) {
                    string line;
                    
                    while ((line = reader.ReadLine ()) != null) {
                        string[] splitLine = line.Split (':');
                        if (splitLine.Length < 2) {
                            server.AddCredential (new NetworkCredential (null, line));
                            server.AuthenticationMethod = AuthenticationMethod.Password;
                        } else {
                            server.AddCredential (new NetworkCredential (splitLine[0], splitLine[1]));
                            server.AuthenticationMethod = AuthenticationMethod.UserAndPassword;
                        }
                    }
                }

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

        private static string GetNextServiceName (string name) {
            Match match = nameRegex.Match (name);
            if (!match.Success) {
                return name + " [1]";
            } else {
                return String.Format ("{0} [{1}]", match.Groups[1].Value,
                                      Int32.Parse (match.Groups[2].Value) + 1);
            }
        }

        private static void OnCollision (object o, EventArgs args) {
            string name = GetNextServiceName (server.Name);
            log.WarnFormat ("The name '{0}' collided with another on the network, trying '{1}'",
                            server.Name, name);
            server.Name = name;
        }

#if !WINDOWS

        private static GLib.MainLoop loop = new GLib.MainLoop ();

        private static void RunLoop () {
            loop.Run ();
        }

        private static void QuitLoop () {
            loop.Quit ();
        }
#else

        private static object loopLock = new object ();

        private static void RunLoop () {
            lock (loopLock) {
                Monitor.Wait (loopLock);
            }
        }

        private static void QuitLoop() {
            lock (loopLock) {
                Monitor.Pulse (loopLock);
            }
        }
#endif
    }
}
