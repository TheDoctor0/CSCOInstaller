namespace CSCOUpdater
{
	partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button = new System.Windows.Forms.Button();
            this.labelInstalledVersion = new System.Windows.Forms.Label();
            this.labelLatestVersion = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelVersion = new System.Windows.Forms.Label();
            this.textBoxSteam = new System.Windows.Forms.TextBox();
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.labelSteam = new System.Windows.Forms.Label();
            this.groupBoxVersion = new System.Windows.Forms.GroupBox();
            this.labelLatest = new System.Windows.Forms.Label();
            this.labelInstalled = new System.Windows.Forms.Label();
            this.groupBoxVersion.SuspendLayout();
            this.SuspendLayout();
            // 
            // button
            // 
            this.button.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.button.Location = new System.Drawing.Point(15, 112);
            this.button.Margin = new System.Windows.Forms.Padding(2);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(239, 41);
            this.button.TabIndex = 0;
            this.button.Text = "Update";
            this.button.UseVisualStyleBackColor = true;
            this.button.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelInstalledVersion
            // 
            this.labelInstalledVersion.AutoSize = true;
            this.labelInstalledVersion.Location = new System.Drawing.Point(42, 16);
            this.labelInstalledVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelInstalledVersion.Name = "labelInstalledVersion";
            this.labelInstalledVersion.Size = new System.Drawing.Size(49, 13);
            this.labelInstalledVersion.TabIndex = 1;
            this.labelInstalledVersion.Text = "Installed:";
            // 
            // labelLatestVersion
            // 
            this.labelLatestVersion.AutoSize = true;
            this.labelLatestVersion.Location = new System.Drawing.Point(149, 16);
            this.labelLatestVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelLatestVersion.Name = "labelLatestVersion";
            this.labelLatestVersion.Size = new System.Drawing.Size(39, 13);
            this.labelLatestVersion.TabIndex = 2;
            this.labelLatestVersion.Text = "Latest:";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(15, 167);
            this.progressBar.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(239, 20);
            this.progressBar.TabIndex = 3;
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelVersion.Location = new System.Drawing.Point(124, 196);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(130, 13);
            this.labelVersion.TabIndex = 4;
            this.labelVersion.Text = "CS:CO Installer by O\'Zone";
            // 
            // textBoxSteam
            // 
            this.textBoxSteam.Location = new System.Drawing.Point(15, 77);
            this.textBoxSteam.Name = "textBoxSteam";
            this.textBoxSteam.Size = new System.Drawing.Size(239, 20);
            this.textBoxSteam.TabIndex = 5;
            this.textBoxSteam.Text = "C:\\Program Files (x86)\\Steam";
            this.textBoxSteam.Click += new System.EventHandler(this.textBox1_Click);
            this.textBoxSteam.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // labelSteam
            // 
            this.labelSteam.AutoSize = true;
            this.labelSteam.Location = new System.Drawing.Point(12, 61);
            this.labelSteam.Name = "labelSteam";
            this.labelSteam.Size = new System.Drawing.Size(72, 13);
            this.labelSteam.TabIndex = 6;
            this.labelSteam.Text = "Steam Folder:";
            // 
            // groupBoxVersion
            // 
            this.groupBoxVersion.Controls.Add(this.labelLatest);
            this.groupBoxVersion.Controls.Add(this.labelInstalled);
            this.groupBoxVersion.Controls.Add(this.labelLatestVersion);
            this.groupBoxVersion.Controls.Add(this.labelInstalledVersion);
            this.groupBoxVersion.Location = new System.Drawing.Point(13, 13);
            this.groupBoxVersion.Name = "groupBoxVersion";
            this.groupBoxVersion.Size = new System.Drawing.Size(241, 34);
            this.groupBoxVersion.TabIndex = 7;
            this.groupBoxVersion.TabStop = false;
            this.groupBoxVersion.Text = "Version";
            // 
            // labelLatest
            // 
            this.labelLatest.AutoSize = true;
            this.labelLatest.Location = new System.Drawing.Point(185, 16);
            this.labelLatest.Name = "labelLatest";
            this.labelLatest.Size = new System.Drawing.Size(0, 13);
            this.labelLatest.TabIndex = 4;
            // 
            // labelInstalled
            // 
            this.labelInstalled.AutoSize = true;
            this.labelInstalled.Location = new System.Drawing.Point(86, 16);
            this.labelInstalled.Name = "labelInstalled";
            this.labelInstalled.Size = new System.Drawing.Size(0, 13);
            this.labelInstalled.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(271, 218);
            this.Controls.Add(this.groupBoxVersion);
            this.Controls.Add(this.labelSteam);
            this.Controls.Add(this.textBoxSteam);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "CS:CO Installer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBoxVersion.ResumeLayout(false);
            this.groupBoxVersion.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button;
		private System.Windows.Forms.Label labelInstalledVersion;
		private System.Windows.Forms.Label labelLatestVersion;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.TextBox textBoxSteam;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private System.Windows.Forms.Label labelSteam;
        private System.Windows.Forms.GroupBox groupBoxVersion;
        private System.Windows.Forms.Label labelLatest;
        private System.Windows.Forms.Label labelInstalled;
    }
}

