namespace Gallery_dl_UI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            tsBar = new ToolStrip();
            tsStatusLabel = new ToolStripLabel();
            tsProgresBar = new ToolStripProgressBar();
            tsbtnConfig = new ToolStripButton();
            tsbtnArgs = new ToolStripButton();
            tsbtnLog = new ToolStripButton();
            btnStartdownload = new Button();
            txbAddUrl = new TextBox();
            Sclip = new WK.Libraries.SharpClipboardNS.SharpClipboard(components);
            dgvUrls = new DataGridView();
            Site = new DataGridViewTextBoxColumn();
            Url = new DataGridViewTextBoxColumn();
            btnClear = new Button();
            tpTexts = new ToolTip(components);
            tsBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUrls).BeginInit();
            SuspendLayout();
            // 
            // tsBar
            // 
            tsBar.ImageScalingSize = new Size(20, 20);
            tsBar.Items.AddRange(new ToolStripItem[] { tsStatusLabel, tsProgresBar, tsbtnConfig, tsbtnArgs, tsbtnLog });
            tsBar.Location = new Point(0, 0);
            tsBar.Name = "tsBar";
            tsBar.Size = new Size(380, 27);
            tsBar.TabIndex = 0;
            tsBar.Text = "toolStrip1";
            // 
            // tsStatusLabel
            // 
            tsStatusLabel.Name = "tsStatusLabel";
            tsStatusLabel.Size = new Size(73, 24);
            tsStatusLabel.Text = "Sleeping..";
            // 
            // tsProgresBar
            // 
            tsProgresBar.Name = "tsProgresBar";
            tsProgresBar.Size = new Size(112, 24);
            // 
            // tsbtnConfig
            // 
            tsbtnConfig.Alignment = ToolStripItemAlignment.Right;
            tsbtnConfig.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbtnConfig.Image = (Image)resources.GetObject("tsbtnConfig.Image");
            tsbtnConfig.ImageTransparentColor = Color.Magenta;
            tsbtnConfig.Name = "tsbtnConfig";
            tsbtnConfig.Size = new Size(29, 24);
            tsbtnConfig.Text = "toolStripButton1";
            tsbtnConfig.ToolTipText = "Configuration";
            tsbtnConfig.Click += tsbtnConfig_Click;
            // 
            // tsbtnArgs
            // 
            tsbtnArgs.Alignment = ToolStripItemAlignment.Right;
            tsbtnArgs.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbtnArgs.Image = (Image)resources.GetObject("tsbtnArgs.Image");
            tsbtnArgs.ImageTransparentColor = Color.Magenta;
            tsbtnArgs.Name = "tsbtnArgs";
            tsbtnArgs.Size = new Size(29, 24);
            tsbtnArgs.Text = "toolStripButton1";
            tsbtnArgs.ToolTipText = "Gallery Dl Args";
            tsbtnArgs.Click += tsbtnArgs_Click;
            // 
            // tsbtnLog
            // 
            tsbtnLog.Alignment = ToolStripItemAlignment.Right;
            tsbtnLog.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbtnLog.Image = (Image)resources.GetObject("tsbtnLog.Image");
            tsbtnLog.ImageTransparentColor = Color.Magenta;
            tsbtnLog.Name = "tsbtnLog";
            tsbtnLog.Size = new Size(29, 24);
            tsbtnLog.Text = "toolStripButton1";
            tsbtnLog.ToolTipText = "Log";
            // 
            // btnStartdownload
            // 
            btnStartdownload.Font = new Font("Daily Vibes", 8.999999F);
            btnStartdownload.Location = new Point(292, 221);
            btnStartdownload.Name = "btnStartdownload";
            btnStartdownload.Size = new Size(82, 27);
            btnStartdownload.TabIndex = 1;
            btnStartdownload.Text = "Start";
            tpTexts.SetToolTip(btnStartdownload, "Start downloads");
            btnStartdownload.UseVisualStyleBackColor = true;
            btnStartdownload.Click += button1_Click;
            // 
            // txbAddUrl
            // 
            txbAddUrl.Location = new Point(0, 222);
            txbAddUrl.Multiline = true;
            txbAddUrl.Name = "txbAddUrl";
            txbAddUrl.PlaceholderText = "Paste Urls to Add them";
            txbAddUrl.Size = new Size(253, 27);
            txbAddUrl.TabIndex = 2;
            txbAddUrl.TextChanged += txbAddUrl_TextChanged;
            // 
            // Sclip
            // 
            Sclip.MonitorClipboard = true;
            Sclip.ObservableFormats.All = true;
            Sclip.ObservableFormats.Files = false;
            Sclip.ObservableFormats.Images = false;
            Sclip.ObservableFormats.Others = false;
            Sclip.ObservableFormats.Texts = true;
            Sclip.ObserveLastEntry = false;
            Sclip.Tag = null;
            Sclip.ClipboardChanged += sharpClipboard1_ClipboardChanged;
            // 
            // dgvUrls
            // 
            dgvUrls.AllowUserToAddRows = false;
            dgvUrls.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUrls.Columns.AddRange(new DataGridViewColumn[] { Site, Url });
            dgvUrls.Location = new Point(0, 32);
            dgvUrls.Name = "dgvUrls";
            dgvUrls.RowHeadersVisible = false;
            dgvUrls.RowHeadersWidth = 51;
            dgvUrls.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUrls.Size = new Size(374, 184);
            dgvUrls.TabIndex = 3;
            // 
            // Site
            // 
            Site.HeaderText = "Site";
            Site.MinimumWidth = 6;
            Site.Name = "Site";
            Site.Width = 120;
            // 
            // Url
            // 
            Url.HeaderText = "Url";
            Url.MinimumWidth = 6;
            Url.Name = "Url";
            Url.Width = 250;
            // 
            // btnClear
            // 
            btnClear.BackgroundImageLayout = ImageLayout.Stretch;
            btnClear.Location = new Point(259, 221);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(27, 27);
            btnClear.TabIndex = 4;
            tpTexts.SetToolTip(btnClear, "Clear List");
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(380, 253);
            Controls.Add(btnClear);
            Controls.Add(dgvUrls);
            Controls.Add(txbAddUrl);
            Controls.Add(btnStartdownload);
            Controls.Add(tsBar);
            Font = new Font("Daily Vibes", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "MainForm";
            Text = "Gallery dl UI";
            Load += Form1_Load;
            tsBar.ResumeLayout(false);
            tsBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUrls).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip tsBar;
        private Button btnStartdownload;
        private TextBox txbAddUrl;
        private WK.Libraries.SharpClipboardNS.SharpClipboard Sclip;
        private ToolStripProgressBar tsProgresBar;
        private ToolStripButton tsbtnConfig;
        private ToolStripLabel tsStatusLabel;
        private DataGridView dgvUrls;
        private Button btnClear;
        private DataGridViewTextBoxColumn Site;
        private DataGridViewTextBoxColumn Url;
        private ToolStripButton tsbtnLog;
        private ToolTip tpTexts;
        private ToolStripButton tsbtnArgs;
    }
}
