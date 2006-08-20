
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using log4net;

namespace Tangerine {

    public class PluginAttribute : Attribute {

        private string name;

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public PluginAttribute (string name) {
            this.name = name;
        }
    }

    public class PluginManager {

        private static ArrayList plugins = new ArrayList ();
        private static ILog log = LogManager.GetLogger (typeof (Daemon));

        public static void LoadPlugins (string[] names) {
            LoadPlugins (names, Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "plugins"));
            LoadPlugins (names, Path.Combine (Daemon.ConfigDirectory, "plugins"));

#if DEBUG
            LoadPlugins (names, AppDomain.CurrentDomain.BaseDirectory);
#endif

            if (plugins.Count == 0)
                Daemon.Log.Warn ("No plugins were loaded");
        }

        public static void LoadPlugins (string[] names, string dir) {
            if (!Directory.Exists (dir)) {
                return;
            }
            
            foreach (string file in Directory.GetFiles (dir, "*.dll")) {
                try {
                    Assembly asm = Assembly.LoadFrom (file);
                    foreach (Type type in asm.GetTypes ()) {
                        PluginAttribute attr = Attribute.GetCustomAttribute (type, typeof (PluginAttribute)) as PluginAttribute;
                        
                        if (attr == null)
                            continue;

                        if (names == null || names.Length == 0 || Array.IndexOf (names, attr.Name) >= 0) {
                            plugins.Add (Activator.CreateInstance (type));
                            Daemon.Log.InfoFormat ("Loaded plugin '{0}'", attr.Name);
                        }
                    }
                } catch (Exception e) {
                    Daemon.LogError (String.Format ("Failed to load '{0}'", file), e);
                }
            }
        }

        public static void UnloadPlugins () {
            foreach (object o in plugins) {
                IDisposable disp = o as IDisposable;

                if (disp != null) {
                    try {
                        disp.Dispose ();
                    } catch (Exception e) {
                        Daemon.LogError ("Failed to unload plugin", e);
                    }
                }
            }
        }
    }
}
