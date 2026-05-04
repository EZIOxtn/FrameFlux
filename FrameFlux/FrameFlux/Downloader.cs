using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrameFlux
{
    public partial class Downloader : Form
    {
        private readonly string DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";

        private readonly string LibFolder;
        private string downloadedFilePath;

        private HttpClient httpClient;
        private CancellationTokenSource cts;

        private void RefreshUI()
        {
            cuiProgressBarHorizontal1.Refresh();
            cuiLabel1.Refresh();
            cuiLabel2.Refresh();
        }
        private void UI(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }
        public Downloader()
        {
            InitializeComponent();

            LibFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib");
            downloadedFilePath = Path.Combine(LibFolder, "ffmpeg-release-full.7z"); // Updated filename

            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "FrameFlux - FFmpeg Downloader";
        }

        private async void Downloader_Load(object sender, EventArgs e)
        {
            cuiLabel1.Content = "Preparing FFmpeg Download...";
            cuiLabel2.Content = "Connecting to server...";
            cuiProgressBarHorizontal1.Value = 0;

            await StartDownloadAndExtractAsync();
        }

        private async Task StartDownloadAndExtractAsync()
        {
            cts = new CancellationTokenSource();

            try
            {
                Directory.CreateDirectory(LibFolder);

                httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

                UI(() =>
                {
                    cuiLabel1.Content = "Downloading FFmpeg...";
                    RefreshUI();
                });

                // ==================== DOWNLOAD ====================
                using (var response = await httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(downloadedFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        DateTime lastUpdate = DateTime.Now;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cts.Token)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, cts.Token);
                            totalBytesRead += bytesRead;

                            if ((DateTime.Now - lastUpdate).TotalMilliseconds > 150)
                            {
                                if (totalBytes.HasValue && totalBytes > 0)
                                {
                                    int progress = (int)((totalBytesRead * 100) / totalBytes.Value);
                                    UI(() =>
                                    {
                                        
                                        cuiProgressBarHorizontal1.Value = Math.Min(progress, 100);

                                        RefreshUI();
                                    });
                                }
                                
                                double downloadedMB = Math.Round(totalBytesRead / 1024.0 / 1024.0, 2);
                                double totalMB = totalBytes.HasValue ? Math.Round(totalBytes.Value / 1024.0 / 1024.0, 2) : 0;

                         
                                UI(() =>
                                {
                                    cuiLabel2.Content = $" {downloadedMB} MB / {totalMB} MB";
                                    
                                });
                                lastUpdate = DateTime.Now;
                                
                            }
                        }
                    }
                }

                // ==================== EXTRACTION ====================
                cuiLabel1.Text = "Download Complete ✓";
                cuiLabel2.Text = "Extracting FFmpeg files...";
                cuiProgressBarHorizontal1.Value = 100;

                await Task.Run(() => Extract7zFile(downloadedFilePath, LibFolder));

                // Final Success
                cuiLabel1.Text = "✅ Operation Completed Successfully!";
                cuiLabel2.Text = $"FFmpeg successfully installed in: {LibFolder}";

                await Task.Delay(1500);
                this.Close();
            }
            catch (OperationCanceledException)
            {
                cuiLabel1.Text = "⛔ Operation Cancelled";
                cuiLabel2.Text = "Download was cancelled.";
            }
            catch (Exception ex)
            {
                cuiLabel1.Text = "❌ Operation Failed";
                cuiLabel2.Text = "An error occurred during the process.";
                MessageBox.Show($"Error: {ex.Message}", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                httpClient?.Dispose();
            }
        }

        private void Extract7zFile(string archivePath, string extractToPath)
        {
            using (var archive = ArchiveFactory.OpenArchive(archivePath))
            {
                int totalEntries = 0;
                foreach (var entry in archive.Entries)
                    if (!entry.IsDirectory) totalEntries++;

                int current = 0;

                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(extractToPath, new ExtractionOptions()
                        {
                            Overwrite = true,
                            ExtractFullPath = true
                        });

                        current++;

                        int progress = totalEntries > 0 ? (current * 100) / totalEntries : 100;

                        try
                        {
                            this.Invoke(new Action(() =>
                            {
                                cuiProgressBarHorizontal1.Value = Math.Min(progress, 100);
                                cuiLabel2.Content = $"Extracting files... {current}/{totalEntries}";
                            }));
                        }
                        catch { }
                    }
                }
            }
        }

        private void CancelDownload()
        {
            cts?.Cancel();
        }
    }
}