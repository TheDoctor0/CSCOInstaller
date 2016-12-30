using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Web;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace CSCOUpdater
{
    public partial class Form1 : Form
    {
        string versionUrl = "http://cs-reload.pl/csco/";
        string updateUrl = "";
        string steamDirectory = "";

        double latestVersion = 0.0;
        double installedVersion = 0.0;

        bool downloading = false;
        bool update = false;

        WebClient client;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            CheckVersion();
        }

        protected override void OnPaintBackground(PaintEventArgs p)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            base.OnPaintBackground(p);
        }

        private void CheckVersion()
        {
            this.ActiveControl = labelVersion;

            latestVersion = 0.0;
            installedVersion = 0.0;

            string versionFile = "NULL";

            steamDirectory = textBoxSteam.Text + "/steamapps/sourcemods";

            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient()) versionFile = client.DownloadString(versionUrl);
            }
            catch (Exception)
            {
                MessageBox.Show("Error! Unable to get version file.\nCheck your internet connection!");
            }

            if (versionFile != "NULL")
            {
                string temp = "";

                foreach (char c in versionFile)
                {
                    if (c == ' ')
                    {
                        latestVersion = Convert.ToDouble(temp);

                        temp = "";
                    }
                    else temp += c;
                }

                updateUrl = temp;

                if (!RemoteFileExists(updateUrl)) updateUrl = "";
            }

            if (Directory.Exists(steamDirectory + "/csco"))
            {
                try
                {
                    string localVersionFile = File.ReadAllText(steamDirectory + "/csco/version.txt");

                    installedVersion = Convert.ToDouble(localVersionFile);

                    labelInstalled.Text = installedVersion.ToString("0.0");
                }
                catch (Exception)
                {

                    DialogResult r = MessageBox.Show("CS:CO installed, but version.txt not found.\nDo you have the latest version?", "Error",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

                    if (r == DialogResult.Yes)
                    {
                        File.WriteAllText(steamDirectory + "/csco/version.txt", "" + latestVersion);

                        installedVersion = latestVersion;

                        labelInstalled.Text = installedVersion.ToString("0.0");
                    }
                    else
                    {
                        labelInstalled.Text = "Unknown";

                        installedVersion = -1.0;
                    }
                }
            }
            else labelInstalled.Text = "None";

            labelLatest.Text = latestVersion.ToString("0.0");

            if (updateUrl != "")
            {
                if (!Directory.Exists(steamDirectory + "/csco")) button.Text = "Download";
                else
                {
                    if (installedVersion < latestVersion)
                    {
                        button.Text = "Update";

                        update = true;
                    }
                    else
                    {
                        button.Text = "No Update Required";

                        button.Enabled = false;
                    }
                }
            }
            else
            {
                button.Text = "Update Not Possible!";

                button.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBoxSteam.Text + "/Steam.exe"))
            {
                steamDirectory = textBoxSteam.Text + "/steamapps/sourcemods";

                IProgress<double> progress = new Progress<double>(b => progressBar.Value = (int)b);

                DeleteDir(steamDirectory + "/Temp");

                Directory.CreateDirectory(steamDirectory + "/Temp");

                try
                {
                    client = new WebClient();

                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadFileAsync(new Uri(updateUrl), steamDirectory + "/Temp/csco.zip");

                    button.Text = "Downloading...";

                    button.Enabled = false;

                    textBoxSteam.Enabled = false;

                    downloading = true;
                }
                catch (Exception ee)
                {
                    MessageBox.Show("Error: " + ee.Message);
                }
            }
            else
            {
                MessageBox.Show("Error: Check path to your Steam directory!");
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double percentage = double.Parse(e.BytesReceived.ToString()) / double.Parse(e.TotalBytesToReceive.ToString()) * 100;

            if (percentage >= 99.9) button.Text = "Installing...";
            else button.Text = "Downloading... " + Convert.ToString(Math.Round(percentage)) + "%";

            progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null) button.Text = "Downloading error!";
            else if (e.Cancelled) DeleteDir(steamDirectory + "/Temp");
            else
            {
                try
                {
                    DeleteDir(steamDirectory + "/csco");

                    try
                    {
                        ZipFile.ExtractToDirectory(steamDirectory + "/Temp/csco.zip", steamDirectory + "/");
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Error: " + ee.Message);
                    }

                    DeleteDir(steamDirectory + "/Temp");

                    if (File.Exists(steamDirectory + "/HostMe.txt")) File.Delete(steamDirectory + "/HostMe.txt");
                    if (File.Exists(steamDirectory + "/ReadMe.txt")) File.Delete(steamDirectory + "/ReadMe.txt");
                    if (!File.Exists(steamDirectory + "/csco/version.txt")) File.WriteAllText(steamDirectory + "/csco/version.txt", "" + latestVersion);
                    if (File.Exists(textBoxSteam.Text + "/maps/workshop")) File.Delete(textBoxSteam.Text + "/maps/workshop");

                    System.IO.DirectoryInfo di = new DirectoryInfo(textBoxSteam.Text + "/userdata");

                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        try
                        {
                            System.IO.DirectoryInfo dii = new DirectoryInfo(textBoxSteam.Text + "/userdata/" + dir.Name + "/ugc/");

                            foreach (DirectoryInfo dirr in dii.GetDirectories()) dirr.Delete(true);

                            ProcessStartInfo process = new ProcessStartInfo("cmd.exe");

                            process.WorkingDirectory = textBoxSteam.Text + @"\userdata\" + dir.Name + @"\ugc";
                            process.Arguments = @"/c mklink /J workshop " + "\"" + textBoxSteam.Text + @"\steamapps\common\Counter-Strike Global Offensive\csgo\maps\workshop" + "\"";

                            Process.Start(process);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (!update)
                    {
                        foreach (var process in Process.GetProcessesByName("Steam")) process.Kill();

                        MessageBox.Show("Steam is launching.\nCounter-Strike: Classic Offensive will appear in Library.");

                        System.Diagnostics.Process.Start(textBoxSteam.Text + "/Steam.exe");

                        button.Text = "Installation Complete";
                    }
                    else button.Text = "Update Complete";

                    labelInstalled.Text = latestVersion.ToString("0.0");

                    downloading = false;
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CheckVersion();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            folderBrowser.ShowDialog();

            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath)) textBoxSteam.Text = folderBrowser.SelectedPath;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (downloading)
            {
                DialogResult result = MessageBox.Show(this, "Downloading in progress. You want to quit?", "Exit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes) client.CancelAsync();
            }
        }

        public void DeleteDir(string path)
        {
            if (Directory.Exists(path))
            {
                if (path.Contains("csco"))
                {
                    string[] fileCollection = Directory.GetFiles(path);

                    foreach (String file in fileCollection) File.SetAttributes(file, FileAttributes.Normal);
                }

                System.IO.DirectoryInfo di = new DirectoryInfo(path);

                foreach (FileInfo file in di.GetFiles()) file.Delete();

                foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);

                Directory.Delete(path);
            }
        }

        private bool RemoteFileExists(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

                request.Method = "HEAD";

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                response.Close();

                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }
    }
}
