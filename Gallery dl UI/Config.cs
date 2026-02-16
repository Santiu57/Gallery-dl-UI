using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Gallery_dl_UI
{
    public partial class Config : Form
    {
        public Config()
        {
            InitializeComponent();
            this.Icon = ConvertImageToIcon("images/icon.png");
            WindowConfig();
            TraverseAllControls(this, control =>
            {
                if (control is not Panel)
                {
                    control.BackColor = Properties.Settings.Default.MainBackColor;
                    control.ForeColor = Properties.Settings.Default.MainForeColor;
                }
            });
        }
        private void TraverseAllControls(Control parent, Action<Control> action)
        {
            action(parent);

            foreach (Control control in parent.Controls)
            {
                TraverseAllControls(control, action);
            }
        }
        private Icon ConvertImageToIcon(string imagePath, int size = 256)
        {
            using (Bitmap original = new Bitmap(imagePath))
            using (Bitmap resized = new Bitmap(original, new Size(size, size)))
            {
                IntPtr hIcon = resized.GetHicon();

                try
                {
                    using (Icon temp = Icon.FromHandle(hIcon))
                    {
                        return (Icon)temp.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);
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
    }
}
