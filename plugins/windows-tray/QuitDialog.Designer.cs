namespace Tangerine.src {
    partial class QuitDialog {
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
            this.label1 = new System.Windows.Forms.Label ();
            this.yesButton = new System.Windows.Forms.Button ();
            this.noButton = new System.Windows.Forms.Button ();
            this.dontAskCheck = new System.Windows.Forms.CheckBox ();
            this.SuspendLayout ();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point (12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size (268, 45);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tangerine will be started again when you login.  Do you want to prevent this by d" +
                "isabling music sharing?";
            // 
            // yesButton
            // 
            this.yesButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.yesButton.Location = new System.Drawing.Point (205, 92);
            this.yesButton.Name = "yesButton";
            this.yesButton.Size = new System.Drawing.Size (75, 23);
            this.yesButton.TabIndex = 1;
            this.yesButton.Text = "Yes";
            this.yesButton.UseVisualStyleBackColor = true;
            // 
            // noButton
            // 
            this.noButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.noButton.Location = new System.Drawing.Point (124, 92);
            this.noButton.Name = "noButton";
            this.noButton.Size = new System.Drawing.Size (75, 23);
            this.noButton.TabIndex = 2;
            this.noButton.Text = "No";
            this.noButton.UseVisualStyleBackColor = true;
            // 
            // dontAskCheck
            // 
            this.dontAskCheck.AutoSize = true;
            this.dontAskCheck.Location = new System.Drawing.Point (15, 48);
            this.dontAskCheck.Name = "dontAskCheck";
            this.dontAskCheck.Size = new System.Drawing.Size (117, 17);
            this.dontAskCheck.TabIndex = 3;
            this.dontAskCheck.Text = "Don\'t ask me again";
            this.dontAskCheck.UseVisualStyleBackColor = true;
            // 
            // QuitDialog
            // 
            this.AcceptButton = this.yesButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.noButton;
            this.ClientSize = new System.Drawing.Size (290, 127);
            this.Controls.Add (this.dontAskCheck);
            this.Controls.Add (this.noButton);
            this.Controls.Add (this.yesButton);
            this.Controls.Add (this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QuitDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Question";
            this.TopMost = true;
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button yesButton;
        private System.Windows.Forms.Button noButton;
        private System.Windows.Forms.CheckBox dontAskCheck;
    }
}