﻿using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SegmentDownloader.Core;
using SegmentDownloader.Protocol;
using SharpCompress.Archives;
using SharpCompress.Readers;
using SharpCompress.Common;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using League_Sandbox_Auto_Setup.Util;
using System.Collections.Generic;
using Microsoft.Win32;

namespace League_Sandbox_Auto_Setup
{
    public partial class leagueSandboxAutoSetupForm : Form
    {
        static string Client_Folder_Name = "League_Sandbox_Client";
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
            startCloningRepositories();
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
                options.BranchName = "Indev";
                options.RecurseSubmodules = true;
                if (!Directory.Exists(cloningPath))
                {
                    Directory.CreateDirectory(cloningPath);
                    Repository.Clone("https://github.com/LeagueSandbox/GameServer", cloningPath, options);
                }
                cloningProgressLabel.Invoke(new Action(() =>
                {
                    cloningProgressLabel.Text = "✔️";

                    startDownloadingClient();
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
            var localFilePath = Path.Combine(installDirectoryText.Text, Client_Folder_Name + ".7z");
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
                    startUnzippingClient();
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
        private void startUnzippingClient()
        {
            var localFilePath = Path.Combine(installDirectoryText.Text, Client_Folder_Name + ".7z");
            unzippingProgressLabel.Text = "--";
            var directoryPath = installDirectoryText.Text;

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
                    startSettingUpTestbox();
                }));
            }).Start();
        }
        private void startSettingUpTestbox()
        {
            String testboxLink = "http://gamemakersgarage.com/LeagueUI.7z";
            String testboxLocation = Path.GetFullPath(Path.Combine(installDirectoryText.Text, "LeagueUI.7z"));
            if (File.Exists(testboxLocation))
            {
                File.Delete(testboxLocation);
            }

            Download.File(testboxLink, testboxLocation, (progress) =>
            {
                installingTestboxLabel.Text = "Downloading Testbox: " + progress + "%";
            }, () =>
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    var entryCount = 0;
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (var archive = ArchiveFactory.Open(testboxLocation, new ReaderOptions() { LookForHeader = true }))
                    {
                        var reader = archive.ExtractAllEntries();
                        while (reader.MoveToNextEntry())
                        {
                            if (stopwatch.Elapsed.Milliseconds >= 100)
                            {
                                installingTestboxLabel.Invoke(new Action(() =>
                                {
                                    installingTestboxLabel.Text = $"{Math.Round((double)entryCount / archive.Entries.Count() * 100, 2)}% - {entryCount} / {archive.Entries.Count()} files";
                                }));
                                stopwatch.Restart();
                            }
                            reader.WriteEntryToDirectory(installDirectoryText.Text,
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

                    File.Delete(testboxLocation);

                    unzippingProgressLabel.Invoke(new Action(() =>
                    {
                        string desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        Utility.CreateShortcut("LeagueUI", desktopDirectory, Path.GetFullPath(Path.Combine(installDirectoryText.Text, "LeagueUI", "LeagueUI.exe")), "League Sandbox Matchmaking Client", Path.GetFullPath(Path.Combine(installDirectoryText.Text, "LeagueUI", "assets", "sandbox-app-icon.ico")));
                        installingTestboxLabel.Text = "✔️";
                        startVisualStudioFirstRun();
                    }));
                }).Start();
            });
        }
        private void startVisualStudioFirstRun()
        {
            var leagueInstallFolder = Path.GetFullPath(Path.Combine(installDirectoryText.Text, Client_Folder_Name));
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
            String nugetLocation = Path.GetFullPath(Path.Combine(installDirectoryText.Text, "nuget.exe"));
            if (File.Exists(nugetLocation))
            {
                File.Delete(nugetLocation);
            }

            Download.File(nugetLink, nugetLocation, (progress)=> {
                launchingProgressLabel.Text = "Downloading Nuget: " + progress + "%";
            }, ()=> {
                launchingProgressLabel.Text = "Getting Dependencies";
                
                System.Diagnostics.Process nugetProcess = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo nugetStartInfo = new System.Diagnostics.ProcessStartInfo();
                nugetStartInfo.WorkingDirectory = gameServerFolder;
                nugetStartInfo.FileName = nugetLocation;
                nugetStartInfo.Arguments = $"restore \"{gameServerFolder}\"";
                nugetStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                nugetProcess.StartInfo = nugetStartInfo;
                nugetProcess.Start();
                nugetProcess.WaitForExit();

                File.Delete(nugetLocation);

                launchingProgressLabel.Text = "Looking for Visual Studio";

                List<string> allFiles = new List<string>();
                String devenvPath;

                try
                {
                    using (RegistryKey key = Registry.LocalMachine
                        .OpenSubKey("Software\\Wow6432Node\\Microsoft\\VisualStudio\\SxS\\VS7"))
                    {
                        if (key != null)
                        {
                            String[] tmp = key.GetValueNames();


                            foreach (string str in tmp)
                            {
                                allFiles.Add(key.GetValue(str).ToString());
                            }

                            //Should always pick the latest version of VS installed
                            devenvPath = allFiles[0] + "Common7\\IDE\\devenv.exe";
                        }
                        else
                        {
                            launchingProgressLabel.Text = "Could not find Visual Studio";
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    launchingProgressLabel.Text = "Defaulting to fallback method";
                    allFiles = Search.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder
                        .ProgramFilesX86), "devenv.com").ToList();
                    devenvPath = allFiles[0];
                }


                var solutionPath = Path.GetFullPath(Path.Combine(gameServerFolder, "GameServer.sln"));

                launchingProgressLabel.Text = "Starting Visual Studio";

                //Run build -- Not Necessary?
                //"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.com" "C:/LeagueSandbox/GameServer/GameServer.sln" /build Debug

                //Open in visual studio and run
                //"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" "C:/LeagueSandbox/GameServer/GameServer.sln" /Command "Debug.Start"
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = devenvPath;
                startInfo.Arguments = $"\"{solutionPath}\" /Command \"Debug.Start\"";
                process.StartInfo = startInfo;
                process.Start();
                launchingProgressLabel.Text = "✔️";
                finishProgressLabel.Text = "✔️";
            });
        }
    }
}