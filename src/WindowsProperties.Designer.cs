namespace TangerineProperties.src {
    partial class WindowsProperties {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose ();
            }
            base.Dispose (disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.components = new System.ComponentModel.Container ();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (WindowsProperties));
            this.enabledCheck = new System.Windows.Forms.CheckBox ();
            this.shareLabel = new System.Windows.Forms.Label ();
            this.shareNameBox = new System.Windows.Forms.TextBox ();
            this.musicDirButton = new System.Windows.Forms.Button ();
            this.musicDirBox = new System.Windows.Forms.TextBox ();
            this.musicDirDialog = new System.Windows.Forms.FolderBrowserDialog ();
            this.okButton = new System.Windows.Forms.Button ();
            this.cancelButton = new System.Windows.Forms.Button ();
            this.generalOptionsPanel = new System.Windows.Forms.GroupBox ();
            this.dirRadioButton = new System.Windows.Forms.RadioButton ();
            this.googleRadioButton = new System.Windows.Forms.RadioButton ();
            this.accessControlPanel = new System.Windows.Forms.GroupBox ();
            this.maxUsersButton = new System.Windows.Forms.NumericUpDown ();
            this.label3 = new System.Windows.Forms.Label ();
            this.label1 = new System.Windows.Forms.Label ();
            this.passwordBox = new System.Windows.Forms.TextBox ();
            this.toolTip1 = new System.Windows.Forms.ToolTip (this.components);
            this.generalOptionsPanel.SuspendLayout ();
            this.accessControlPanel.SuspendLayout ();
            ((System.ComponentModel.ISupportInitialize) (this.maxUsersButton)).BeginInit ();
            this.SuspendLayout ();
            // 
            // enabledCheck
            // 
            this.enabledCheck.AutoSize = true;
            this.enabledCheck.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.enabledCheck.Location = new System.Drawing.Point (12, 12);
            this.enabledCheck.Name = "enabledCheck";
            this.enabledCheck.Size = new System.Drawing.Size (149, 17);
            this.enabledCheck.TabIndex = 0;
            this.enabledCheck.Text = "Enable Music Sharing";
            this.toolTip1.SetToolTip (this.enabledCheck, "Whether music sharing is enabled or not");
            this.enabledCheck.UseVisualStyleBackColor = true;
            this.enabledCheck.CheckedChanged += new System.EventHandler (this.checkBox1_CheckedChanged);
            // 
            // shareLabel
            // 
            this.shareLabel.AutoSize = true;
            this.shareLabel.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.shareLabel.Location = new System.Drawing.Point (12, 23);
            this.shareLabel.Name = "shareLabel";
            this.shareLabel.Size = new System.Drawing.Size (76, 13);
            this.shareLabel.TabIndex = 1;
            this.shareLabel.Text = "Share Name";
            // 
            // shareNameBox
            // 
            this.shareNameBox.Location = new System.Drawing.Point (94, 20);
            this.shareNameBox.Name = "shareNameBox";
            this.shareNameBox.Size = new System.Drawing.Size (188, 20);
            this.shareNameBox.TabIndex = 2;
            this.toolTip1.SetToolTip (this.shareNameBox, "The name that iTunes or other players will see");
            // 
            // musicDirButton
            // 
            this.musicDirButton.Location = new System.Drawing.Point (207, 90);
            this.musicDirButton.Name = "musicDirButton";
            this.musicDirButton.Size = new System.Drawing.Size (75, 23);
            this.musicDirButton.TabIndex = 3;
            this.musicDirButton.Text = "Browse...";
            this.toolTip1.SetToolTip (this.musicDirButton, "Show a directory chooser");
            this.musicDirButton.UseVisualStyleBackColor = true;
            this.musicDirButton.Click += new System.EventHandler (this.button1_Click);
            // 
            // musicDirBox
            // 
            this.musicDirBox.Location = new System.Drawing.Point (15, 92);
            this.musicDirBox.Name = "musicDirBox";
            this.musicDirBox.Size = new System.Drawing.Size (186, 20);
            this.musicDirBox.TabIndex = 5;
            this.toolTip1.SetToolTip (this.musicDirBox, "The directory to share");
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point (232, 275);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size (75, 23);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler (this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point (151, 275);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size (75, 23);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler (this.cancelButton_Click);
            // 
            // generalOptionsPanel
            // 
            this.generalOptionsPanel.Controls.Add (this.dirRadioButton);
            this.generalOptionsPanel.Controls.Add (this.googleRadioButton);
            this.generalOptionsPanel.Controls.Add (this.shareNameBox);
            this.generalOptionsPanel.Controls.Add (this.musicDirButton);
            this.generalOptionsPanel.Controls.Add (this.shareLabel);
            this.generalOptionsPanel.Controls.Add (this.musicDirBox);
            this.generalOptionsPanel.Location = new System.Drawing.Point (12, 35);
            this.generalOptionsPanel.Name = "generalOptionsPanel";
            this.generalOptionsPanel.Size = new System.Drawing.Size (295, 127);
            this.generalOptionsPanel.TabIndex = 13;
            this.generalOptionsPanel.TabStop = false;
            this.generalOptionsPanel.Text = "General Options";
            // 
            // dirRadioButton
            // 
            this.dirRadioButton.AutoSize = true;
            this.dirRadioButton.Location = new System.Drawing.Point (15, 69);
            this.dirRadioButton.Name = "dirRadioButton";
            this.dirRadioButton.Size = new System.Drawing.Size (92, 17);
            this.dirRadioButton.TabIndex = 15;
            this.dirRadioButton.Text = "Specify Folder";
            this.toolTip1.SetToolTip (this.dirRadioButton, "Manually specify a folder of music to share");
            this.dirRadioButton.UseVisualStyleBackColor = true;
            this.dirRadioButton.CheckedChanged += new System.EventHandler (this.dirRadioButton_CheckedChanged);
            // 
            // googleRadioButton
            // 
            this.googleRadioButton.AutoSize = true;
            this.googleRadioButton.Checked = true;
            this.googleRadioButton.Location = new System.Drawing.Point (15, 46);
            this.googleRadioButton.Name = "googleRadioButton";
            this.googleRadioButton.Size = new System.Drawing.Size (141, 17);
            this.googleRadioButton.TabIndex = 15;
            this.googleRadioButton.TabStop = true;
            this.googleRadioButton.Text = "Automatically Find Music";
            this.toolTip1.SetToolTip (this.googleRadioButton, "Use Google Desktop to automatically find and share your music");
            this.googleRadioButton.UseVisualStyleBackColor = true;
            this.googleRadioButton.CheckedChanged += new System.EventHandler (this.googleRadioButton_CheckedChanged);
            // 
            // accessControlPanel
            // 
            this.accessControlPanel.Controls.Add (this.maxUsersButton);
            this.accessControlPanel.Controls.Add (this.label3);
            this.accessControlPanel.Controls.Add (this.label1);
            this.accessControlPanel.Controls.Add (this.passwordBox);
            this.accessControlPanel.Location = new System.Drawing.Point (12, 177);
            this.accessControlPanel.Name = "accessControlPanel";
            this.accessControlPanel.Size = new System.Drawing.Size (295, 83);
            this.accessControlPanel.TabIndex = 14;
            this.accessControlPanel.TabStop = false;
            this.accessControlPanel.Text = "Access Control";
            // 
            // maxUsersButton
            // 
            this.maxUsersButton.Location = new System.Drawing.Point (79, 45);
            this.maxUsersButton.Maximum = new decimal (new int[] {
            999,
            0,
            0,
            0});
            this.maxUsersButton.Name = "maxUsersButton";
            this.maxUsersButton.Size = new System.Drawing.Size (45, 20);
            this.maxUsersButton.TabIndex = 15;
            this.toolTip1.SetToolTip (this.maxUsersButton, "Maximum number of simultaneous users, 0 for no limit");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.label3.Location = new System.Drawing.Point (10, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size (63, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "User Limit";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.label1.Location = new System.Drawing.Point (12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size (61, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Password";
            // 
            // passwordBox
            // 
            this.passwordBox.Location = new System.Drawing.Point (79, 19);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.Size = new System.Drawing.Size (169, 20);
            this.passwordBox.TabIndex = 15;
            this.toolTip1.SetToolTip (this.passwordBox, "Password used to access the share, leave blank for none");
            this.passwordBox.UseSystemPasswordChar = true;
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 1000;
            // 
            // WindowsProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size (318, 308);
            this.Controls.Add (this.accessControlPanel);
            this.Controls.Add (this.generalOptionsPanel);
            this.Controls.Add (this.cancelButton);
            this.Controls.Add (this.okButton);
            this.Controls.Add (this.enabledCheck);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject ("$this.Icon")));
            this.Name = "WindowsProperties";
            this.Text = "Tangerine Preferences";
            this.generalOptionsPanel.ResumeLayout (false);
            this.generalOptionsPanel.PerformLayout ();
            this.accessControlPanel.ResumeLayout (false);
            this.accessControlPanel.PerformLayout ();
            ((System.ComponentModel.ISupportInitialize) (this.maxUsersButton)).EndInit ();
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.CheckBox enabledCheck;
        private System.Windows.Forms.Label shareLabel;
        private System.Windows.Forms.TextBox shareNameBox;
        private System.Windows.Forms.Button musicDirButton;
        private System.Windows.Forms.TextBox musicDirBox;
        private System.Windows.Forms.FolderBrowserDialog musicDirDialog;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox generalOptionsPanel;
        private System.Windows.Forms.GroupBox accessControlPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.NumericUpDown maxUsersButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton dirRadioButton;
        private System.Windows.Forms.RadioButton googleRadioButton;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}