using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SharpCompress.Archives;
using SharpCompress.Readers;
using SharpCompress.Common;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using League_Sandbox_Auto_Setup.Util;

namespace League_Sandbox_Auto_Setup
{
    public partial class LeagueSandboxAutoSetupForm : Form
    {
        private bool _abortInitiated;
        private bool _setupStarted;
        private bool _convertProjectsToX86;
        private const string ClientArchive = "lol_game_client_sln_0.0.1.68.7z";

        public LeagueSandboxAutoSetupForm()
        {
            InitializeComponent();

            this.FormClosing += (_, _2) =>
            {
                try
                {
                    _abortInitiated = true;
                    abortText.Visible = true;
                    startButton.Text = "Start";
                    startButton.Enabled = false;

                    Environment.Exit(0);
                }
                catch (Exception)
                {

                }
                
            };
        }
        private void OnAbortSuccessfully()
        {
            _setupStarted = false;
            _abortInitiated = false;
            startButton.Enabled = true;
            abortText.Visible = false;
        }
        
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!_setupStarted)
            {
                _convertProjectsToX86 = 
                    MessageBox.Show("Would you like to convert all projects to x86 - " +
                    "currently fixes DLL issues with: ENet DLL for x64/AnyCpu?", 
                    "Convert to x86",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

                installDirectoryText.Enabled = false;
                browseButton.Enabled = false;
                startButton.Text = "Abort";
                Directory.CreateDirectory(installDirectoryText.Text);
                StartCloningRepositories();
                _setupStarted = true;
            }
            else
            {
                _abortInitiated = true;
                abortText.Visible = true;
                startButton.Text = "Start";
                startButton.Enabled = false;
            }
        }

        private void StartCloningRepositories()
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
                options.RecurseSubmodules = false;                

//                git submodule init
//                git submodule update

