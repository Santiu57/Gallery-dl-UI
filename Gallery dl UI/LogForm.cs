using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace Gallery_dl_UI
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();
            this.Icon = MainForm.ConvertImageToIcon("images/icon.png");
            WindowConfig();
            MainForm.FontChange(this);
            MainForm.ColorComponents(this);
            LoadLog(dgvLog);
            BtnsPaint();
        }

        private void WindowConfig()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ControlBox = true;
            this.ShowIcon = true;
        }
        private void BtnsPaint()
        {
            btnDeleteLog.BackgroundImage = Image.FromFile("images/clear.png");
            btnDeleteLog.BackgroundImageLayout = ImageLayout.Stretch;
        }
        public static void OpenLocation(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    MessageBox.Show("La ruta está vacía.");
                    return;
                }
                path = Path.GetFullPath(path);

                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{path}\"",
                        UseShellExecute = true
                    });
                }
                else if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{path}\"",
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("La ruta no existe.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir la ubicación.\n" + ex.Message);
            }
        }
        private void LoadLog(DataGridView dgv)
        {
            try
            {
                string filePath = Path.Combine(
                    Environment.CurrentDirectory,
                    "log.json");

                if (!File.Exists(filePath))
                    return;

                string json = File.ReadAllText(filePath);

                var logs = JsonSerializer.Deserialize<List<MainForm.Log>>(json);

                dgv.AutoGenerateColumns = true;
                dgv.DataSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading log:\n{ex.Message}",
                    "Log Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void dgvLog_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            switch (e.ColumnIndex)
            {
                case 2: OpenLocation(dgvLog.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()); break;
                case 3: MainForm.OpenUrl(dgvLog.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()); break;
                default: return;
            }
        }

        private void btnDeleteLog_Click(object sender, EventArgs e)
        {
            File.Delete("log.json");
        }

        private void LogForm_Load(object sender, EventArgs e)
        {

        }
    }
}
