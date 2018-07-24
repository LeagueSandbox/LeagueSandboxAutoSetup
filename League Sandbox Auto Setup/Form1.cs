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
            startUnzippingClient(Path.Combine(installDirectoryText.Text, "League_Sandbox_Client.7z"));
            //startCloningRepositories();
        }

        private void startCloningRepositories()
        {
            cloningProgressLabel.Text = "--";
            var cloningPath = Path.Combine(installDirectoryText.Text, "GameServer");
            Directory.CreateDirectory(cloningPath);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                CloneOptions options = new CloneOptions()
                {
                    OnTransferProgress = progress =>
                    {
                        cloningProgressLabel.Invoke(new Action(() =>
                        {
                            cloningProgressLabel.Text = $"Objects: {progress.ReceivedObjects} of {progress.TotalObjects}, MB: {progress.ReceivedBytes / 1024 / 1024}";
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
                Repository.Clone("https://github.com/LeagueSandbox/GameServer", cloningPath, options);
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
            var fileName = uri.Segments.Last();
            var localFilePath = Path.Combine(installDirectoryText.Text, fileName);

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
            var downloader = DownloadManager.Instance.Add(resourceLocation, null, localFilePath, 50, false);
            // start download
            downloader.Start();

            timer.Tick += (_, _2) =>
            {
                downloadingProgressLabel.Text = $"{Math.Round(downloader.Progress, 2)}% - {Math.Round(downloader.Rate / 1024 / 1024, 2)} MB/s";
            };
            timer.Start();

            /* Old method
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                wc.DownloadProgressChanged += (_, progress) =>
                {
                    downloadingProgressLabel.Text = $"{progress.ProgressPercentage}% - {progress.BytesReceived / 1024 / 1024}MB / {progress.TotalBytesToReceive / 1024 / 1024}MB";
                };
                wc.DownloadDataCompleted += (_, _2) =>
                {
                    downloadingProgressLabel.Text = "✔️";
                    startUnzippingClient();
                };
                wc.DownloadFileAsync(new System.Uri("http://gamemakersgarage.com/League_Sandbox_Client.7z"),
                    Path.Combine(installDirectoryText.Text, "League_Sandbox_Client.7z"));
            }
            */
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
                    startVisualStudioFirstRun();
                }));
            }).Start();
        }
        private void startVisualStudioFirstRun()
        {
            launchingProgressLabel.Text = "--";
        }
    }
}