using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Xml.Linq;
using static Gallery_dl_UI.MainForm;

namespace Gallery_dl_UI
{
    public partial class ArgsForm : Form
    {
        public ArgsForm()
        {
            InitializeComponent();
        }

        private void ArgsForm_Load(object sender, EventArgs e)
        {
            GenerateArgumentUI(this);

            //Add event handlers for buttons
            AttachArgumentButtonEvents(
            this,
            Image.FromFile("images/folder.png"),
            arg => arg.Command == "-d" || arg.Command == "-D",
            arg =>
            {
                
                using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        arg.Value = dlg.SelectedPath;
                        UpdateArgumentTextBox(this, arg);
                    }
                }
            });


            WindowConfig();
            MainForm.FontChange(this);
            MainForm.ColorComponents(this);
            this.Icon = MainForm.ConvertImageToIcon("images/icon.png");
        }

        private void WindowConfig()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ControlBox = true;
            this.ShowIcon = true;
        }
        public void GenerateArgumentUI(Control container)
        {
            container.Controls.Clear();

            ToolTip tooltip = new ToolTip();

            var visibleArgs = MainForm.Args
                .Where(a => a.Visible)
                .ToList();

            int total = visibleArgs.Count;
            int rowsPerColumn = (int)Math.Ceiling(total / 2.0);

            int panelWidth = (container.Width / 2) - 20;
            int panelHeight = 85;
            int verticalSpacing = 95;
            int leftMargin = 10;
            int rightMargin = container.Width / 2 + 10;

            for (int i = 0; i < total; i++)
            {
                var arg = visibleArgs[i];
                string safeName = arg.Name.Replace(" ", "");

                int column = i / rowsPerColumn;
                int row = i % rowsPerColumn;

                int x = column == 0 ? leftMargin : rightMargin;
                int y = 10 + row * verticalSpacing;

                Panel pnl = new Panel
                {
                    Width = panelWidth,
                    Height = panelHeight,
                    Location = new Point(x, y),
                    BorderStyle = BorderStyle.FixedSingle,
                    Name = "pnl" + safeName
                };

                tooltip.SetToolTip(pnl, arg.Description);

                CheckBox chk = new CheckBox
                {
                    Text = arg.Name,
                    Checked = arg.Enabled,
                    AutoSize = true,
                    Location = new Point(10, 10),
                    Name = "chk" + safeName
                };
                tooltip.SetToolTip(chk, arg.Description);

                Button btn = new Button
                {
                    Text = "...",
                    Width = 30,
                    Height = 25,
                    Location = new Point(panelWidth - 40, 8),
                    Name = "btn" + safeName,
                    Tag = arg
                };
                tooltip.SetToolTip(btn, arg.Description);

                TextBox txb = new TextBox
                {
                    Width = panelWidth - 20,
                    Location = new Point(10, 45),
                    Name = "txb" + safeName,
                    Text = arg.Value,
                    PlaceholderText = $"<value>"
                };
                tooltip.SetToolTip(txb, arg.Description);

                chk.CheckedChanged += (s, e) =>
                {
                    arg.Enabled = chk.Checked;

                    foreach (Control c in pnl.Controls)
                        if (c != chk)
                            c.Enabled = chk.Checked;
                };

                txb.TextChanged += (s, e) =>
                {
                    arg.Value = txb.Text;
                };

                btn.Enabled = txb.Enabled = chk.Checked;

                pnl.Controls.Add(chk);
                pnl.Controls.Add(btn);
                pnl.Controls.Add(txb);
                container.Controls.Add(pnl);
            }
        }
        private void UpdateArgumentTextBox(Control container, Argument arg)
        {
            string safeName = arg.Name.Replace(" ", "");
            string textBoxName = "txb" + safeName;

            var txb = container.Controls
                .Find(textBoxName, true)
                .FirstOrDefault() as TextBox;

            if (txb != null)
            {
                txb.Text = arg.Value;
            }
        }
        public void SaveArgumentsToSettings()
        {
            foreach (var arg in MainForm.Args)
            {
                var prop = Properties.Settings.Default
                    .GetType()
                    .GetProperty(arg.Name.Replace(" ", ""));

                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(Properties.Settings.Default, arg.Value);
                }
            }

            Properties.Settings.Default.Save();
        }

        public void AttachArgumentButtonEvents(
        Control container,
        Image img,
        Func<Argument, bool> filter,
        Action<Argument> customAction)
        {
            foreach (Control pnl in container.Controls)
            {
                foreach (Control ctrl in pnl.Controls)
                {
                    if (ctrl is Button btn && btn.Tag is Argument arg && filter(arg))
                    {
                        btn.BackgroundImage = img;
                        btn.BackgroundImageLayout = ImageLayout.Stretch;
                        btn.Text = "";

                        btn.Click += (s, e) =>
                        {
                            customAction?.Invoke(arg);
                        };
                    }
                }
            }
        }

        private void ArgsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveArgumentsToSettings();
            MainForm.SaveArguments();
        }
    }
}
