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
        string v_url = "http://cs-reload.pl/csco/";

        double latest_version = 0.0;
        double installed_version = 0.0;

        bool done = false;
        bool downloading = false;
        bool update = false;

        WebClient client;

        DialogResult r;

        string latest_url = "";
        string csco_path = "";

        public Form1()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckVersion();
        }

        private void CheckVersion()
        {
            this.ActiveControl = label1;

            latest_version = 0.0;
            installed_version = 0.0;

            string v_file = "NULL";
            string local_v_file = "NULL";

            button1.Enabled = false;

            csco_path = textBox1.Text + "/steamapps/sourcemods";

            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    v_file = client.DownloadString(v_url);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error! Unable to get version file.\nCheck your internet connection!");
            }

            if (v_file != "NULL")
            {
                latest_version = 0.0;

                string buff = "";

                foreach (char c in v_file)
                {
                    if (c == ' ')
                    {
                        break;
                    }
                    else
                    {
                        buff += c;
                    }
                }

                latest_version = Convert.ToDouble(buff);

                buff = "";

                bool reading = false;

                foreach (char c in v_file)
                {
                    if (!reading)
                    {
                        if (c == ' ')
                        {
                            reading = true;
                        }
                    }
                    else
                    {
                        buff += c;
                    }
                }

                latest_url = buff;
            }

            if (Directory.Exists(csco_path + "/csco"))
            {

                try
                {
                    local_v_file = File.ReadAllText(csco_path + "/csco/version.txt");
                    installed_version = Convert.ToDouble(local_v_file);
                }
                catch (Exception)
                {

                    r = MessageBox.Show("File version.txt not found. Do you have the latest version?",
                        "Error", MessageBoxButtons.YesNo);

                    if (r == DialogResult.Yes)
                    {
                        File.WriteAllText(csco_path + "/csco/version.txt", "" + latest_version);

                        installed_version = latest_version;
                    }
                    else
                    {
                        label3.Text = "Unknown";
                        installed_version = -0.1;
                    }
                }
            }

            if (installed_version > 0.0)
            {
                label3.Text = Convert.ToString(installed_version);
            } 
            else if (installed_version == 0.0)
            {
                label3.Text = "None";
            }

            label4.Text = Convert.ToString(latest_version);

            if (latest_url != "")
            {
                if (!Directory.Exists(csco_path + "/csco"))
                {
                    button1.Text = "Download";
                    button1.Enabled = true;
                }
                else
                {
                    if (installed_version < latest_version)
                    {
                        button1.Text = "Update";
                        button1.Enabled = true;
                        update = true;
                    }
                    else
                    {
                        button1.Text = "No Update Required";
                        button1.Enabled = false;
                    }
                }
            }
            else
            {
                button1.Enabled = false;
                button1.Text = "Update Not Possible!";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBox1.Text + "/Steam.exe"))
            {
                csco_path = textBox1.Text + "/steamapps/sourcemods";

                IProgress<double> progress = new Progress<double>(b => progressBar1.Value = (int)b);

                if (!done && button1.Enabled)
                {
                    button1.Text = "Downloading...";

                    button1.Enabled = false;

                    textBox1.Enabled = false;

                    downloading = true;

                    delete_dir(csco_path + "/Temp");

                    Directory.CreateDirectory(csco_path + "/Temp");

                    try
                    {
                        client = new WebClient();
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                        client.DownloadFileAsync(new Uri(latest_url), csco_path + "/Temp/csco.zip");
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Error: " + ee.Message);
                        done = true;
                    }
                }
                else
                {
                    button1.Text = "Update In Progress!";
                    button1.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("Error: Check path to your Steam directory!");
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            if(percentage > 99.5)
            {
                button1.Text = "Installing...";
            }

            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                delete_dir(csco_path + "/Temp");
            }
            else
            {
                try
                {
                    if (!done)
                    {
                        done = true;

                        progressBar1.Maximum = 100;

                        progressBar1.Value = 100;

                        delete_dir(csco_path + "/csco");

                        try
                        {
                            ZipFile.ExtractToDirectory(csco_path + "/Temp/csco.zip", csco_path + "/");
                        }
                        catch (Exception ee)
                        {
                            MessageBox.Show("Error: " + ee.Message);
                        }

                        if (File.Exists(csco_path + "/HostMe.txt"))
                        {
                            File.Delete(csco_path + "/HostMe.txt");
                        }

                        if (File.Exists(csco_path + "/ReadMe.txt"))
                        {
                            File.Delete(csco_path + "/ReadMe.txt");
                        }

                        if (!File.Exists(csco_path + "/csco/version.txt"))
                        {
                            File.WriteAllText(csco_path + "/csco/version.txt", "" + latest_version);
                        }

                        delete_dir(csco_path + "/Temp");

                        if (!update)
                        {
                            foreach (var process in Process.GetProcessesByName("Steam"))
                            {
                                process.Kill();
                            }
                        }

                        label3.Text = Convert.ToString(latest_version);

                        button1.Text = "Installation Complete";

                        downloading = false;
                    }
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
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (!string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (downloading)
            {
                var res = MessageBox.Show(this, "Downloading in progress. You want to quit?", "Exit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (res == DialogResult.Yes)
                {
                    client.CancelAsync();
                }
            }
        }

        public void delete_dir(string path)
        {
            if (Directory.Exists(path))
            {
                if (path.Contains("csco"))
                {
                    string[] fileCollection = Directory.GetFiles(path);

                    foreach (String file in fileCollection)
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                    }
                }

                System.IO.DirectoryInfo di = new DirectoryInfo(path);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                Directory.Delete(path);
            }
        }
    }
}
