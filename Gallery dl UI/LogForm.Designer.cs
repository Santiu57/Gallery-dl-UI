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
            btnDeleteLog = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
            SuspendLayout();
            // 
            // dgvLog
            // 
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AllowUserToOrderColumns = true;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLog.Location = new Point(0, 1);
            dgvLog.Name = "dgvLog";
            dgvLog.ReadOnly = true;
            dgvLog.RowHeadersVisible = false;
            dgvLog.RowHeadersWidth = 51;
            dgvLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLog.Size = new Size(888, 375);
            dgvLog.TabIndex = 4;
            dgvLog.CellMouseClick += dgvLog_CellMouseClick;
            // 
            // btnDeleteLog
            // 
            btnDeleteLog.Location = new Point(858, 347);
            btnDeleteLog.Name = "btnDeleteLog";
            btnDeleteLog.Size = new Size(30, 29);
            btnDeleteLog.TabIndex = 5;
            btnDeleteLog.UseVisualStyleBackColor = true;
            btnDeleteLog.Click += btnDeleteLog_Click;
            // 
            // LogForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(888, 376);
            Controls.Add(btnDeleteLog);
            Controls.Add(dgvLog);
            Name = "LogForm";
            Text = "LogForm";
            Load += LogForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvLog;
        private Button btnDeleteLog;
    }
}