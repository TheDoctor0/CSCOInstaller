using System;
using System.Windows;
using MahApps.Metro.Controls;
using System.Net;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CSCOInstaller
{
    public partial class MainWindow : MetroWindow
    {
        string rawRepoUrl = "https://raw.githubusercontent.com/TheDoctor0/CSCOInstaller/master/";
        string updateUrl = "";
        string versionText = "";
        string steamDirectory = "";

        double latestVersion = 0.0;
        double installedVersion = 0.0;
        double localVersion = 1.51;

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

            if (!checkUpdates())
            {
                RegistryKey regKey = Registry.CurrentUser;

                regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

                if (regKey != null)
                {
                    string[] steamPath = regKey.GetValue("SourceModInstallPath").ToString().Split(new string[] { @"\steamapps\sourcemods" }, StringSplitOptions.RemoveEmptyEntries);

                    textBoxSteam.Text = steamPath[0];
                }

                checkVersion();
            }
        }

        private void checkVersion()
        {
            latestVersion = 0.0;
            installedVersion = 0.0;

            button.IsEnabled = true;

            string versionFile = String.Empty;

            steamDirectory = textBoxSteam.Text + @"\steamapps\sourcemods";

            try
            {
                using (WebClient client = new WebClient()) versionFile = client.DownloadString(rawRepoUrl + "version.txt");
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Unable to get version file.\nCheck your internet connection!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (versionFile != String.Empty)
            {
                double tempVersion = 0.0;

                foreach (string line in versionFile.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] version = line.Split();

                    tempVersion = Convert.ToDouble(version[0]);

                    if (remoteFileExists(version[1]) && tempVersion > latestVersion)
                    {
                        versionText = version[0];
                        updateUrl = version[1];
                        latestVersion = tempVersion;
                    }
                }
            }

            if (Directory.Exists(steamDirectory + @"\csco"))
            {
                try
                {
                    string localVersionFile = File.ReadAllText(steamDirectory + @"\csco\version.txt");

                    installedVersion = Convert.ToDouble(Regex.Match(localVersionFile, @"[0-9]+\.[0-9]+").Value);

                    labelInstalledVersion.Content = installedVersion.ToString();
                }
                catch (Exception)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Classic Offensive installed, but version.txt was not found.\nDo you have the latest version?", "Version", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        File.WriteAllText(steamDirectory + @"\csco\version.txt", "" + versionText);

                        installedVersion = latestVersion;

                        labelInstalledVersion.Content = versionText;
                    }
                    else
                    {
                        labelInstalledVersion.Content = "Unknown";

                        installedVersion = -1.0;
                    }
                }
            }
            else
            {
                labelInstalledVersion.Content = "None";
            }

            labelLatestVersion.Content = versionText == String.Empty ? "None" : versionText;

            if (updateUrl != "")
            {
                if (!Directory.Exists(steamDirectory + @"\csco"))
                {
                    button.Content = "Download";
                }
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

                deleteDir(steamDirectory + @"\Temp");

                Directory.CreateDirectory(steamDirectory + @"\Temp");

                try
                {
                    client = new WebClient();

                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadFileCompleted);
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

        private void downloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                button.Content = "Downloading error!";
            }
            else if (e.Cancelled)
            {
                deleteDir(steamDirectory + "/Temp");
            }
            else
            {
                try
                {
                    if (File.Exists(steamDirectory + "/csco/maps/workshop"))
                    {
                        File.SetAttributes(steamDirectory + "/csco/maps/workshop", FileAttributes.Normal);
                        File.Delete(steamDirectory + "/csco/maps/workshop");
                    }

                    if (Directory.Exists(steamDirectory + "/csco/maps/workshop")) Directory.Delete(steamDirectory + "/csco/maps/workshop");

                    deleteDir(steamDirectory + "/csco");

                    try
                    {
                        ZipFile.ExtractToDirectory(steamDirectory + "/Temp/csco.zip", steamDirectory + "/");
                    }
                    catch (Exception ee)
                    {
                        System.Windows.MessageBox.Show(ee.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    deleteDir(steamDirectory + "/Temp");

                    if (File.Exists(steamDirectory + "/HostMe.txt")) File.Delete(steamDirectory + "/HostMe.txt");
                    if (File.Exists(steamDirectory + "/ReadMe.txt")) File.Delete(steamDirectory + "/ReadMe.txt");
                    if (File.Exists(steamDirectory + "/FixedNotes.txt")) File.Delete(steamDirectory + "/FixedNotes.txt");
                    if (File.Exists(steamDirectory + "/csco/maps/workshop"))
                    {
                        File.SetAttributes(steamDirectory + "/csco/maps/workshop", FileAttributes.Normal);
                        File.Delete(steamDirectory + "/csco/maps/workshop");
                    }

                    File.WriteAllText(steamDirectory + "/csco/version.txt", "" + versionText);

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

                    ProcessStartInfo processs = new ProcessStartInfo("cmd.exe");

                    processs.WorkingDirectory = steamDirectory + @"\csco\maps";
                    processs.Arguments = @"/c mklink /J workshop " + "\"" + textBoxSteam.Text + @"\steamapps\common\Counter-Strike Global Offensive\csgo\maps\workshop" + "\"";

                    Process.Start(processs);

                    if (!update)
                    {
                        foreach (var process in Process.GetProcessesByName("Steam")) process.Kill();

                        System.Windows.MessageBox.Show("Installation complete, now Steam is launching.\nClassic Offensive will appear in Library.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                        System.Diagnostics.Process.Start(textBoxSteam.Text + "/Steam.exe");

                        button.Content = "Installation Complete";
                    }
                    else
                    {
                        button.Content = "Update Complete";
                    }

                    labelInstalledVersion.Content = versionText;

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

                checkVersion();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (downloading)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Downloading in progress. You want to quit?", "Exit",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    client.CancelAsync();

                    deleteDir(steamDirectory + "/Temp");
                }
            }
        }

        public void deleteDir(string path)
        {
            if (Directory.Exists(path))
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(path);

                foreach (FileInfo file in di.GetFiles()) file.Delete();

                foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);

                Directory.Delete(path);
            }
        }

        private bool remoteFileExists(string url)
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

        private bool checkUpdates()
        {
            String updateFile = String.Empty;
            Double updateVersion = 0.0;

            if (File.Exists("updater.bat")) System.Diagnostics.Process.Start("updater.bat");

            try
            {
                using (WebClient client = new WebClient()) updateFile = client.DownloadString(rawRepoUrl + "update.txt");
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Unable to get version file.\nCheck your internet connection!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (updateFile != String.Empty)
            {
                updateVersion = Convert.ToDouble(Regex.Match(updateFile, @"[0-9]+\.[0-9]+").Value);
            }
            else
            {
                return false;
            }

            if (updateVersion > localVersion)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("New Installer version is available. Do you want to update?", "Update",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    System.IO.FileInfo file = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    System.IO.File.Move(file.FullName, file.DirectoryName + "\\" + file.Name + ".old");

                    WebClient updateClient = new WebClient();

                    updateClient.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadUpdateCompleted);
                    updateClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
                    updateClient.DownloadFileAsync(new Uri(updateFile), file.DirectoryName + "\\" + file.Name);

                    labelLatestVersion.Content = updateVersion.ToString("0.00");
                    labelInstalledVersion.Content = localVersion.ToString("0.00");

                    button.Content = "Installer update in progress...";

                    button.IsEnabled = false;

                    textBoxSteam.IsEnabled = false;

                    return true;
                }
            }

            return false;
        }

        private void downloadUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            System.IO.FileInfo file = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

            using (StreamWriter sw = File.CreateText("updater.bat"))
            {
                sw.WriteLine("del " + @"""" + file.Name + ".old");
                sw.WriteLine("del updater.bat");
            }

            System.Diagnostics.Process.Start(file.DirectoryName + "/" + file.Name);

            System.Windows.Application.Current.Shutdown();
        }

        void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double percentage = double.Parse(e.BytesReceived.ToString()) / double.Parse(e.TotalBytesToReceive.ToString()) * 100;

            if (percentage >= 99.9)
            {
                button.Content = "Installing...";
            }
            else
            {
                button.Content = "Downloading... " + Convert.ToString(Math.Round(percentage)) + "%";
            }

            progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
    }
}
