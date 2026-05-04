using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

namespace FrameFlux
{
    public partial class MainWin : Form
    {
        private static string ffmpegRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib","ffmpeg-8.1.1-full_build");
        public class ValidationResult
        {
            public bool IsValid { get; set; } = false;
            public string RootPath { get; set; } = string.Empty;

            public List<string> MissingFolders { get; } = new List<string>();
            public List<string> MissingFiles { get; } = new List<string>();
            public List<string> MissingOptionalFiles { get; } = new List<string>();
            public List<string> Errors { get; } = new List<string>();
        }
        public static ValidationResult Validate(string rootPath)
        {
            var result = new ValidationResult
            {
                RootPath = rootPath
            };

            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                result.IsValid = false;
                result.Errors.Add("Root path does not exist or is invalid.");
                return result;
            }

            // Required folders
            string binFolder = Path.Combine(rootPath, "bin");
            string docFolder = Path.Combine(rootPath, "doc");

            if (!Directory.Exists(binFolder))
                result.MissingFolders.Add("bin");

            if (!Directory.Exists(docFolder))
                result.MissingFolders.Add("doc");

            // Required executables
            CheckFile(result, Path.Combine(binFolder, "ffmpeg.exe"), "ffmpeg.exe");
            CheckFile(result, Path.Combine(binFolder, "ffprobe.exe"), "ffprobe.exe");
            CheckFile(result, Path.Combine(binFolder, "ffplay.exe"), "ffplay.exe"); // optional but recommended

            // Important documentation files (optional)
            CheckFile(result, Path.Combine(rootPath, "LICENSE"), "LICENSE", optional: true);
            CheckFile(result, Path.Combine(rootPath, "README.txt"), "README.txt", optional: true);

            // Final validation
            result.IsValid = result.MissingFolders.Count == 0 && result.MissingFiles.Count == 0;

            return result;
        }
        private static void CheckFile(ValidationResult result, string fullPath, string fileName, bool optional = false)
        {
            if (!File.Exists(fullPath))
            {
                if (optional)
                    result.MissingOptionalFiles.Add(fileName);
                else
                    result.MissingFiles.Add(fileName);
            }
        }
        public MainWin()
        {
            InitializeComponent();
        }
        private void LaunchMainApplication()
        {
            this.Visible = false;
            this.ShowInTaskbar = false;

            Slowmo sl = new Slowmo();
            sl.Show();

           
            sl.Shown += (s, e) =>
            {
                this.Hide();        
            };
        }

        private async void MainWin_Load(object sender, EventArgs e)
        {
            // Initial validation
            var result = Validate(ffmpegRoot);

            if (result.IsValid)
            {
                // FFmpeg is ready → Launch main app
                LaunchMainApplication();
                return;
            }

            // FFmpeg missing → Hide MainWin completely and show Downloader
            this.Visible = false;
            this.ShowInTaskbar = false;
            this.Opacity = 0;

            using (Downloader downloader = new Downloader())
            {
                downloader.FormClosed += (s, args) =>
                {
                    var recheck = Validate(ffmpegRoot);

                    if (recheck.IsValid)
                    {
                        // Success - Launch main application
                        LaunchMainApplication();
                    }
                    else
                    {
                        MessageBox.Show("FFmpeg installation failed or is incomplete.\nPlease try again.",
                                      "Installation Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Application.Exit();
                    }
                };

                // Show downloader as modal dialog
                downloader.ShowDialog();
            }
        }
    }
}
