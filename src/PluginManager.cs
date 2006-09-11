
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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

        private static List<Assembly> pluginAssemblies = new List<Assembly> ();
        private static ArrayList plugins = new ArrayList ();

        public static string PluginDirectory {
            get {
                return Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "plugins");
            }
        }

        public static void LoadPlugins (string[] names) {
            LoadPlugins (names, PluginDirectory);

#if DEBUG
            LoadPlugins (names, AppDomain.CurrentDomain.BaseDirectory);
#endif

            if (plugins.Count == 0)
                Daemon.Log.Warn ("No plugins were loaded");
        }

        private static void LoadPluginAssemblies (string dir) {
            if (!Directory.Exists (dir)) {
                return;
            }
            
            foreach (string file in Directory.GetFiles (dir, "*.dll")) {
                try {
                    pluginAssemblies.Add (Assembly.LoadFrom (file));
                } catch (Exception e) {
                    Daemon.LogError (String.Format ("Failed to load plugin assembly '{0}'", file), e);
                }
            }
        }

        public static void LoadPlugins (string[] names, string dir) {
            LoadPluginAssemblies (dir);

            foreach (Assembly asm in pluginAssemblies) {
                foreach (Type type in asm.GetTypes ()) {
                    PluginAttribute attr = Attribute.GetCustomAttribute (type, typeof (PluginAttribute)) as PluginAttribute;

                    if (attr == null)
                        continue;
                    
                    if (names == null || names.Length == 0 || Array.IndexOf (names, attr.Name) >= 0) {
                        try {
                            plugins.Add (Activator.CreateInstance (type));
                            Daemon.Log.InfoFormat ("Loaded plugin '{0}'", attr.Name);
                        } catch (Exception e) {
                            Daemon.LogError (String.Format ("Failed to load '{0}'", attr.Name), e);
                        }
                    }
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
