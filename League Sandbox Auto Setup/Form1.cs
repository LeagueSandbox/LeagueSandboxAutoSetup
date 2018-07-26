using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SegmentDownloader.Core;
using SegmentDownloader.Protocol;
using SharpCompress.Archives;
using SharpCompress.Readers;
using SharpCompress.Common;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Client;
using NuGet.Common;
using NuGet.ContentModel;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Repositories;
using NuGet.RuntimeModel;

namespace League_Sandbox_Auto_Setup
{
    public partial class leagueSandboxAutoSetupForm : Form
    {
        public leagueSandboxAutoSetupForm()
        {
            InitializeComponent();

            this.FormClosing += (_, _2) =>
            {
                Environment.Exit(0);
            };
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            installDirectoryText.Enabled = false;
            startButton.Enabled = false;
            Directory.CreateDirectory(installDirectoryText.Text);
            //startCloningRepositories();

            startVisualStudioFirstRun(Path.Combine(installDirectoryText.Text, "League_Sandbox_Client"));
        }

        private void startCloningRepositories()
        {
            cloningProgressLabel.Text = "--";
            Directory.CreateDirectory(installDirectoryText.Text);
            var cloningPath = Path.Combine(installDirectoryText.Text, "GameServer");

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                CloneOptions options = new CloneOptions()
                {
                    OnTransferProgress = progress =>
                    {
                        cloningProgressLabel.Invoke(new Action(() =>
                        {
                            cloningProgressLabel.Text = $"Objects: {progress.ReceivedObjects} of {progress.TotalObjects}, Downloaded {progress.ReceivedBytes / 1024 / 1024} MB";
                        }));
                        return true;
                    },
                    OnProgress = progress =>
                    {
                        cloningProgressLabel.Invoke(new Action(() =>
                        {
                            cloningProgressLabel.Text = progress;
                        }));
                        return true;
                    },
                    OnUpdateTips = (_, _2, _3) =>
                    {
                        cloningProgressLabel.Invoke(new Action(() =>
                        {
                            cloningProgressLabel.Text = "Updating Tips";
                        }));
                        return true;
                    },
                    OnCheckoutProgress = (string path, int completedSteps, int totalSteps) =>
                    {
                        cloningProgressLabel.Invoke(new Action(() =>
                        {
                            cloningProgressLabel.Text = $"Checkout: {completedSteps}/{totalSteps}";
                        }));
                    },
                    RepositoryOperationStarting = progress =>
                    {
                        cloningProgressLabel.Invoke(new Action(() =>
                        {
                            cloningProgressLabel.Text = "Cloning...";
                        }));
                        return true;
                    },
                    RepositoryOperationCompleted = progress =>
                    {
                        cloningProgressLabel.Invoke(new Action(() =>
                        {
                            cloningProgressLabel.Text = $"Cloned a repository";
                        }));
                    }
                };
                options.BranchName = "ClientAutoLauncher";
                options.RecurseSubmodules = true;
                if (!Directory.Exists(cloningPath))
                {
                    Directory.CreateDirectory(cloningPath);
                    Repository.Clone("https://github.com/LeagueSandbox/GameServer", cloningPath, options);
                }
                cloningProgressLabel.Invoke(new Action(() =>
                {
                    cloningProgressLabel.Text = "✔️";

                    startVisualStudioFirstRun(Path.Combine(installDirectoryText.Text, "League_Sandbox_Client"));
                    //startDownloadingClient();
                }));
            }).Start();
        }

        private void startDownloadingClient()
        {
            downloadingProgressLabel.Text = "--";

            ProtocolProviderFactory.RegisterProtocolHandler("http", typeof(HttpProtocolProvider));
            var resourceLocation = ResourceLocation.FromURL("http://gamemakersgarage.com/League_Sandbox_Client.7z");
            // local file path
            var uri = new Uri("http://gamemakersgarage.com/League_Sandbox_Client.7z");
            var fileName = uri.Segments.Last();
            var localFilePath = Path.Combine(installDirectoryText.Text, fileName);
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;

            // register download ended event
            DownloadManager.Instance.DownloadEnded += (_, _2) =>
            {
                downloadingProgressLabel.Invoke(new Action(() =>
                {
                    timer.Stop();
                    downloadingProgressLabel.Text = "✔️";
                    startUnzippingClient(localFilePath);
                }));
            };

            // create downloader with 8 segments
            var downloader = DownloadManager.Instance.Add(resourceLocation, null, localFilePath, 25, false);
            // start download
            downloader.Start();

            timer.Tick += (_, _2) =>
            {
                downloadingProgressLabel.Text = $"{Math.Round(downloader.Progress, 2)}% - {Math.Round(downloader.Rate / 1024 / 1024, 2)} MB/s";
            };
            timer.Start();
        }
        private void startUnzippingClient(String localFilePath)
        {
            unzippingProgressLabel.Text = "--";
            var directoryPath = Path.GetDirectoryName(localFilePath);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                var entryCount = 0;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (var archive = ArchiveFactory.Open(localFilePath, new ReaderOptions() { LookForHeader = true }))
                {
                    var reader = archive.ExtractAllEntries();
                    while (reader.MoveToNextEntry())
                    {
                        if (stopwatch.Elapsed.Milliseconds >= 100)
                        {
                            unzippingProgressLabel.Invoke(new Action(() =>
                            {
                                unzippingProgressLabel.Text = $"{Math.Round((double)entryCount / archive.Entries.Count() * 100, 2)}% - {entryCount} / {archive.Entries.Count()} files";
                            }));
                            stopwatch.Restart();
                        }
                        reader.WriteEntryToDirectory(directoryPath,
                            new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true,
                                PreserveAttributes = true,
                                PreserveFileTime = true
                            });
                        entryCount += 1;
                    }
                }

                File.Delete(localFilePath);

                unzippingProgressLabel.Invoke(new Action(() =>
                {
                    unzippingProgressLabel.Text = "✔️";
                    startVisualStudioFirstRun(localFilePath);
                }));
            }).Start();
        }
        private void startVisualStudioFirstRun(String leagueInstallFolder)
        {
            launchingProgressLabel.Text = "Setting up Configs";

            //Set up GameServer configs
            var gameServerFolder = Path.Combine(installDirectoryText.Text, "GameServer");
            var templatePath = Path.Combine(gameServerFolder, "GameServerApp\\Settings\\GameServerSettings.json.template");
            var newPath = Path.Combine(gameServerFolder, "GameServerApp\\Settings\\GameServerSettings.json");
            var templateString = File.ReadAllText(templatePath);
            var configTemplatePath = Path.Combine(gameServerFolder, "GameServerApp\\Settings\\GameInfo.json.template");
            var configNewPath = Path.Combine(gameServerFolder, "GameServerApp\\Settings\\GameInfo.json");
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
            if (File.Exists(configNewPath))
            {
                File.Delete(configNewPath);
            }
            JObject json = JObject.Parse(templateString);
            json["clientLocation"] = leagueInstallFolder;
            json["autoStartClient"] = true;
            File.WriteAllText(newPath, json.ToString());
            File.Copy(configTemplatePath, configNewPath);

            launchingProgressLabel.Text = "Downloading Nuget";
            //Download Nuget to restore packages
            String nugetLink = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";
            String nugetLocation = Path.GetFullPath(Path.Combine(leagueInstallFolder, "nuget.exe"));
            if (File.Exists(nugetLocation))
            {
                File.Delete(nugetLocation);
            }

            DownloadFile(nugetLink, nugetLocation, (progress)=> {
                launchingProgressLabel.Text = "Downloading Nuget: " + progress + "%";
            }, ()=> {
                launchingProgressLabel.Text = "Getting Dependencies";
                
                System.Diagnostics.Process nugetProcess = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo nugetStartInfo = new System.Diagnostics.ProcessStartInfo();
                nugetStartInfo.FileName = nugetLocation;
                nugetStartInfo.Arguments = $"restore \"{gameServerFolder}\"";
                nugetProcess.StartInfo = nugetStartInfo;
                nugetProcess.Start();

                File.Delete(nugetLocation);

                launchingProgressLabel.Text = "Looking for Visual Studio";

                var solutionPath = Path.GetFullPath(Path.Combine(gameServerFolder, "GameServer.sln"));

                var allFiles = GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "devenv.com").ToList();

                if (allFiles.Count > 0)
                {
                    var devenvPath = allFiles[0];

                    launchingProgressLabel.Text = "Starting Visual Studio";

                    //Run build
                    //"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.com" "C:/LeagueSandbox/GameServer/GameServer.sln" /build Debug

                    //Open in visual studio and run
                    //"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" "C:/LeagueSandbox/GameServer/GameServer.sln" /Command "Debug.Start"
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = devenvPath;
                    startInfo.Arguments = $"\"{solutionPath}\" /Command \"Debug.Start\"";
                    process.StartInfo = startInfo;
                    process.Start();
                }
            });
        }

        public static IEnumerable<string> GetFiles(string root, string searchPattern)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var path = pending.Pop();
                string[] next = null;
                try
                {
                    next = Directory.GetFiles(path, searchPattern);
                }
                catch { }
                if (next != null && next.Length != 0)
                    foreach (var file in next) yield return file;
                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var subdir in next) pending.Push(subdir);
                }
                catch { }
            }
        }

        public static void DownloadFile(String link, String location, Action<int> downloadProgress, Action finishedDownload)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                wc.DownloadProgressChanged += (_, progress) =>
                {
                    downloadProgress((int)progress.ProgressPercentage);
                };
                wc.DownloadDataCompleted += (_, _2) =>
                {
                    finishedDownload();
                };
                wc.DownloadFileAsync(new System.Uri(link), location);
            }
        }
    }
}