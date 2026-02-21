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

            //Makes invisible the buttons for arguments that don't have to much configuration
            AttachArgumentButtonEvents(
            this,
            Image.FromFile("images/plus.png"),
            arg => NoBtnFields.Contains(arg.Command),
            arg => { },
            arg =>
            {
                string safeName = arg.Name.Replace(" ", "");
                string btnBoxName = "btn" + safeName;

                var btn = this.Controls
                    .Find(btnBoxName, true)
                    .FirstOrDefault() as Button;
                btn.Visible = false;
            }
            );

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

            AttachArgumentButtonEvents(
            this,
            Image.FromFile("images/plus.png"),
            arg => arg.Command == "-f",
            arg =>
            {
                NameFileMini(arg);
            },
             arg =>
             {
                 string safeName = arg.Name.Replace(" ", "");
                 string panelName = "pnl" + safeName;
                 string btnName = "btn" + safeName;

                 var panel = this.Controls.Find(panelName, true).FirstOrDefault() as Panel;
                 var originalBtn = this.Controls.Find(btnName, true).FirstOrDefault() as Button;

                 if (panel != null && originalBtn != null)
                 {
                     Button ClearBtn = new Button
                     {
                         Width = 18,
                         Height = 15,
                         Location = new Point(originalBtn.Left - 25, originalBtn.Top),
                         BackgroundImage = Image.FromFile("images/clear.png"),
                         BackgroundImageLayout = ImageLayout.Stretch,
                         Tag = arg
                     };

                     ClearBtn.Click += (s, e) =>
                     {
                         arg.Value = "";
                         UpdateArgumentTextBox(this, arg);
                     };

                     panel.Controls.Add(ClearBtn);
                     ClearBtn.BringToFront();
                 }
             });

            AttachArgumentButtonEvents(
            this,
            Image.FromFile("images/plus.png"),
            arg => arg.Command == "",
            arg =>
            {

            },
             arg =>
             {
                 string safeName = arg.Name.Replace(" ", "");
                 string panelName = "pnl" + safeName;
                 string txbName = "txb" + safeName;

                 var panel = this.Controls.Find(panelName, true).FirstOrDefault() as Panel;
                 var originaltxb = this.Controls.Find(txbName, true).FirstOrDefault() as TextBox;


                 if (panel != null && originaltxb != null)
                 {
                     originaltxb.PlaceholderText = "<-Command> <Value>";
                 }
             });

            AttachArgumentButtonEvents(
            this,
            Image.FromFile("images/plus.png"),
            arg => arg.Command == "--cookies-from-browser",
            arg =>
            {
                BrowserCoookiesMini(arg);
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

            int panelWidth = (container.Width / 2) - 12;  // antes -20
            int panelHeight = 51;                         // antes 85
            int verticalSpacing = 57;                     // antes 95
            int leftMargin = 6;                           // antes 10
            int rightMargin = container.Width / 2 + 6;    // antes +10

            for (int i = 0; i < total; i++)
            {
                var arg = visibleArgs[i];
                string safeName = arg.Name.Replace(" ", "");

                int column = i / rowsPerColumn;
                int row = i % rowsPerColumn;

                int x = column == 0 ? leftMargin : rightMargin;
                int y = (int)(10)+ row * verticalSpacing;

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
                    Location = new Point(6, 6),  // antes (10,10)
                    Name = "chk" + safeName
                };

                Button btn = new Button
                {
                    Width = 18,     // antes 30
                    Height = 15,    // antes 25
                    Location = new Point(panelWidth - 24, 5), // antes -40 , 8
                    Name = "btn" + safeName,
                    Tag = arg
                };

                TextBox txb = new TextBox
                {
                    Width = panelWidth - 12, // antes -20
                    Location = new Point(6, 27), // antes (10,45)
                    Name = "txb" + safeName,
                    Text = arg.Value,
                    PlaceholderText = "<value>"
                };

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
        Action<Argument> customAction = null,
        Action<Argument> extra = null)
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
                        extra?.Invoke(arg);
                    }
                }
            }
        }

        private void ArgsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var filenameArg = MainForm.Args
            .FirstOrDefault(a => a.Command == "-f");

            if (filenameArg != null && !string.IsNullOrWhiteSpace(filenameArg.Value) && filenameArg.Enabled)
            {
                var value = filenameArg.Value.Trim();

                if (!value.Contains("{extension}"))
                {
                    value = value.TrimEnd('.');

                    filenameArg.Value = value + ".{extension}";
                }
            }
            SaveArgumentsToSettings();
            MainForm.SaveArguments();
        }

        private Button Createbtn(string text, Action click)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Click += (s, e) => click();
            return btn;
        }
        private void NameFileMini(Argument arg)
        {
            MiniForm mini = new MiniForm("File naming");

            foreach (var category in GalleryDlFields)
            {
                var fields = category.Value;
                mini.AddControl(new Label() { Text = category.Key });

                for (int i = 0; i < fields.Count; i++)
                {
                    string f = fields[i];
                    bool isLast = i == fields.Count - 1;

                    Button btn = Createbtn(f, () =>
                    {
                        if (f == "extension")
                            arg.Value += $".{{{f}}}";
                        else
                            arg.Value += $"{{{f}}}";

                        UpdateArgumentTextBox(this, arg);
                    });
                    if (isLast)
                    {
                        mini.AddControl(btn, true);
                    }
                    else
                    {
                        mini.AddControl(btn);
                    }
                }
            }
            mini.AddButton("folder", () => { arg.Value += "/"; UpdateArgumentTextBox(this, arg); }, false);

            mini.MaximumSize = new Size(800, 800);

            mini.FontAndColorMini();
            mini.MiniWindowConfig();
            if (Application.OpenForms.OfType<MiniForm>().Any())
                return;
            mini.Show();
        }

        private void BrowserCoookiesMini(Argument arg)
        {
            MiniForm mini = new MiniForm("Supported Browsers");

            foreach (var browser in SupportedBrowsers)
            {
                Button btn = Createbtn(browser, () =>
                {
                    arg.Value = browser;
                    UpdateArgumentTextBox(this, arg);
                });
                mini.AddControl(btn, true);
            }
            mini.MaximumSize = new Size(mini.Width, 200);
            mini.MiniWindowConfig();
            mini.FontAndColorMini();
            mini.Show();
        }

        List<string> NoBtnFields = new()
        {
            "--sleep",
            "-u",
            "-p",
            "-R",
            "--range",
            ""
        };

        List<string> SupportedBrowsers = new List<string>
        {
            "chromium",
            "edge",
            "brave",
            "opera",
            "vivaldi",
            "firefox"
        };

        Dictionary<string, List<string>> GalleryDlFields = new()
        {
            ["Identifiers"] = new()
        {
            "id",
            "post_id",
            "media_id",
            "image_id",
            "illust_id",
            "tweet_id",
            "status_id",
            "submission_id",
            "comment_id",
            "parent_id",
            "conversation_id",
            "gallery_id",
            "album_id",
            "chapter_id",
            "series_id",
            "volume_id",
            "user_id",
            "uploader_id"
        },

            ["Indexing"] = new()
        {
            "num",
            "index",
            "position",
            "page",
            "page_count",
            "chapter",
            "chapter_number",
            "volume",
            "volume_number",
            "part",
            "episode"
        },

            ["File"] = new()
        {
            "filename",
            "basename",
            "extension",
            "file_extension",
            "directory",
            "path",
            "md5",
            "sha1",
            "sha256"
        },

            ["File Properties"] = new()
        {
            "width",
            "height",
            "filesize",
            "filesize_approx",
            "duration",
            "fps",
            "bitrate",
            "format",
            "mimetype"
        },

            ["Date & Time"] = new()
        {
            "date",
            "datetime",
            "timestamp",
            "year",
            "month",
            "day",
            "hour",
            "minute",
            "second",
            "week",
            "weekday"
        },

            ["User / Author"] = new()
        {
            "author",
            "artist",
            "creator",
            "user",
            "username",
            "account",
            "profile",
            "nickname",
            "display_name",
            "uploader",
            "group"
        },

            ["Content"] = new()
        {
            "title",
            "caption",
            "description",
            "commentary",
            "text",
            "body",
            "summary",
            "note",
            "language",
            "type",
            "category",
            "subcategory",
            "rating"
        },

            ["Tags"] = new()
        {
            "tags",
            "tag_string",
            "tagged",
            "character",
            "characters",
            "series",
            "copyright",
            "genre",
            "species",
            "meta"
        },

            ["Metrics"] = new()
        {
            "score",
            "favorites",
            "favorite_count",
            "likes",
            "like_count",
            "views",
            "view_count",
            "retweets",
            "reposts",
            "comments",
            "comment_count",
            "bookmark_count",
            "download_count"
        },

            ["Source"] = new()
        {
            "source",
            "url",
            "original_url",
            "referer",
            "webpage_url",
            "source_url"
        }
        };
    }
}
