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
            MainForm.FontChange(this);
            MainForm.ColorComponents(this);
            WindowConfig();
            LoadHistory(dgvLog,"history.log");
        }

        private void WindowConfig()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ControlBox = true;
            this.ShowIcon = true;
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

        private void dgvLog_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.RowIndex == 0 || e.ColumnIndex == 0 || e.ColumnIndex == 1)
                { return; }
            if(e.ColumnIndex == 2)
            {

            }
            switch (e.ColumnIndex)
            {
                case 2: OpenLocation(dgvLog.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()); break;
                case 3: MainForm.OpenUrl(dgvLog.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()); break;
                default: return;
            }
        }
        public class HistoryItem
        {
            public DateTime Date { get; set; }
            public string Site { get; set; }
            public string Location { get; set; }
            public string Url { get; set; }
        }
        public static void AppendHistory(HistoryItem item, string filePath)
        {
            string jsonLine = JsonSerializer.Serialize(item);

            File.AppendAllText(filePath, jsonLine + Environment.NewLine);
        }
        public static void LoadHistory(DataGridView dgv, string filePath)
        {
            if (!File.Exists(filePath))
                return;

            dgv.Rows.Clear();

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var item = JsonSerializer.Deserialize<HistoryItem>(line);

                dgv.Rows.Add(
                    item.Date,
                    item.Site,
                    item.Location,
                    item.Url
                );
            }
        }
    }
}
