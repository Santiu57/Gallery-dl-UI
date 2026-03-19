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
            MainForm.FontChange(this);
            MainForm.ColorComponents(this);
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
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string configPath = Path.Combine(
                roamingPath,
                "gallery-dl",
                "config.json");

            if (File.Exists(configPath))
            {
                btnCreateConfig.Text = "Open Config";
            }
            else
            {
                // No existe
            }
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

        private void btnCreateConfig_Click(object sender, EventArgs e)
        {
            if (btnCreateConfig.Text == "Open Config")
            {
                string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string configPath = Path.Combine(roamingPath, "gallery-dl", "config.json");
                if (File.Exists(configPath))
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "gallery-dl",
                        Arguments = $"--config-open",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process())
                    {
                        process.StartInfo = startInfo;
                        process.Start();
                    }
                }
            }
            else
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "gallery-dl",
                    Arguments = $"--config-create",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                }
            }

        }

        private void btnNotifications_Click(object sender, EventArgs e)
        {
            var mini = new MiniForm("Notification Config", new Size(350, 150), true);

            var row = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var numeric = new NumericUpDown
            {
                Minimum = -1,
                Maximum = 10000,
                Value = Properties.Settings.Default.NotificationPerLink,
                Width = 60
            };

            row.Controls.Add(new Label { Text = "Notify every", AutoSize = true });
            row.Controls.Add(numeric);
            row.Controls.Add(new Label { Text = "copied URLs", AutoSize = true });

            mini.AddControl(row);

            if (!Properties.Settings.Default.ShowNotifs)
            {
                mini.AddButton("Turn on all notifs", () =>
                {
                    Properties.Settings.Default.ShowNotifs = true;
                    Properties.Settings.Default.Save();
                });
            }
            else
            {
                mini.AddButton("Turn off all notifs", () =>
                {
                    Properties.Settings.Default.ShowNotifs = false;
                    Properties.Settings.Default.Save();
                });
            }

            mini.FormClosing += (s, e) =>
            {
                Properties.Settings.Default.NotificationPerLink = (int)numeric.Value;
                Properties.Settings.Default.Save();
            };

            mini.FontAndColorMini();
            mini.MiniWindowConfig();

            mini.ShowDialog();
        }
    }
}
