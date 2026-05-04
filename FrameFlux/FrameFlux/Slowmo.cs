using CuoreUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FrameFlux
{
    public partial class Slowmo : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private string selectedVideoPath = "";
        private string logFilePath = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "ffmpeg_log.txt"
);
        private readonly object logLock = new object();
        private void WriteLog(string text)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {text}";

            lock (logLock)
            {
                File.AppendAllText(logFilePath, line + Environment.NewLine);
            }

            LogUI(line); // 👈 THIS WAS MISSING
        }
        public class VideoSettings
        {
            public string Filename { get; set; } = string.Empty;
            public string Output { get; set; } = string.Empty;

            public double Fps { get; set; } = 60.0;
            public double Sfactor { get; set; } = 10.0; // 2x, 4x slow factor
            public double Duration { get; set; } = 0.0; // in seconds
            public bool Interpolation { get; set; } = true;
        }
        public VideoSettings stg = new VideoSettings();
        private void DragForm(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        // ====================== EVENT ======================
        private void cuiPanel2_MouseDown(object sender, MouseEventArgs e)
        {
            DragForm(e);
        }

        // Optional: Change cursor to hand when hovering over the panel
        private void cuiPanel2_MouseEnter(object sender, EventArgs e)
        {
            cuiPanel2.Cursor = Cursors.Hand;
        }

        private void cuiPanel2_MouseLeave(object sender, EventArgs e)
        {
            cuiPanel2.Cursor = Cursors.Default;
        }
        public Slowmo()
        {
            InitializeComponent();
        }
        private string BuildFilter()
        {
            var filters = new List<string>();

            bool interp = cuiCheckbox1.Checked;   // Interpolation
            bool blend = cuiCheckbox2.Checked;   // Frame blending

            // Interpolation (optical flow)
            if (interp)
            {
                filters.Add($"minterpolate=fps={stg.Fps}:mi_mode=mci:mc_mode=aobmc:me_mode=bidir");
            }

            // Frame blending
            if (blend)
            {
                filters.Add("tblend=all_mode=average");
                // OR stronger:
                // filters.Add("tmix=frames=3:weights='1 2 1'");
            }

            // Always apply slow motion at the end
            filters.Add($"setpts={stg.Sfactor}*PTS");

            return string.Join(",", filters);
        }
        private double GetVideoDuration(string ffprobePath, string videoPath)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = System.Diagnostics.Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (double.TryParse(output, out double seconds))
                    return seconds;
            }

            return 0;
        }
        private void GenerateThumbnail(string ffmpegPath, string videoPath, string outputImage)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{videoPath}\" -ss 00:00:01.000 -vframes 1 \"{outputImage}\" -y",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            System.Diagnostics.Process.Start(psi).WaitForExit();
        }
        private void LoadVideoInfo(string videoPath)
        {
            string ffmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "ffmpeg-8.1.1-full_build", "bin", "ffmpeg.exe");
            string ffprobe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "ffmpeg-8.1.1-full_build", "bin", "ffprobe.exe");

            // Duration
            double seconds = GetVideoDuration(ffprobe, videoPath);
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            cuiLabel6.Content = $"Duration: {time:mm\\:ss}";

            // Thumbnail
            string thumbPath = Path.Combine(Path.GetTempPath(), "thumb.jpg");
            GenerateThumbnail(ffmpeg, videoPath, thumbPath);

            // Load the generated thumbnail image and assign it to the picture control's Image property.
            // The control in your signatures exposes an Image-typed 'Content' property, so assign to that.
            try
            {
                // Load image from file (Image.FromFile will return a System.Drawing.Image)
                Image img = Image.FromFile(thumbPath);

                // Dispose previous image if present to avoid leaks
                if (cuiPictureBox1.Content != null)
                {
                    var old = cuiPictureBox1.Content;
                    cuiPictureBox1.Content = img;
                    old.Dispose();
                }
                else
                {
                    cuiPictureBox1.Content = img;
                }
            }
            catch (Exception ex)
            {
                cuiPictureBox1.Content = null;
                MessageBox.Show($"Failed to load thumbnail: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void cuiLabel2_Load(object sender, EventArgs e)
        {

        }

        private void cuiButton1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Video File";
                ofd.Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov;*.webm";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedVideoPath = ofd.FileName;

                    FileInfo fi = new FileInfo(selectedVideoPath);

                    string fileName = fi.Name;
                    double sizeMB = Math.Round(fi.Length / 1024.0 / 1024.0, 2);

                    cuiLabel3.Content = $"File: {fileName} \n Size: {sizeMB} MB";
                    stg.Filename = fi.FullName;


                }
            }
            try
            {
                LoadVideoInfo(selectedVideoPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading video info: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void cuiPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
        private void cuiPanel2_MouseDownx(object sender, MouseEventArgs e)
        {
            DragForm(e);
        }

        private void Slowmo_Load(object sender, EventArgs e)
        {

            cuiTextBox1.Content = stg.Output;
            cuiLabel3.Content = "No video selected";

        }
        private string GenerateOutputFilename()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Example: 20250504_143022
            string randomPart = GenerateRandomString(3);                // 3 random uppercase letters

            return $"{datePart}_{randomPart}.mp4";
        }

        // Helper method to generate random string
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }
        private async void RunSlowMotion()
        {
            cuiLabel6.Content = "✅ Processing";
            string ffmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "lib", "ffmpeg-8.1.1-full_build", "bin", "ffmpeg.exe");
            string filter = BuildFilter();
            string args =
    $"-i \"{stg.Filename}\" " +
    $"-vf \"{filter}\" " +
    "-c:v libx264 -crf 18 -preset slow -c:a aac " +
    $"\"{stg.Output}\"";
            WriteLog("FILTER: " + filter);
            WriteLog("========== FFmpeg START ==========");
            WriteLog("COMMAND: " + ffmpeg + " " + args);

            var psi = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;

                WriteLog("ERR: " + e.Data);

                if (e.Data.Contains("time="))
                {
                    string timePart = ExtractTime(e.Data);

                }
            };

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    WriteLog("OUT: " + e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            await Task.Run(() => process.WaitForExit());

            WriteLog($"EXIT CODE: {process.ExitCode}");
            WriteLog("========== FFmpeg END ==========");

            // ✅ HANDLE RESULT
            Invoke(new Action(() =>
            {
                if (process.ExitCode == 0)
                {
                    cuiLabel6.Content = $"✅ Done \n {stg.Output}";

                    try
                    {
                        if (cuiCheckbox3.Checked)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = stg.Output,
                                UseShellExecute = true
                            });

                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open output file: {ex.Message}");
                    }
                }
                else
                {
                    cuiLabel6.Content = "❌ Failed";
                    MessageBox.Show("FFmpeg failed. Check logs.");
                }
            }));
        }

        private string ExtractTime(string line)
        {
            try
            {
                int index = line.IndexOf("time=");
                if (index == -1) return null;

                string time = line.Substring(index + 5, 8); // 00:00:00
                return time;
            }
            catch
            {
                return null;
            }
        }
        // C#

        private void LogUI(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action(() =>
                {
                    textBox1.AppendText(text + Environment.NewLine);
                }));
            }
            else
            {
                textBox1.AppendText(text + Environment.NewLine);
            }
        }
        private void cuiButton2_Click(object sender, EventArgs e)
        {
            if (!cuiCheckbox1.Checked && !cuiCheckbox2.Checked)
            {
                MessageBox.Show(
                    "You must enable at least one option:\n- Interpolation\n- Frame Blending",
                    "Invalid Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            if (cuiTextBox1.Content == string.Empty)
            {
                stg.Output = GenerateOutputFilename();
                RunSlowMotion();
            }
            else
            {
                stg.Output = cuiTextBox1.Content;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            stg.Fps = (double)numericUpDown1.Value;

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            stg.Sfactor = (double)numericUpDown2.Value;
        }

        private void cuiCheckbox1_CheckedChanged(object sender, EventArgs e)
        {
            stg.Interpolation = cuiCheckbox1.Checked;
        }

        private void cuiButton3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void cuiButton4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void cuiLabel8_Load(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/EZIOxtn",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open link: {ex.Message}");
            }
        }

        private void cuiLabel8_Load_1(object sender, EventArgs e)
        {
            cuiLabel8.Cursor = Cursors.Hand;
        }
        private void cuiLabel8_enter(object sender, EventArgs e)
        {
            cuiLabel8.ForeColor = Color.Blue; 
        }
        private void cuiLabel8_leave(object sender, EventArgs e)
        {
            cuiLabel8.ForeColor = Color.FromArgb(224, 224, 224);
        }
    }
}

