
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Mono.Unix.Native;
using Gtk;
using Nini.Config;

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

        public static void Main () {
            SetProcessName ("tangerine-properties");
            Application.Init ();

            PropertiesWindow win = new PropertiesWindow ();
            win.ShowAll ();
            
            Application.Run ();
            
        }
    }

    public class PropertiesWindow : Dialog {

        private string configPath;
        private string passwdPath;
        private List<Provider> providers;

        private ListStore providerStore;

        [Glade.Widget]
        private VBox prefsContent;

        [Glade.Widget]
        private VBox prefsControls;

        [Glade.Widget]
        private CheckButton enabledButton;
        
        [Glade.Widget]
        private Entry nameEntry;

        [Glade.Widget]
        private RadioButton beagleRadio;

        [Glade.Widget]
        private RadioButton providerRadio;

        [Glade.Widget]
        private ComboBox providerCombo;

        [Glade.Widget]
        private RadioButton specifyRadio;

        [Glade.Widget]
        private FileChooserButton directoryButton;

        [Glade.Widget]
        private SpinButton limitSpinButton;

        [Glade.Widget]
        private CheckButton userLimitButton;

        [Glade.Widget]
        private Entry passwordEntry;

        [Glade.Widget]
        private CheckButton passwordButton;

        public PropertiesWindow () : base ("Tangerine Music Sharing", null, DialogFlags.NoSeparator) {
            Resizable = false;
            IconThemeUtils.SetWindowIcon (this);
            AddButton (Stock.Close, ResponseType.Close);
            Response += delegate {
                Hide ();

                // hacktastic
                while (Application.EventsPending ()) {
                    Application.RunIteration (false);
                }
                
                SavePrefs ();
                Destroy ();
                Application.Quit ();
            };

            Glade.XML xml = new Glade.XML ("tangerine-properties.glade", "prefsContent");
            xml.Autoconnect (this);

            SetupProviders ();

            beagleRadio.Toggled += delegate {
                SetSensitive ();
            };

            providerRadio.Toggled += delegate {
                SetSensitive ();
            };
            
            specifyRadio.Toggled += delegate {
                SetSensitive ();
            };

            enabledButton.Toggled += delegate {
                SetSensitive ();
            };

            userLimitButton.Toggled += delegate {
                SetSensitive ();
            };

            passwordButton.Toggled += delegate {
                SetSensitive ();
            };

            VBox.Add (prefsContent);

            configPath = Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                                  ".tangerine");
            passwdPath = Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                                  ".tangerine-passwd");
            LoadPrefs ();
            SetSensitive ();
        }

        private void SetupProviders () {
            providers = Daemon.GetProviders ();
            
            providerStore = new ListStore (typeof (string), typeof (string));
            providerCombo.Model = providerStore;

            CellRendererText renderer = new CellRendererText ();
            providerCombo.PackStart (renderer, false);
            providerCombo.AddAttribute (renderer, "text", 0);

            foreach (Provider p in providers) {
                if (p.Plugin != "beagle") {
                    providerStore.AppendValues (p.Name, p.Plugin);
                }
            }

            if (providerStore.IterNChildren () == 0) {
                providerRadio.Sensitive = false;
                providerCombo.Sensitive = false;
                specifyRadio.Active = true;

                providerStore.AppendValues ("None", "None");
                providerCombo.Active = 0;
            } else {
                providerCombo.Active = 0;
            }

            beagleRadio.Sensitive = FindProvider ("beagle") != null;
        }

        private Provider FindProvider (string plugin) {
            foreach (Provider p in providers) {
                if (p.Plugin == plugin)
                    return p;
            }

            return null;
        }

        private void SetSensitive () {
            prefsControls.Sensitive = enabledButton.Active;
            directoryButton.Sensitive = specifyRadio.Active;
            providerCombo.Sensitive = providerRadio.Active;
            limitSpinButton.Sensitive = userLimitButton.Active;
            passwordEntry.Sensitive = passwordButton.Active;
        }

        private string Combine (params string[] paths) {
            if (paths.Length < 2)
                throw new ApplicationException ("at least 2 paths required");

            string result = paths[0];
            for (int i = 1; i < paths.Length; i++) {
                result = System.IO.Path.Combine (result, paths[i]);
            }

            return result;
        }

        private void SetProvider (string plugin) {
            if (plugin == "file" || (plugin == "beagle" && FindProvider (plugin) == null)) {
                specifyRadio.Active = true;
            } else if (plugin == "beagle") {
                beagleRadio.Active = true;
            } else {
                bool found = false;
                for (int i = 0; i < providerStore.IterNChildren (); i++) {
                    TreeIter iter;

                    providerStore.IterNthChild (out iter, i);
                    string val = (string) providerStore.GetValue (iter, 1);
                    if (val == plugin) {
                        providerCombo.Active = i;
                        found = true;
                    }
                }

                if (found) {
                    providerRadio.Active = true;
                } else if (FindProvider ("beagle") != null) {
                    beagleRadio.Active = true;
                } else {
                    specifyRadio.Active = true;
                }
            }
        }

        private void LoadPrefs () {
            if (!File.Exists (configPath))
                File.Create (configPath).Close ();

            Daemon.ConfigPath = configPath;
            Daemon.ParseConfig ();

            nameEntry.Text = Daemon.Name;

            SetProvider (Daemon.PluginNames[0]);
            
            limitSpinButton.Value = Daemon.MaxUsers;
            userLimitButton.Active = Daemon.MaxUsers > 0;

            string defaultDir = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                                                        "Music");
            
            if (Daemon.ConfigSource.Configs["FilePlugin"] != null) {
                string[] directories = Daemon.ConfigSource.Configs["FilePlugin"].Get ("directories",
                                                                                    defaultDir).Split (':');
                if (Directory.Exists (directories[0])) {
                    directoryButton.SetFilename (directories[0]);
                } else {
                    directoryButton.SetFilename (defaultDir);
                }
            } else {
                directoryButton.SetFilename (defaultDir);
            }

            passwordEntry.Text = ReadPassword ();
            passwordButton.Active = passwordEntry.Text != null && passwordEntry.Text != String.Empty;

            enabledButton.Active = Daemon.IsAutostartEnabled;
        }

        private string ReadPassword () {
            if (!File.Exists (passwdPath))
                return String.Empty;

            using (StreamReader reader = new StreamReader (File.Open (passwdPath, FileMode.Open))) {
                return reader.ReadLine ();
            }
        }

        private void WritePassword () {
            if (!passwordEntry.Sensitive || passwordEntry.Text == String.Empty || passwordEntry.Text == null) {
                File.Delete (passwdPath);
            } else {
                using (StreamWriter writer = new StreamWriter (File.Open (passwdPath, FileMode.Create))) {
                    writer.WriteLine (passwordEntry.Text);
                }
            }
        }

        private void SavePrefs () {
            Daemon.PasswordFile = passwdPath;
            if (!passwordEntry.Sensitive) {
                File.Delete (passwdPath);
            }
            
            Daemon.LogFile = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                                                     ".tangerine-log");

            if (limitSpinButton.Sensitive) {
                Daemon.MaxUsers = (int) limitSpinButton.Value;
            } else {
                Daemon.MaxUsers = 0; // unlimited
            }
            
            Daemon.Name = nameEntry.Text;

            string pluginName;
            
            if (specifyRadio.Active) {
                pluginName = "file";
            } else if (beagleRadio.Active) {
                pluginName = "beagle";
            } else {
                TreeIter iter;

                providerStore.IterNthChild (out iter, providerCombo.Active);
                pluginName = (string) providerStore.GetValue (iter, 1);
            }

            Daemon.PluginNames = new string[] { pluginName, "session" };

            IConfig cfg = Daemon.ConfigSource.Configs["FilePlugin"];
            if (cfg == null) {
                cfg = Daemon.ConfigSource.AddConfig ("FilePlugin");
            }

            cfg.Set ("directories", directoryButton.Filename);

            if (Daemon.IsSaveNeeded () ||
                passwordEntry.Text != ReadPassword ()) {
                WritePassword ();
                Daemon.SaveConfig ();

                if (enabledButton.Active) {
                    Daemon.EnableAutostart ();
                    RestartDaemon ();
                }
            } else {
                if (enabledButton.Active) {
                    Daemon.EnableAutostart ();

                    if (!IsRunning ()) {
                        StartDaemon ();
                    }
                }
            }

            if (!enabledButton.Active) {
                Daemon.DisableAutostart ();
                StopDaemon ();
            }
        }

        private void RestartDaemon () {
            StopDaemon ();
            StartDaemon ();
        }

        private bool IsRunning () {
            Process[] procs = Process.GetProcessesByName ("tangerine-daemon");
            return procs != null && procs.Length > 0;
        }
        
        private void StopDaemon () {
            Process[] procs = Process.GetProcessesByName ("tangerine-daemon");
            if (procs != null && procs.Length > 0) {
                Syscall.kill (procs[0].Id, Signum.SIGTERM);

                while (Syscall.kill (procs[0].Id, 0) == 0) {
                    Thread.Sleep (100);
                }
            }
        }

        [DllImport ("libglib-2.0.so.0")]
        private static extern int g_spawn_command_line_async (string cmd, IntPtr error);

        private void StartDaemon () {
            g_spawn_command_line_async ("tangerine", IntPtr.Zero);
        }
    }
}
