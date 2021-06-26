namespace League_Sandbox_Auto_Setup
{
    partial class LeagueSandboxAutoSetupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LeagueSandboxAutoSetupForm));
            this.label1 = new System.Windows.Forms.Label();
            this.installDirectoryText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cloningProgressLabel = new System.Windows.Forms.Label();
            this.downloadingProgressLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.unzippingProgressLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.launchingProgressLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.finishProgressLabel = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.installingTestboxLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.abortText = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(204, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Directory for League Sandbox Installation:";
            // 
            // installDirectoryText
            // 
            this.installDirectoryText.Location = new System.Drawing.Point(227, 12);
            this.installDirectoryText.Name = "installDirectoryText";
            this.installDirectoryText.Size = new System.Drawing.Size(213, 20);
            this.installDirectoryText.TabIndex = 1;
            this.installDirectoryText.Text = "C:\\LeagueSandbox";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(273, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Note: Please install Visual Studio first for complete setup.";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(446, 35);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 3;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(264, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Cloning Code Repositories";
            // 
            // cloningProgressLabel
            // 
            this.cloningProgressLabel.Location = new System.Drawing.Point(12, 88);
            this.cloningProgressLabel.Name = "cloningProgressLabel";
            this.cloningProgressLabel.Size = new System.Drawing.Size(246, 13);
            this.cloningProgressLabel.TabIndex = 5;
            this.cloningProgressLabel.Text = "❌";
            this.cloningProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // downloadingProgressLabel
            // 
            this.downloadingProgressLabel.Location = new System.Drawing.Point(12, 88);
            this.downloadingProgressLabel.Name = "downloadingProgressLabel";
            this.downloadingProgressLabel.Size = new System.Drawing.Size(246, 36);
            this.downloadingProgressLabel.TabIndex = 7;
            this.downloadingProgressLabel.Text = "❌";
            this.downloadingProgressLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(264, 111);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(214, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Downloading League of Legends 420 Client";
            // 
            // unzippingProgressLabel
            // 
            this.unzippingProgressLabel.Location = new System.Drawing.Point(12, 134);
            this.unzippingProgressLabel.Name = "unzippingProgressLabel";
            this.unzippingProgressLabel.Size = new System.Drawing.Size(246, 13);
            this.unzippingProgressLabel.TabIndex = 9;
            this.unzippingProgressLabel.Text = "❌";
            this.unzippingProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(264, 134);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(199, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Unzipping League of Legends 420 Client";
            // 
            // launchingProgressLabel
            // 
            this.launchingProgressLabel.Location = new System.Drawing.Point(12, 180);
            this.launchingProgressLabel.Name = "launchingProgressLabel";
            this.launchingProgressLabel.Size = new System.Drawing.Size(246, 13);
            this.launchingProgressLabel.TabIndex = 11;
            this.launchingProgressLabel.Text = "❌";
            this.launchingProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(264, 180);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(173, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Launching Visual Studio for first run";
            // 
            // finishProgressLabel
            // 
            this.finishProgressLabel.Location = new System.Drawing.Point(12, 203);
            this.finishProgressLabel.Name = "finishProgressLabel";
            this.finishProgressLabel.Size = new System.Drawing.Size(246, 13);
            this.finishProgressLabel.TabIndex = 13;
            this.finishProgressLabel.Text = "❌";
            this.finishProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(264, 203);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(142, 13);
            this.label12.TabIndex = 12;
            this.label12.Text = "Finished one time auto setup";
            // 
            // installingTestboxLabel
            // 
            this.installingTestboxLabel.Location = new System.Drawing.Point(12, 158);
            this.installingTestboxLabel.Name = "installingTestboxLabel";
            this.installingTestboxLabel.Size = new System.Drawing.Size(246, 13);
            this.installingTestboxLabel.TabIndex = 15;
            this.installingTestboxLabel.Text = "❌";
            this.installingTestboxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(264, 158);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(257, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Installing LeagueUI(Testbox) Multiplayer Matchmaker";
            // 
            // abortText
            // 
            this.abortText.AutoSize = true;
            this.abortText.ForeColor = System.Drawing.Color.Maroon;
            this.abortText.Location = new System.Drawing.Point(46, 58);
            this.abortText.Name = "abortText";
            this.abortText.Size = new System.Drawing.Size(292, 13);
            this.abortText.TabIndex = 16;
            this.abortText.Text = "Current operation will be aborted at the next task completion.";
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(446, 9);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 17;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // LeagueSandboxAutoSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 236);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.abortText);
            this.Controls.Add(this.installingTestboxLabel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.finishProgressLabel);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.launchingProgressLabel);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.unzippingProgressLabel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.downloadingProgressLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cloningProgressLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.installDirectoryText);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LeagueSandboxAutoSetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "League Sandbox Auto Setup";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox installDirectoryText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label cloningProgressLabel;
        private System.Windows.Forms.Label downloadingProgressLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label unzippingProgressLabel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label launchingProgressLabel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label finishProgressLabel;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label installingTestboxLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label abortText;
        private System.Windows.Forms.Button browseButton;
    }
}