                if (!Directory.Exists(cloningPath))
                {
                    Directory.CreateDirectory(cloningPath);
                    options.BranchName = "indev";  // Branch for GameServer
                    Repository.Clone("https://github.com/LeagueSandbox/GameServer", cloningPath, options);
                    options.BranchName = "master"; // Branch for LeaguePackets
                    Repository.Clone("https://github.com/LeagueSandbox/LeaguePackets", Path.Combine(cloningPath, "LeaguePackets"), options);
                    options.BranchName = "indev"; // Branch for LeagueSandbox-Default
                    Repository.Clone("https://github.com/LeagueSandbox/LeagueSandbox-Default", Path.Combine(cloningPath, "Content\\LeagueSandbox-Default"), options);
                }
                cloningProgressLabel.Invoke(new Action(() =>
                {
                    cloningProgressLabel.Text = "✔️";

                    

                    if(_convertProjectsToX86)
                    {
                        ConvertProjectsToX86AfterCloning(cloningPath);
                    }

                    StartDownloadingClient();
                }));
            }).Start();
        }

        private void ConvertProjectsToX86AfterCloning(string cloningPath)
        {
            foreach (var item in Directory.GetFiles(cloningPath))
            {
                if( Path.GetExtension(item).ToLower() == ".csproj")
                {
                    var text = File.ReadAllText(item);

                    if(text.Contains("<Prefer32Bit>false</Prefer32Bit>"))
                    {
                        File.WriteAllText(item, text.
                            Replace("<Prefer32Bit>false</Prefer32Bit>", "<Prefer32Bit>true</Prefer32Bit>").
                            Replace("<PlatformTarget>AnyCPU</PlatformTarget>", "<PlatformTarget>x86</PlatformTarget>"));
                    }
                    else
                    {
                        File.WriteAllText(item, File.ReadAllText(item).
                            Replace("</Project>", @"
<PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|AnyCPU'"">
    <PlatformTarget>x86</PlatformTarget>
</PropertyGroup>
</Project>"));
                    }
                }
            }

            foreach (var item in Directory.GetDirectories(cloningPath))
            {
                ConvertProjectsToX86AfterCloning(item);
            }
        }

        private void StartDownloadingClient()
        {
            if (_abortInitiated)
            {
                OnAbortSuccessfully();
                return;
            }
            var localFilePath = Path.Combine(installDirectoryText.Text, ClientArchive);
            if (File.Exists(localFilePath))
            {
                downloadingProgressLabel.Text = "✔️";
                StartUnzippingClient();
                return;
            }

            Process.Start(installDirectoryText.Text);
            Process.Start("https://drive.google.com/open?id=1JVUGe75nMluczrY14xb0KDXiihFRlGnV");
            downloadingProgressLabel.Text = "Please download {clientArchive}" + Environment.NewLine + $"and move it to: {installDirectoryText.Text}";

            Clipboard.SetText(localFilePath);

            while (!File.Exists(localFilePath))
            {
                if (_abortInitiated)
                {
                    return;
                }

                Application.DoEvents();

                Thread.Sleep(1);
            }

            downloadingProgressLabel.Text = "✔️";
            StartUnzippingClient();
        }
        private void StartUnzippingClient()
        {
            if (Directory.Exists(Path.Combine(installDirectoryText.Text, "League-of-Legends-4-20")))
            {
                unzippingProgressLabel.Text = "✔️";
                StartSettingUpTestbox();
                return;
            }

            var localFilePath = Path.Combine(installDirectoryText.Text, ClientArchive);
            unzippingProgressLabel.Text = "--";
            var directoryPath = installDirectoryText.Text;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                var entryCount = 0;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                // WaitForFile is used to wait for access to zip file while it's copyed by user from Downloads folder to installDirectory
                Utility.WaitForFile(() => ArchiveFactory.Open(localFilePath, new ReaderOptions() { LookForHeader = true }));
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
                    StartSettingUpTestbox();
                }));
            }).Start();
        }
        private void StartSettingUpTestbox()
        {
            if (_abortInitiated)
            {
                OnAbortSuccessfully();
                return;
            }

            if(Directory.Exists(Path.Combine(installDirectoryText.Text, "LeagueUI")))
            {
                installingTestboxLabel.Text = "✔️";
                StartVisualStudioFirstRun();
                return;
            }

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
                        StartVisualStudioFirstRun();
                    }));
                }).Start();
            });
        }
        private void StartVisualStudioFirstRun()
        {
            var leagueInstallFolder = Path.GetFullPath(Path.Combine(installDirectoryText.Text, "League-of-Legends-4-20"));
            launchingProgressLabel.Text = "Setting up Configs";

            //Set up GameServer configs
            var gameServerFolder = Path.Combine(installDirectoryText.Text, "GameServer");
            var templatePath = Path.Combine(gameServerFolder, "GameServerConsole\\Settings\\GameServerSettings.json.template");
            var newPath = Path.Combine(gameServerFolder, "GameServerConsole\\Settings\\GameServerSettings.json");
            var templateString = File.ReadAllText(templatePath);
            var configTemplatePath = Path.Combine(gameServerFolder, "GameServerConsole\\Settings\\GameInfo.json.template");
            var configNewPath = Path.Combine(gameServerFolder, "GameServerConsole\\Settings\\GameInfo.json");
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
            if (File.Exists(configNewPath))
            {
                File.Delete(configNewPath);
            }
            JObject json = JObject.Parse(templateString);
            //C:\LeagueSandbox\League-of-Legends-4-20\RADS\solutions\lol_game_client_sln\releases\0.0.1.68\deploy
            json["clientLocation"] = Path.Combine(leagueInstallFolder, @"RADS\solutions\lol_game_client_sln\releases\0.0.1.68\deploy\League of Legends.exe");
            json["autoStartClient"] = true;
            File.WriteAllText(newPath, json.ToString());
            File.Copy(configTemplatePath, configNewPath);

            File.WriteAllText(templatePath, json.ToString());

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

                var solutionPath = Path.GetFullPath(Path.Combine(gameServerFolder, "GameServer.sln"));

                var allFiles = Search.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "devenv.com").ToList();

                if (allFiles.Count > 0)
                {
                    //Might want to make this pick the newest devenv.
                    var devenvPath = allFiles[0];

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

                    MessageBox.Show("AutoSetup completed ✔️","League Sandbox Auto Setup",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                } 
                else 
                {
                    launchingProgressLabel.Text = "Could not find Visual Studio";
                }
            });
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var selectPath = new FolderBrowserDialog())
            {
                var result = selectPath.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(selectPath.SelectedPath))
                {
                    installDirectoryText.Text = selectPath.SelectedPath;
                }
            }
        }
    }
}
