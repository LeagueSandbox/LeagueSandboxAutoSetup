using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace League_Sandbox_Auto_Setup
{
    public partial class leagueSandboxAutoSetupForm : Form
    {
        public leagueSandboxAutoSetupForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            installDirectoryText.Enabled = false;
            startButton.Enabled = false;
            startCloningRepositories();
        }

        private void startCloningRepositories()
        {
            cloningProgressLabel.Text = "--";
            var cloningPath = Path.Combine(installDirectoryText.Text, "GameServer");

            CloneOptions options = new CloneOptions();
            options.BranchName = "Indev";
            options.RecurseSubmodules = true;
            options.RepositoryOperationStarting = (x)=> {
                cloningProgressLabel.Text = "0%";
                return true;
            };
            options.OnProgress = (x) => {
                cloningProgressLabel.Text = x;
                return true;
            };
            options.RepositoryOperationCompleted = (x) => {
                cloningProgressLabel.Text = "✔️";
            };
            Repository.Clone("https://github.com/LeagueSandbox/GameServer", cloningPath, options);
        }

        private void startDownloadingClient()
        {
            cloningProgressLabel.Text = "--";
        }
    }
}
