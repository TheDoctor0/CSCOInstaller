using System;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Net;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace CSCOInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        string versionUrl = "http://cs-reload.pl/csco/";
        string updateUrl = "";
        string steamDirectory = "";

        double latestVersion = 0.0;
        double installedVersion = 0.0;

        bool downloading = false;
        bool update = false;

        WebClient client;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, RoutedEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            RegistryKey regKey = Registry.CurrentUser;

            regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

            if (regKey != null)
            {
                string[] steamPath = regKey.GetValue("SourceModInstallPath").ToString().Split(new string[] { @"\steamapps\sourcemods" }, StringSplitOptions.RemoveEmptyEntries);

                textBoxSteam.Text = steamPath[0];
            }

            CheckVersion();
        }

        private void CheckVersion()
        {
            latestVersion = 0.0;
            installedVersion = 0.0;

            button.IsEnabled = true;

            string versionFile = "NULL";

            steamDirectory = textBoxSteam.Text + @"\steamapps\sourcemods";

            try
            {
                using (WebClient client = new WebClient()) versionFile = client.DownloadString(versionUrl);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Unable to get version file.\nCheck your internet connection!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (Directory.Exists(steamDirectory + @"\csco"))
            {
                try
                {
                    string localVersionFile = File.ReadAllText(steamDirectory + @"\csco\version.txt");

                    installedVersion = Convert.ToDouble(localVersionFile);

                    labelInstalled.Content = installedVersion.ToString("0.0");
                }
                catch (Exception)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("CS:CO installed, but version.txt not found.\nDo you have the latest version?", "Version", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        File.WriteAllText(steamDirectory + @"\csco\version.txt", "" + latestVersion);

                        installedVersion = latestVersion;

                        labelInstalled.Content = installedVersion.ToString("0.0");
                    }
                    else
                    {
                        labelInstalled.Content = "Unknown";

                        installedVersion = -1.0;
                    }
                }
            }
            else labelInstalled.Content = "None";

            labelLatest.Content = latestVersion.ToString("0.0");

            if (updateUrl != "")
            {
                if (!Directory.Exists(steamDirectory + @"\csco")) button.Content = "Download";
                else
                {
                    if (installedVersion < latestVersion)
                    {
                        button.Content = "Update";

                        update = true;
                    }
                    else
                    {
                        button.Content = "No Update Required";

                        button.IsEnabled = false;
                    }
                }
            }
            else
            {
                button.Content = "Update Not Possible!";

                button.IsEnabled = false;
            }
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(textBoxSteam.Text + @"\Steam.exe"))
            {
                steamDirectory = textBoxSteam.Text + @"\steamapps\sourcemods";

                IProgress<double> progress = new Progress<double>(b => progressBar.Value = (int)b);

                DeleteDir(steamDirectory + @"\Temp");

                Directory.CreateDirectory(steamDirectory + @"\Temp");

                try
                {
                    client = new WebClient();

                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadFileAsync(new Uri(updateUrl), steamDirectory + "/Temp/csco.zip");

                    button.Content = "Downloading...";

                    button.IsEnabled = false;

                    textBoxSteam.IsEnabled = false;

                    downloading = true;
                }
                catch (Exception ee)
                {
                    System.Windows.MessageBox.Show(ee.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Check path to your Steam directory!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double percentage = double.Parse(e.BytesReceived.ToString()) / double.Parse(e.TotalBytesToReceive.ToString()) * 100;

            if (percentage >= 99.9) button.Content = "Installing...";
            else button.Content = "Downloading... " + Convert.ToString(Math.Round(percentage)) + "%";

            progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null) button.Content = "Downloading error!";
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
                        System.Windows.MessageBox.Show(ee.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        System.Windows.MessageBox.Show("Installation complete, now Steam is launching.\nCounter-Strike: Classic Offensive will appear in Library.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                        System.Diagnostics.Process.Start(textBoxSteam.Text + "/Steam.exe");

                        button.Content = "Installation Complete";
                    }
                    else button.Content = "Update Complete";

                    labelInstalled.Content = latestVersion.ToString("0.0");

                    downloading = false;
                }
                catch (Exception ee)
                {
                    System.Windows.MessageBox.Show(ee.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void textBox_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();

            folderBrowser.ShowDialog();

            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                textBoxSteam.Text = folderBrowser.SelectedPath;
                CheckVersion();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (downloading)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Downloading in progress. You want to quit?", "Exit",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes) client.CancelAsync();
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
