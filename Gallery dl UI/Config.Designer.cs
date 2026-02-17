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
            label1.Click += label1_Click;
            // 
            // nudSimultaneousDownloads
            // 
            nudSimultaneousDownloads.Location = new Point(314, 7);
            nudSimultaneousDownloads.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            nudSimultaneousDownloads.Name = "nudSimultaneousDownloads";
            nudSimultaneousDownloads.Size = new Size(59, 35);
            nudSimultaneousDownloads.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(331, 196);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(85, 40);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
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
            // 
            // label3
            // 
            label3.Location = new Point(14, 85);
            label3.Margin = new Padding(5, 0, 5, 0);
            label3.Name = "label3";
            label3.Size = new Size(292, 39);
            label3.TabIndex = 4;
            label3.Text = "Main Fore Color";
            // 
            // btnChangeBackColor
            // 
            btnChangeBackColor.Location = new Point(314, 48);
            btnChangeBackColor.Name = "btnChangeBackColor";
            btnChangeBackColor.Size = new Size(102, 35);
            btnChangeBackColor.TabIndex = 5;
            btnChangeBackColor.Text = "Change";
            btnChangeBackColor.UseVisualStyleBackColor = true;
            btnChangeBackColor.Click += btnChangeBackColor_Click;
            // 
            // pnlBackColor
            // 
            pnlBackColor.Location = new Point(258, 47);
            pnlBackColor.Name = "pnlBackColor";
            pnlBackColor.Size = new Size(50, 36);
            pnlBackColor.TabIndex = 6;
            // 
            // pnlForeColor
            // 
            pnlForeColor.Location = new Point(258, 89);
            pnlForeColor.Name = "pnlForeColor";
            pnlForeColor.Size = new Size(50, 35);
            pnlForeColor.TabIndex = 8;
            // 
            // btnChangeForeColor
            // 
            btnChangeForeColor.Location = new Point(314, 89);
            btnChangeForeColor.Name = "btnChangeForeColor";
            btnChangeForeColor.Size = new Size(102, 35);
            btnChangeForeColor.TabIndex = 7;
            btnChangeForeColor.Text = "Change";
            btnChangeForeColor.UseVisualStyleBackColor = true;
            btnChangeForeColor.Click += btnChangeForeColor_Click;
            // 
            // btnUpdateGalleryDl
            // 
            btnUpdateGalleryDl.Location = new Point(14, 196);
            btnUpdateGalleryDl.Name = "btnUpdateGalleryDl";
            btnUpdateGalleryDl.Size = new Size(213, 40);
            btnUpdateGalleryDl.TabIndex = 9;
            btnUpdateGalleryDl.Text = "Update Gallery Dl";
            btnUpdateGalleryDl.UseVisualStyleBackColor = true;
            btnUpdateGalleryDl.Click += btnUpdateGalleryDl_Click;
            // 
            // btnFontChange
            // 
            btnFontChange.Location = new Point(314, 130);
            btnFontChange.Name = "btnFontChange";
            btnFontChange.Size = new Size(102, 35);
            btnFontChange.TabIndex = 11;
            btnFontChange.Text = "Change";
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
            // 
            // lblFontPreview
            // 
            lblFontPreview.Location = new Point(242, 133);
            lblFontPreview.Name = "lblFontPreview";
            lblFontPreview.Size = new Size(66, 32);
            lblFontPreview.TabIndex = 12;
            lblFontPreview.Text = "ABC";
            // 
            // Config
            // 
            AutoScaleDimensions = new SizeF(14F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(433, 248);
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
            Text = "Config";
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
    }
}