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
            this.providerCombo = new System.Windows.Forms.ComboBox ();
            this.providerRadioButton = new System.Windows.Forms.RadioButton ();
            this.dirRadioButton = new System.Windows.Forms.RadioButton ();
            this.googleRadioButton = new System.Windows.Forms.RadioButton ();
            this.accessControlPanel = new System.Windows.Forms.GroupBox ();
            this.userLimitCheckBox = new System.Windows.Forms.CheckBox ();
            this.passwordCheckBox = new System.Windows.Forms.CheckBox ();
            this.maxUsersButton = new System.Windows.Forms.NumericUpDown ();
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
            this.enabledCheck.Size = new System.Drawing.Size (146, 17);
            this.enabledCheck.TabIndex = 0;
            this.enabledCheck.Text = "Enable music sharing";
            this.toolTip1.SetToolTip (this.enabledCheck, "Whether music sharing is enabled or not");
            this.enabledCheck.UseVisualStyleBackColor = true;
            this.enabledCheck.CheckedChanged += new System.EventHandler (this.OnEnableCheckedChanged);
            // 
            // shareLabel
            // 
            this.shareLabel.AutoSize = true;
            this.shareLabel.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.shareLabel.Location = new System.Drawing.Point (12, 23);
            this.shareLabel.Name = "shareLabel";
            this.shareLabel.Size = new System.Drawing.Size (74, 13);
            this.shareLabel.TabIndex = 1;
            this.shareLabel.Text = "Share name";
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
            this.musicDirButton.Location = new System.Drawing.Point (207, 116);
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
            this.musicDirBox.Location = new System.Drawing.Point (15, 118);
            this.musicDirBox.Name = "musicDirBox";
            this.musicDirBox.Size = new System.Drawing.Size (186, 20);
            this.musicDirBox.TabIndex = 5;
            this.toolTip1.SetToolTip (this.musicDirBox, "The directory to share");
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point (232, 296);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size (75, 23);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler (this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point (151, 296);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size (75, 23);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler (this.cancelButton_Click);
            // 
            // generalOptionsPanel
            // 
            this.generalOptionsPanel.Controls.Add (this.providerCombo);
            this.generalOptionsPanel.Controls.Add (this.providerRadioButton);
            this.generalOptionsPanel.Controls.Add (this.dirRadioButton);
            this.generalOptionsPanel.Controls.Add (this.googleRadioButton);
            this.generalOptionsPanel.Controls.Add (this.shareNameBox);
            this.generalOptionsPanel.Controls.Add (this.musicDirButton);
            this.generalOptionsPanel.Controls.Add (this.shareLabel);
            this.generalOptionsPanel.Controls.Add (this.musicDirBox);
            this.generalOptionsPanel.Location = new System.Drawing.Point (12, 35);
            this.generalOptionsPanel.Name = "generalOptionsPanel";
            this.generalOptionsPanel.Size = new System.Drawing.Size (295, 157);
            this.generalOptionsPanel.TabIndex = 13;
            this.generalOptionsPanel.TabStop = false;
            this.generalOptionsPanel.Text = "General Options";
            // 
            // providerCombo
            // 
            this.providerCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.providerCombo.FormattingEnabled = true;
            this.providerCombo.Location = new System.Drawing.Point (127, 69);
            this.providerCombo.Name = "providerCombo";
            this.providerCombo.Size = new System.Drawing.Size (155, 21);
            this.providerCombo.TabIndex = 17;
            // 
            // providerRadioButton
            // 
            this.providerRadioButton.AutoSize = true;
            this.providerRadioButton.Location = new System.Drawing.Point (15, 69);
            this.providerRadioButton.Name = "providerRadioButton";
            this.providerRadioButton.Size = new System.Drawing.Size (106, 17);
            this.providerRadioButton.TabIndex = 16;
            this.providerRadioButton.TabStop = true;
            this.providerRadioButton.Text = "Find music using:";
            this.providerRadioButton.UseVisualStyleBackColor = true;
            this.providerRadioButton.CheckedChanged += new System.EventHandler (this.providerRadio_CheckedChanged);
            // 
            // dirRadioButton
            // 
            this.dirRadioButton.AutoSize = true;
            this.dirRadioButton.Location = new System.Drawing.Point (15, 95);
            this.dirRadioButton.Name = "dirRadioButton";
            this.dirRadioButton.Size = new System.Drawing.Size (89, 17);
            this.dirRadioButton.TabIndex = 15;
            this.dirRadioButton.Text = "Specify folder";
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
            this.googleRadioButton.Size = new System.Drawing.Size (137, 17);
            this.googleRadioButton.TabIndex = 15;
            this.googleRadioButton.TabStop = true;
            this.googleRadioButton.Text = "Automatically find music";
            this.toolTip1.SetToolTip (this.googleRadioButton, "Use Google Desktop to automatically find and share your music");
            this.googleRadioButton.UseVisualStyleBackColor = true;
            this.googleRadioButton.CheckedChanged += new System.EventHandler (this.googleRadioButton_CheckedChanged);
            // 
            // accessControlPanel
            // 
            this.accessControlPanel.Controls.Add (this.userLimitCheckBox);
            this.accessControlPanel.Controls.Add (this.passwordCheckBox);
            this.accessControlPanel.Controls.Add (this.maxUsersButton);
            this.accessControlPanel.Controls.Add (this.passwordBox);
            this.accessControlPanel.Location = new System.Drawing.Point (12, 198);
            this.accessControlPanel.Name = "accessControlPanel";
            this.accessControlPanel.Size = new System.Drawing.Size (295, 83);
            this.accessControlPanel.TabIndex = 14;
            this.accessControlPanel.TabStop = false;
            this.accessControlPanel.Text = "Access Control";
            // 
            // userLimitCheckBox
            // 
            this.userLimitCheckBox.AutoSize = true;
            this.userLimitCheckBox.Location = new System.Drawing.Point (15, 51);
            this.userLimitCheckBox.Name = "userLimitCheckBox";
            this.userLimitCheckBox.Size = new System.Drawing.Size (71, 17);
            this.userLimitCheckBox.TabIndex = 17;
            this.userLimitCheckBox.Text = "User limit:";
            this.userLimitCheckBox.UseVisualStyleBackColor = true;
            this.userLimitCheckBox.CheckedChanged += new System.EventHandler (this.userLimitCheckBox_CheckedChanged);
            // 
            // passwordCheckBox
            // 
            this.passwordCheckBox.AutoSize = true;
            this.passwordCheckBox.Location = new System.Drawing.Point (15, 24);
            this.passwordCheckBox.Name = "passwordCheckBox";
            this.passwordCheckBox.Size = new System.Drawing.Size (75, 17);
            this.passwordCheckBox.TabIndex = 16;
            this.passwordCheckBox.Text = "Password:";
            this.passwordCheckBox.UseVisualStyleBackColor = true;
            this.passwordCheckBox.CheckedChanged += new System.EventHandler (this.checkBox1_CheckedChanged);
            // 
            // maxUsersButton
            // 
            this.maxUsersButton.Location = new System.Drawing.Point (94, 48);
            this.maxUsersButton.Maximum = new decimal (new int[] {
            999,
            0,
            0,
            0});
            this.maxUsersButton.Minimum = new decimal (new int[] {
            1,
            0,
            0,
            0});
            this.maxUsersButton.Name = "maxUsersButton";
            this.maxUsersButton.Size = new System.Drawing.Size (45, 20);
            this.maxUsersButton.TabIndex = 15;
            this.toolTip1.SetToolTip (this.maxUsersButton, "Maximum number of simultaneous users, 0 for no limit");
            this.maxUsersButton.Value = new decimal (new int[] {
            1,
            0,
            0,
            0});
            // 
            // passwordBox
            // 
            this.passwordBox.Location = new System.Drawing.Point (94, 22);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.Size = new System.Drawing.Size (188, 20);
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
            this.ClientSize = new System.Drawing.Size (318, 330);
            this.Controls.Add (this.accessControlPanel);
            this.Controls.Add (this.generalOptionsPanel);
            this.Controls.Add (this.cancelButton);
            this.Controls.Add (this.okButton);
            this.Controls.Add (this.enabledCheck);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject ("$this.Icon")));
            this.Name = "WindowsProperties";
            this.Text = "Tangerine Preferences";
            this.Load += new System.EventHandler (this.WindowsProperties_Load);
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
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.NumericUpDown maxUsersButton;
        private System.Windows.Forms.RadioButton dirRadioButton;
        private System.Windows.Forms.RadioButton googleRadioButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ComboBox providerCombo;
        private System.Windows.Forms.RadioButton providerRadioButton;
        private System.Windows.Forms.CheckBox passwordCheckBox;
        private System.Windows.Forms.CheckBox userLimitCheckBox;
    }
}