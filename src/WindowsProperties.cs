using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tangerine;
using Nini;
using IWshRuntimeLibrary;

using File = System.IO.File;

namespace TangerineProperties.src {

    public partial class WindowsProperties : Form {
        private string passwdPath;
        private IList<Provider> providers;

        public WindowsProperties () {
            InitializeComponent ();
            LoadProviders ();
            LoadPrefs ();
        }

        private void LoadProviders () {
            providers = Daemon.GetProviders ();

            List<Provider> comboProviders = new List<Provider> ();
            foreach (Provider p in providers) {
                if (p.Plugin != "google") {
                    comboProviders.Add (p);
                }
            }

            if (comboProviders.Count == 0) {
                comboProviders.Add (new Provider ("None", "none"));
                providerCombo.Enabled = false;
                providerRadioButton.Enabled = false;
            }

            providerCombo.DataSource = comboProviders;
            googleRadioButton.Enabled = FindProvider ("google") != null;
        }

        private Provider FindProvider (string plugin) {
            foreach (Provider p in providers) {
                if (p.Plugin == plugin)
                    return p;
            }

            return null;
        }

        private void SetProvider (string plugin) {
            Provider provider = FindProvider (plugin);

            Process[] crawlers = Process.GetProcessesByName ("GoogleDesktopCrawl");

            if (plugin == "file" || (plugin == "google" && (provider == null ||
                crawlers == null || crawlers.Length == 0))) {
                dirRadioButton.Checked = true;
            } else if (plugin == "google") {
                googleRadioButton.Checked = true;
            } else {
                if (provider != null) {
                    providerCombo.SelectedItem = provider;
                    providerRadioButton.Checked = true;
                } if (provider == null && FindProvider ("google") == null) {
                    dirRadioButton.Select ();
                } else if (provider == null) {
                    googleRadioButton.Checked = true;
                }
            }
        }

        private void button1_Click (object sender, EventArgs e) {
            DialogResult res = musicDirDialog.ShowDialog ();
            if (res == DialogResult.OK) {
                musicDirBox.Text = musicDirDialog.SelectedPath;
            }
        }

        private void cancelButton_Click (object sender, EventArgs e) {
            this.Close ();
        }

        private void okButton_Click (object sender, EventArgs e) {
            this.Hide ();
            SavePrefs ();
            this.Close ();
        }

        private void checkBox1_CheckedChanged (object sender, EventArgs e) {
            SetEnabled ();
        }

        private bool GetEnabled () {
            return File.Exists (GetStartupPath ());
        }

        private void SetEnabled () {
            generalOptionsPanel.Enabled = enabledCheck.Checked;
            accessControlPanel.Enabled = enabledCheck.Checked;

            providerCombo.Enabled = providerRadioButton.Checked;
            musicDirBox.Enabled = dirRadioButton.Checked;
            musicDirButton.Enabled = dirRadioButton.Checked;
        }

        private void LoadPrefs () {
            passwdPath = Path.Combine (Daemon.ConfigDirectory, "password");

            Daemon.ParseConfig ();
            enabledCheck.Checked = GetEnabled ();
            shareNameBox.Text = Daemon.Name;

            if (Daemon.ConfigSource.Configs["FilePlugin"] != null) {
                musicDirBox.Text = Daemon.ConfigSource.Configs["FilePlugin"].GetString ("directories").Split (';')[0];
            } else {
                musicDirBox.Text = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);
            }

            SetProvider (Daemon.PluginNames[0]);

            passwordBox.Text = ReadPassword ();

            maxUsersButton.Value = Daemon.MaxUsers;

            Process[] crawlers = Process.GetProcessesByName("GoogleDesktopCrawl");
            if (crawlers == null || crawlers.Length == 0) {
                googleRadioButton.Enabled = false;
            }

            SetEnabled ();
        }

        private string GetStartupPath () {
            return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Startup),
                "tangerine.lnk");
        }

        private string GetDaemonPath () {
            return Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "tangerine-daemon.exe");
        }

        private void InstallStartupProgram () {
            WshShell shell = new WshShell ();
            IWshShortcut shortcut = (IWshShortcut) shell.CreateShortcut (GetStartupPath ());
            shortcut.TargetPath = GetDaemonPath ();
            shortcut.IconLocation = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "tangerine.ico");
            shortcut.Save ();
        }

        private void RemoveStartupProgram () {
            string path = GetStartupPath ();
            if (File.Exists (path))
                File.Delete (path);
        }

        private string ReadPassword () {
            if (!File.Exists (passwdPath))
                return String.Empty;

            using (StreamReader reader = new StreamReader (File.Open (passwdPath, FileMode.Open))) {
                return reader.ReadLine ();
            }
        }

        private void WritePassword () {
            if (File.Exists (passwdPath) && (passwordBox.Text == String.Empty || passwordBox.Text == null)) {
                File.Delete (passwdPath);
            } else {
                string dir = Path.GetDirectoryName (passwdPath);
                if (!Directory.Exists (dir)) {
                    Directory.CreateDirectory (dir);
                }

                using (StreamWriter writer = new StreamWriter (File.Open (passwdPath, FileMode.Create))) {
                    writer.WriteLine (passwordBox.Text);
                }
            }
        }

        private void SavePrefs () {
            Daemon.Name = shareNameBox.Text;

            string plugin;

            if (googleRadioButton.Checked) {
                plugin = "google";
            } else if (dirRadioButton.Checked) {
                plugin = "file";
            } else {
                plugin = ((Provider) providerCombo.SelectedItem).Plugin;
            }

            Daemon.PluginNames = new string[] { plugin, "system-tray" };

            if (Daemon.ConfigSource.Configs["FilePlugin"] == null) {
                Daemon.ConfigSource.AddConfig ("FilePlugin");
            }

            Daemon.ConfigSource.Configs["FilePlugin"].Set ("directories", musicDirBox.Text);
            Daemon.MaxUsers = (int) maxUsersButton.Value;

            WritePassword ();
            if (passwordBox.Text != null && passwordBox.Text != String.Empty) {
                Daemon.PasswordFile = passwdPath;
            } else {
                Daemon.PasswordFile = null;
            }

            Daemon.SaveConfig ();

            if (enabledCheck.Checked) {
                InstallStartupProgram ();
                RestartDaemon ();
            } else {
                RemoveStartupProgram ();
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
                foreach (Process proc in procs) {
                    EventWaitHandle handle = new EventWaitHandle (false, EventResetMode.AutoReset,
                        "tangerine-" + proc.Id);

                    handle.Set ();
                    proc.WaitForExit ();
                }
            }
        }

        private void StartDaemon () {
            Process.Start (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "tangerine-daemon"));
        }

        private void googleRadioButton_CheckedChanged (object sender, EventArgs e) {
            SetEnabled ();
        }

        private void dirRadioButton_CheckedChanged (object sender, EventArgs e) {
            SetEnabled ();
        }

        private void WindowsProperties_Load(object sender, EventArgs e)
        {

        }

        private void providerRadio_CheckedChanged(object sender, EventArgs e)
        {
            SetEnabled ();
        }
    }

    public class EntryPoint {

        [STAThread]
        public static void Main (string[] args) {
            Application.EnableVisualStyles ();
            Application.SetCompatibleTextRenderingDefault (false);
            WindowsProperties win = new WindowsProperties ();
            win.Closed += delegate {
                Application.Exit ();
            };

            win.Show ();
            Application.Run ();
        }
    }


}