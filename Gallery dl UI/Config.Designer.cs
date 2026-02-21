namespace Gallery_dl_UI
{
    partial class Config
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
            components = new System.ComponentModel.Container();
            label1 = new Label();
            nudSimultaneousDownloads = new NumericUpDown();
            btnSave = new Button();
            label2 = new Label();
            label3 = new Label();
            cdColor = new ColorDialog();
            btnChangeBackColor = new Button();
            pnlBackColor = new Panel();
            pnlForeColor = new Panel();
            btnChangeForeColor = new Button();
            btnUpdateGalleryDl = new Button();
            fdLetteres = new FontDialog();
            btnFontChange = new Button();
            label4 = new Label();
            lblFontPreview = new Label();
            btninstallGDl = new Button();
            btnCreateConfig = new Button();
            label5 = new Label();
            btnNotifications = new Button();
            tp = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)nudSimultaneousDownloads).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new Point(14, 9);
            label1.Margin = new Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new Size(292, 27);
            label1.TabIndex = 0;
            label1.Text = "Simultaneous Downloads";
            tp.SetToolTip(label1, "Simultaneous gallery dl instances that will be open ");
            label1.Click += label1_Click;
            // 
            // nudSimultaneousDownloads
            // 
            nudSimultaneousDownloads.Location = new Point(326, 7);
            nudSimultaneousDownloads.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
            nudSimultaneousDownloads.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSimultaneousDownloads.Name = "nudSimultaneousDownloads";
            nudSimultaneousDownloads.Size = new Size(59, 35);
            nudSimultaneousDownloads.TabIndex = 1;
            tp.SetToolTip(nudSimultaneousDownloads, "Simultaneous gallery dl instances that will be open ");
            nudSimultaneousDownloads.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnSave
            // 
            btnSave.Location = new Point(343, 221);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(85, 39);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            tp.SetToolTip(btnSave, "Saves this config");
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // label2
            // 
            label2.Location = new Point(14, 47);
            label2.Margin = new Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new Size(292, 36);
            label2.TabIndex = 3;
            label2.Text = "Main Back Color";
            tp.SetToolTip(label2, "Main Back color of the App");
            // 
            // label3
            // 
            label3.Location = new Point(14, 85);
            label3.Margin = new Padding(5, 0, 5, 0);
            label3.Name = "label3";
            label3.Size = new Size(292, 39);
            label3.TabIndex = 4;
            label3.Text = "Main Fore Color";
            tp.SetToolTip(label3, "Main Fore color of the App");
            // 
            // btnChangeBackColor
            // 
            btnChangeBackColor.Location = new Point(326, 48);
            btnChangeBackColor.Name = "btnChangeBackColor";
            btnChangeBackColor.Size = new Size(102, 35);
            btnChangeBackColor.TabIndex = 5;
            btnChangeBackColor.Text = "Change";
            tp.SetToolTip(btnChangeBackColor, "Main Back color of the App");
            btnChangeBackColor.UseVisualStyleBackColor = true;
            btnChangeBackColor.Click += btnChangeBackColor_Click;
            // 
            // pnlBackColor
            // 
            pnlBackColor.Location = new Point(270, 47);
            pnlBackColor.Name = "pnlBackColor";
            pnlBackColor.Size = new Size(50, 36);
            pnlBackColor.TabIndex = 6;
            tp.SetToolTip(pnlBackColor, "Main Back color of the App");
            // 
            // pnlForeColor
            // 
            pnlForeColor.Location = new Point(270, 89);
            pnlForeColor.Name = "pnlForeColor";
            pnlForeColor.Size = new Size(50, 35);
            pnlForeColor.TabIndex = 8;
            tp.SetToolTip(pnlForeColor, "Main Fore color of the App");
            // 
            // btnChangeForeColor
            // 
            btnChangeForeColor.Location = new Point(326, 89);
            btnChangeForeColor.Name = "btnChangeForeColor";
            btnChangeForeColor.Size = new Size(102, 35);
            btnChangeForeColor.TabIndex = 7;
            btnChangeForeColor.Text = "Change";
            tp.SetToolTip(btnChangeForeColor, "Main Fore color of the App");
            btnChangeForeColor.UseVisualStyleBackColor = true;
            btnChangeForeColor.Click += btnChangeForeColor_Click;
            // 
            // btnUpdateGalleryDl
            // 
            btnUpdateGalleryDl.Location = new Point(180, 221);
            btnUpdateGalleryDl.Name = "btnUpdateGalleryDl";
            btnUpdateGalleryDl.Size = new Size(157, 39);
            btnUpdateGalleryDl.TabIndex = 9;
            btnUpdateGalleryDl.Text = "Update G-Dl";
            tp.SetToolTip(btnUpdateGalleryDl, "Update Gallery-Dl");
            btnUpdateGalleryDl.UseVisualStyleBackColor = true;
            btnUpdateGalleryDl.Click += btnUpdateGalleryDl_Click;
            // 
            // btnFontChange
            // 
            btnFontChange.Location = new Point(326, 130);
            btnFontChange.Name = "btnFontChange";
            btnFontChange.Size = new Size(102, 35);
            btnFontChange.TabIndex = 11;
            btnFontChange.Text = "Change";
            tp.SetToolTip(btnFontChange, "Main Font and size of the App");
            btnFontChange.UseVisualStyleBackColor = true;
            btnFontChange.Click += btnFontChange_Click;
            // 
            // label4
            // 
            label4.Location = new Point(14, 126);
            label4.Margin = new Padding(5, 0, 5, 0);
            label4.Name = "label4";
            label4.Size = new Size(292, 39);
            label4.TabIndex = 10;
            label4.Text = "Main Font";
            tp.SetToolTip(label4, "Main Font and size of the App");
            // 
            // lblFontPreview
            // 
            lblFontPreview.Location = new Point(221, 133);
            lblFontPreview.Name = "lblFontPreview";
            lblFontPreview.Size = new Size(99, 32);
            lblFontPreview.TabIndex = 12;
            lblFontPreview.Text = "ABC";
            lblFontPreview.TextAlign = ContentAlignment.MiddleCenter;
            tp.SetToolTip(lblFontPreview, "Main Font and size of the App");
            // 
            // btninstallGDl
            // 
            btninstallGDl.Location = new Point(14, 221);
            btninstallGDl.Name = "btninstallGDl";
            btninstallGDl.Size = new Size(160, 39);
            btninstallGDl.TabIndex = 13;
            btninstallGDl.Text = "Install G-Dl";
            tp.SetToolTip(btninstallGDl, "Install Gallery-dl");
            btninstallGDl.UseVisualStyleBackColor = true;
            btninstallGDl.Click += btninstallGDl_Click;
            // 
            // btnCreateConfig
            // 
            btnCreateConfig.Location = new Point(251, 175);
            btnCreateConfig.Name = "btnCreateConfig";
            btnCreateConfig.Size = new Size(170, 39);
            btnCreateConfig.TabIndex = 14;
            btnCreateConfig.Text = "Create Config";
            tp.SetToolTip(btnCreateConfig, "Creates or opens the basic config file for Gallery-dl");
            btnCreateConfig.UseVisualStyleBackColor = true;
            btnCreateConfig.Click += btnCreateConfig_Click;
            // 
            // label5
            // 
            label5.Location = new Point(14, 165);
            label5.Margin = new Padding(5, 0, 5, 0);
            label5.Name = "label5";
            label5.Size = new Size(160, 27);
            label5.TabIndex = 15;
            label5.Text = "Notifications";
            tp.SetToolTip(label5, "How many notifications show when copying");
            // 
            // btnNotifications
            // 
            btnNotifications.Location = new Point(168, 161);
            btnNotifications.Name = "btnNotifications";
            btnNotifications.Size = new Size(31, 37);
            btnNotifications.TabIndex = 16;
            btnNotifications.Text = "+";
            btnNotifications.TextAlign = ContentAlignment.TopCenter;
            tp.SetToolTip(btnNotifications, "How many notifications show when copying");
            btnNotifications.UseVisualStyleBackColor = true;
            btnNotifications.Click += btnNotifications_Click;
            // 
            // Config
            // 
            AutoScaleDimensions = new SizeF(14F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(433, 272);
            Controls.Add(btnNotifications);
            Controls.Add(label5);
            Controls.Add(btnCreateConfig);
            Controls.Add(btninstallGDl);
            Controls.Add(lblFontPreview);
            Controls.Add(btnFontChange);
            Controls.Add(label4);
            Controls.Add(btnUpdateGalleryDl);
            Controls.Add(pnlForeColor);
            Controls.Add(btnChangeForeColor);
            Controls.Add(pnlBackColor);
            Controls.Add(btnChangeBackColor);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btnSave);
            Controls.Add(nudSimultaneousDownloads);
            Controls.Add(label1);
            Font = new Font("Daily Vibes", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ForeColor = SystemColors.ControlText;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(5, 4, 5, 4);
            Name = "Config";
            Text = "AppConfig";
            FormClosing += Config_FormClosing;
            Load += Config_Load;
            ((System.ComponentModel.ISupportInitialize)nudSimultaneousDownloads).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private NumericUpDown nudSimultaneousDownloads;
        private Button btnSave;
        private Label label2;
        private Label label3;
        private ColorDialog cdColor;
        private Button btnChangeBackColor;
        private Panel pnlBackColor;
        private Panel pnlForeColor;
        private Button btnChangeForeColor;
        private Button btnUpdateGalleryDl;
        private FontDialog fdLetteres;
        private Button btnFontChange;
        private Label label4;
        private Label lblFontPreview;
        private Button btninstallGDl;
        private Button btnCreateConfig;
        private Label label5;
        private Button btnNotifications;
        private ToolTip tp;
    }
}