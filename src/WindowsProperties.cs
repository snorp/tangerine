using System;
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

        public WindowsProperties () {
            InitializeComponent ();
            LoadPrefs ();
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

            if (googleRadioButton.Checked) {
                musicDirBox.Enabled = false;
                musicDirButton.Enabled = false;
            } else {
                musicDirBox.Enabled = true;
                musicDirButton.Enabled = true;
            }
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

            if (Daemon.PluginNames == null || Daemon.PluginNames[0] == "google") {
                googleRadioButton.Checked = true;
            } else {
                dirRadioButton.Checked = true;
            }

            passwordBox.Text = ReadPassword ();

            maxUsersButton.Value = Daemon.MaxUsers;

            Process[] crawlers = Process.GetProcessesByName("GoogleDesktopCrawl");
            if (crawlers == null || crawlers.Length == 0) {
                googleRadioButton.Enabled = false;
                dirRadioButton.Checked = true;
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

            if (googleRadioButton.Checked) {
                Daemon.PluginNames = new string[] { "google" };
            } else {
                Daemon.PluginNames = new string[] { "file" };
            }

            if (Daemon.ConfigSource.Configs["FilePlugin"] == null) {
                Daemon.ConfigSource.AddConfig ("FilePlugin");
            }

            Daemon.ConfigSource.Configs["FilePlugin"].Set ("directories", musicDirBox.Text);
            Daemon.MaxUsers = (int) maxUsersButton.Value;

            Daemon.PasswordFile = passwdPath;
            WritePassword ();

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
                    proc.Kill ();
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