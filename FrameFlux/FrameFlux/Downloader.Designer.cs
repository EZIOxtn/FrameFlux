namespace FrameFlux
{
    partial class Downloader
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cuiProgressBarHorizontal1 = new CuoreUI.Controls.cuiProgressBarHorizontal();
            cuiLabel1 = new CuoreUI.Controls.cuiLabel();
            cuiLabel2 = new CuoreUI.Controls.cuiLabel();
            SuspendLayout();
            // 
            // cuiProgressBarHorizontal1
            // 
            cuiProgressBarHorizontal1.Background = Color.FromArgb(64, 128, 128, 128);
            cuiProgressBarHorizontal1.Flipped = false;
            cuiProgressBarHorizontal1.Foreground = Color.FromArgb(0, 192, 0);
            cuiProgressBarHorizontal1.Location = new Point(139, 126);
            cuiProgressBarHorizontal1.MaxValue = 100;
            cuiProgressBarHorizontal1.Name = "cuiProgressBarHorizontal1";
            cuiProgressBarHorizontal1.Rounding = 8;
            cuiProgressBarHorizontal1.Size = new Size(637, 32);
            cuiProgressBarHorizontal1.TabIndex = 0;
            cuiProgressBarHorizontal1.Value = 50;
            // 
            // cuiLabel1
            // 
            cuiLabel1.Content = "Please\\ wait\\ ";
            cuiLabel1.Dock = DockStyle.Top;
            cuiLabel1.Font = new Font("Segoe UI", 22.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            cuiLabel1.ForeColor = Color.FromArgb(224, 224, 224);
            cuiLabel1.HorizontalAlignment = StringAlignment.Center;
            cuiLabel1.Location = new Point(0, 0);
            cuiLabel1.Margin = new Padding(4, 5, 4, 5);
            cuiLabel1.Name = "cuiLabel1";
            cuiLabel1.Size = new Size(857, 108);
            cuiLabel1.TabIndex = 1;
            cuiLabel1.VerticalAlignment = StringAlignment.Center;
           
            // 
            // cuiLabel2
            // 
            cuiLabel2.Content = "100%";
            cuiLabel2.Dock = DockStyle.Bottom;
            cuiLabel2.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            cuiLabel2.ForeColor = Color.FromArgb(224, 224, 224);
            cuiLabel2.HorizontalAlignment = StringAlignment.Center;
            cuiLabel2.Location = new Point(0, 229);
            cuiLabel2.Margin = new Padding(4, 5, 4, 5);
            cuiLabel2.Name = "cuiLabel2";
            cuiLabel2.Size = new Size(857, 108);
            cuiLabel2.TabIndex = 2;
            cuiLabel2.VerticalAlignment = StringAlignment.Center;
            // 
            // Downloader
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(857, 337);
            Controls.Add(cuiLabel2);
            Controls.Add(cuiLabel1);
            Controls.Add(cuiProgressBarHorizontal1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Downloader";
            Text = "Downloader";
            Load += Downloader_Load;
            ResumeLayout(false);
        }

        #endregion

        private CuoreUI.Controls.cuiProgressBarHorizontal cuiProgressBarHorizontal1;
        private CuoreUI.Controls.cuiLabel cuiLabel1;
        private CuoreUI.Controls.cuiLabel cuiLabel2;
    }
}