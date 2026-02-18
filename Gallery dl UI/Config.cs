using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using static Gallery_dl_UI.MainForm;

namespace Gallery_dl_UI
{
    public partial class Config : Form
    {
        public Config()
        {
            InitializeComponent();
            this.Icon = MainForm.ConvertImageToIcon("images/icon.png");
            WindowConfig();
            FontChange();
            MainForm.TraverseAllControls(this, control =>
            {
                if (control is not Panel)
                {
                    control.BackColor = Properties.Settings.Default.MainBackColor;
                    control.ForeColor = Properties.Settings.Default.MainForeColor;
                }
            });
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Config_Load(object sender, EventArgs e)
        {
            ConfigLoad();
        }

        private void ConfigLoad()
        {
            nudSimultaneousDownloads.Value = Properties.Settings.Default.SimultaneousDownloads;
            pnlBackColor.BackColor = Properties.Settings.Default.MainBackColor;
            pnlForeColor.BackColor = Properties.Settings.Default.MainForeColor;
            fdLetteres.Font = Properties.Settings.Default.MainFont;
        }
        private void Config_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.SimultaneousDownloads = (int)nudSimultaneousDownloads.Value;
            // Save settings when the form is closing
            Properties.Settings.Default.Save();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static Color EnsureDifferent(Color color1, Color color2, int lightenAmount = 30)
        {
            if (color1 != color2)
                return color2;

            // Aclarar el segundo color
            int r, g, b;

            if (color2.R + lightenAmount > 255 || color2.G + lightenAmount > 255 || color2.B + lightenAmount > 255)
            {
                // Si aclarar el color excede el límite, oscurecerlo en su lugar
                r = Math.Max(0, color2.R - lightenAmount);
                g = Math.Max(0, color2.G - lightenAmount);
                b = Math.Max(0, color2.B - lightenAmount);
            }
            else
            {
                // Aclarar el segundo color
                r = Math.Min(255, color2.R + lightenAmount);
                g = Math.Min(255, color2.G + lightenAmount);
                b = Math.Min(255, color2.B + lightenAmount);
            }

            return Color.FromArgb(color2.A, r, g, b);
        }
        private void WindowConfig()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ControlBox = true;
            this.ShowIcon = true;
        }
        private void btnChangeBackColor_Click(object sender, EventArgs e)
        {
            if (cdColor.ShowDialog().Equals(DialogResult.OK))
            {
                pnlBackColor.BackColor = cdColor.Color;
                pnlForeColor.BackColor = EnsureDifferent(pnlBackColor.BackColor, pnlForeColor.BackColor);
                Properties.Settings.Default.MainBackColor = pnlBackColor.BackColor;
            }
        }

        private void btnChangeForeColor_Click(object sender, EventArgs e)
        {
            if (cdColor.ShowDialog().Equals(DialogResult.OK))
            {
                pnlForeColor.BackColor = cdColor.Color;
                pnlForeColor.BackColor = EnsureDifferent(pnlBackColor.BackColor, pnlForeColor.BackColor);
                Properties.Settings.Default.MainForeColor = pnlForeColor.BackColor;
            }
        }
        private void btnFontChange_Click(object sender, EventArgs e)
        {
            if (fdLetteres.ShowDialog().Equals(DialogResult.OK))
            {
                Properties.Settings.Default.MainFont = fdLetteres.Font;
                lblFontPreview.Font = fdLetteres.Font;
            }
        }
        private float _currentScale = 1f;

        private void FontChange()
        {
            var newFont = Properties.Settings.Default.MainFont;

            float newScale = newFont.Size / this.Font.Size;
            float deltaScale = newScale / _currentScale;

            this.SuspendLayout();

            this.Font = newFont;
            this.Scale(new SizeF(deltaScale, deltaScale));

            _currentScale = newScale;

        }

        private void btnUpdateGalleryDl_Click(object sender, EventArgs e)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell",
                Arguments = "-Command \"py -m pip install --upgrade gallery-dl\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                MessageBox.Show(
                    process.ExitCode == 0 ? "gallery-dl actualizado correctamente." : error,
                    process.ExitCode == 0 ? "Actualización completada" : "Error",
                    MessageBoxButtons.OK,
                    process.ExitCode == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Error
                );
            }
        }

        private void btninstallGDl_Click(object sender, EventArgs e)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell",
                Arguments = "-Command \"py -m pip install gallery-dl\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                MessageBox.Show(
                    process.ExitCode == 0 ? "gallery-dl instalado correctamente." : error,
                    process.ExitCode == 0 ? "Instalación completada" : "Error",
                    MessageBoxButtons.OK,
                    process.ExitCode == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Error
                );
            }

        }
    }
}
