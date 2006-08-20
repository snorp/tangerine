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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WindowsProperties));
            this.enabledCheck = new System.Windows.Forms.CheckBox();
            this.shareLabel = new System.Windows.Forms.Label();
            this.shareNameBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.musicDirBox = new System.Windows.Forms.TextBox();
            this.musicDirDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.contentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // enabledCheck
            // 
            this.enabledCheck.AutoSize = true;
            this.enabledCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enabledCheck.Location = new System.Drawing.Point(12, 12);
            this.enabledCheck.Name = "enabledCheck";
            this.enabledCheck.Size = new System.Drawing.Size(149, 17);
            this.enabledCheck.TabIndex = 0;
            this.enabledCheck.Text = "Enable Music Sharing";
            this.enabledCheck.UseVisualStyleBackColor = true;
            this.enabledCheck.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // shareLabel
            // 
            this.shareLabel.AutoSize = true;
            this.shareLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.shareLabel.Location = new System.Drawing.Point(21, 9);
            this.shareLabel.Name = "shareLabel";
            this.shareLabel.Size = new System.Drawing.Size(76, 13);
            this.shareLabel.TabIndex = 1;
            this.shareLabel.Text = "Share Name";
            // 
            // shareNameBox
            // 
            this.shareNameBox.Location = new System.Drawing.Point(103, 6);
            this.shareNameBox.Name = "shareNameBox";
            this.shareNameBox.Size = new System.Drawing.Size(169, 20);
            this.shareNameBox.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(197, 55);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Browse...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Music Directory";
            // 
            // musicDirBox
            // 
            this.musicDirBox.Location = new System.Drawing.Point(103, 29);
            this.musicDirBox.Name = "musicDirBox";
            this.musicDirBox.Size = new System.Drawing.Size(169, 20);
            this.musicDirBox.TabIndex = 5;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(198, 133);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(117, 133);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // contentPanel
            // 
            this.contentPanel.Controls.Add(this.shareNameBox);
            this.contentPanel.Controls.Add(this.shareLabel);
            this.contentPanel.Controls.Add(this.button1);
            this.contentPanel.Controls.Add(this.label2);
            this.contentPanel.Controls.Add(this.musicDirBox);
            this.contentPanel.Enabled = false;
            this.contentPanel.Location = new System.Drawing.Point(1, 35);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(283, 92);
            this.contentPanel.TabIndex = 12;
            // 
            // WindowsProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 163);
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.enabledCheck);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WindowsProperties";
            this.Text = "Tangerine Preferences";
            this.contentPanel.ResumeLayout(false);
            this.contentPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox enabledCheck;
        private System.Windows.Forms.Label shareLabel;
        private System.Windows.Forms.TextBox shareNameBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox musicDirBox;
        private System.Windows.Forms.FolderBrowserDialog musicDirDialog;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel contentPanel;
    }
}