
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace Tangerine {

    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginAttribute : Attribute {

        private string name;
        private Type type;

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public Type Type {
            get { return type; }
            set { type = value; }
        }

        public PluginAttribute (string name, Type type) {
            this.name = name;
            this.type = type;
        }
    }

    public class PluginManager {

        private static Dictionary<string, Type> pluginTypes = new Dictionary<string, Type> ();
        private static ArrayList plugins = new ArrayList ();

        public static string PluginDirectory {
            get {
                return Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "plugins");
            }
        }

        public static void LoadPlugins (string[] names) {
            LoadPluginTypes (PluginDirectory);
#if DEBUG
            LoadPluginTypes (AppDomain.CurrentDomain.BaseDirectory);
#endif

            LoadPluginObjects (names);

            if (plugins.Count == 0)
                Daemon.Log.Warn ("No plugins were loaded");
        }

        private static void LoadPluginTypes (string dir) {
            if (!Directory.Exists (dir)) {
                return;
            }
            
            foreach (string file in Directory.GetFiles (dir, "*.dll")) {
                try {
                    Assembly asm = Assembly.LoadFrom (file);
                    Daemon.Log.DebugFormat ("Loaded assembly: {0}", asm.ToString ());

                    object[] attrs = asm.GetCustomAttributes (typeof (PluginAttribute), false);
                    if (attrs != null && attrs.Length > 0) {
                        foreach (PluginAttribute attr in attrs) {
                            Daemon.Log.DebugFormat ("Got plugin type {0} = {1}", attr.Name, attr.Type);
                            pluginTypes[attr.Name] = attr.Type;
                        }
                    }
                } catch (Exception e) {
                    Daemon.LogError (String.Format ("Failed to load plugin assembly '{0}'", file), e);
                }
            }
        }

        private static void LoadPluginObjects (string[] names) {
            foreach (string name in names) {
                if (!pluginTypes.ContainsKey (name)) {
                    Daemon.Log.WarnFormat ("No plugin named '{0}' was found", name);
                    continue;
                }

                try {
                    plugins.Add (Activator.CreateInstance (pluginTypes[name]));
                    Daemon.Log.InfoFormat ("Loaded plugin '{0}'", name);
                } catch (Exception e) {
                    Daemon.LogError (String.Format ("Failed to load '{0}'", name), e);
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
