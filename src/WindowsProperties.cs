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
        public WindowsProperties () {
            InitializeComponent ();
            Load ();
        }

        private void WindowsProperties_Load (object sender, EventArgs e) {

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
            Save ();
            this.Close ();
        }

        private void checkBox1_CheckedChanged (object sender, EventArgs e) {
            SetEnabled (enabledCheck.Checked);
        }

        private bool GetEnabled () {
            return File.Exists (GetStartupPath ());
        }

        private void SetEnabled (bool val) {
            contentPanel.Enabled = val;
        }

        private void Load () {
            Daemon.ParseConfig ();
            enabledCheck.Checked = GetEnabled ();
            shareNameBox.Text = Daemon.Name;

            if (Daemon.ConfigSource.Configs["FilePlugin"] != null) {
                musicDirBox.Text = Daemon.ConfigSource.Configs["FilePlugin"].GetString ("directories").Split (':')[0];
            } else {
                musicDirBox.Text = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);
            }

            SetEnabled (GetEnabled ());
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

        private void Save () {
            Daemon.Name = shareNameBox.Text;
            Daemon.PluginNames = new string[] { "file" };
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