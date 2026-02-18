namespace Gallery_dl_UI
{
    partial class LogForm
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
            dgvLog = new DataGridView();
            Date = new DataGridViewTextBoxColumn();
            Site = new DataGridViewTextBoxColumn();
            Location = new DataGridViewTextBoxColumn();
            Url = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
            SuspendLayout();
            // 
            // dgvLog
            // 
            dgvLog.AllowUserToAddRows = false;
            dgvLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLog.Columns.AddRange(new DataGridViewColumn[] { Date, Site, Location, Url });
            dgvLog.Location = new Point(12, 12);
            dgvLog.Name = "dgvLog";
            dgvLog.RowHeadersVisible = false;
            dgvLog.RowHeadersWidth = 51;
            dgvLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLog.Size = new Size(723, 280);
            dgvLog.TabIndex = 4;
            dgvLog.CellMouseClick += dgvLog_CellMouseClick;
            // 
            // Date
            // 
            Date.HeaderText = "Date";
            Date.MinimumWidth = 6;
            Date.Name = "Date";
            Date.Width = 175;
            // 
            // Site
            // 
            Site.HeaderText = "Site";
            Site.MinimumWidth = 6;
            Site.Name = "Site";
            Site.Width = 120;
            // 
            // Location
            // 
            Location.HeaderText = "Location";
            Location.MinimumWidth = 6;
            Location.Name = "Location";
            Location.Width = 175;
            // 
            // Url
            // 
            Url.HeaderText = "Url";
            Url.MinimumWidth = 6;
            Url.Name = "Url";
            Url.Width = 250;
            // 
            // LogForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvLog);
            Name = "LogForm";
            Text = "LogForm";
            ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvLog;
        private DataGridViewTextBoxColumn Date;
        private DataGridViewTextBoxColumn Site;
        private DataGridViewTextBoxColumn Location;
        private DataGridViewTextBoxColumn Url;
    }
}