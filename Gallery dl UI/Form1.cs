using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using WK.Libraries.SharpClipboardNS;

namespace Gallery_dl_UI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ColorComponents();
            LoadImages();
            WindowConfig();
        }

        private void TraverseAllControls(Control parent, Action<Control> action)
        {
            action(parent);

            foreach (Control control in parent.Controls)
            {
                TraverseAllControls(control, action);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //Buttons
        private async void button1_Click(object sender, EventArgs e)
        {
            TraverseAllControls(this, control =>
            {
                control.Enabled = false;
            });

            BuilArgs();

            Urls = UrlExtractor(dgvUrlsFusion());

            NotificationShow("Operacion Iniciada", Urls.Count.ToString() + " Urls a descargar", 1000);

            await LinksQueue();
        }
        private void tsbtnConfig_Click(object sender, EventArgs e)
        {
            Config config = new Config();
            config.ShowDialog();
            ColorComponents();
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvUrls.Rows.Clear();
        }
        // GalleryDl Manager
        List<string> Urls = new List<string>();
        private SemaphoreSlim _semaphore;

        private async Task RunGalleryDl(string url)
        {
            await _semaphore.WaitAsync();

            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "gallery-dl",
                    Arguments = Arguments + url,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    await process.WaitForExitAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        //Urls manager
        public static List<string> UrlExtractor(string input)
        {
            var results = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return results;

            string pattern = @"https?://[^\s<>""'\)\]\}]+";

            var matches = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            char[] trimChars = new[] { '<', '>', '(', ')', '[', ']', '{', '}', '"', '\'', '.', ',', ';', ':', '!', '?' };

            foreach (Match match in matches)
            {
                if (match == null || string.IsNullOrWhiteSpace(match.Value))
                    continue;

                var candidate = match.Value.Trim(trimChars);

                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                if (Uri.TryCreate(candidate, UriKind.Absolute, out Uri uri))
                {
                    if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                        continue;

                    string normalized = uri.AbsoluteUri;

                    if (seen.Add(normalized))
                    {
                        results.Add(normalized);
                    }
                }
            }

            return results;
        }

        public static bool ValidUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            string pattern = @"^https?:\/\/[^\s\/$.?#].[^\s]*$";

            if (!Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return false;

            return true;
        }

        private async Task LinksQueue()
        {
            _semaphore = new SemaphoreSlim(Properties.Settings.Default.SimultaneousDownloads);

            int completed = 0;
            int total = Urls.Count;

            ChangeStatusLabel($"En progreso... {0}/{total} Completadas");

            var tasks = Urls.Select(async url =>
            {
                await RunGalleryDl(url);

                int done = Interlocked.Increment(ref completed);

                Invoke(() =>
                {
                    ChangeStatusLabel($"En progreso... {done}/{total} Completadas");
                    RowChangeProgresBar(done, total);
                });

            }).ToList();

            await Task.WhenAll(tasks);

            TraverseAllControls(this, control =>
            {
                control.Enabled = true;
            });

            Urls.Clear();
            NotificationShow("Completado", "Operación realizada con exito", 600);
            ChangeStatusLabel("sleeping...");
            ChangeProgresBar(0);
            dgvUrls.Rows.Clear();
        }



        //Args
        static List<Argument> Args = new List<Argument>();
        string Arguments = "";

        void BuilArgs()
        {
            foreach (var arg in Args)
            {
                string cmd = arg.ToCommandString();
                if (!string.IsNullOrEmpty(cmd))
                    Arguments += cmd;
            }
        }

        Argument DestinationPATH = new Argument("Destination Path", Properties.Settings.Default.DestinationPath, "-d", "Target location for file downloads", Args);
        Argument DirectoryPATH = new Argument("Download Path", Properties.Settings.Default.DirectoryPath, "-D", "Exact location for file downloads", Args, false);
        Argument FileName = new Argument("File Name", Properties.Settings.Default.FileName, "-f", "Files naming structure", Args, false);
        Argument NoOverwrites = new Argument("No overwrites", "", "--no-overwrites", "Skip files that already exist", Args);
        Argument NoProgress = new Argument("NoProgress", "", "--no-progress", "Removes console progress", Args);

        class Argument
        {
            public string Name { get; }
            public string Value { get; set; }
            public string Command { get; }
            public string Description { get; }
            public bool Enabled { get; set; }
            List<Argument> Container;

            public Argument(string name, string value, string command, string description, List<Argument> container, bool enabled = true)
            {
                Name = name;
                Value = value;
                Command = command;
                Description = description;
                Enabled = enabled;
                this.Container = container;
                Container.Add(this);
            }

            public string ToCommandString()
            {
                if (!Enabled || string.IsNullOrWhiteSpace(Value))
                    return string.Empty;

                return Command + " " + Value + " ";
            }
        }


        //Config


        //Clipboard detector
        private void sharpClipboard1_ClipboardChanged(object sender, WK.Libraries.SharpClipboardNS.SharpClipboard.ClipboardChangedEventArgs e)
        {
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                string contenido = e.Content.ToString().Trim();

                if (ValidUrl(contenido) && !RepetedUrl(contenido))
                {
                    AddUrlToDGV(contenido);

                    NotificationShow("La URL fue copiada correctamente.", "Has añadido " + UrlCount() + " Urls", 500);
                }
            }
        }
        //Notification
        NotifyIcon trayIcon = new NotifyIcon();

        private void NotificationShow(string Title, string Text, int time)
        {
            trayIcon.Icon = ConvertImageToIcon(Image.FromFile("images/icon.png"));
            trayIcon.Visible = true;
            trayIcon.BalloonTipTitle = Title;
            trayIcon.BalloonTipText = Text;
            trayIcon.BalloonTipIcon = ToolTipIcon.Info;

            trayIcon.ShowBalloonTip(time);
        }

        //UI
        private void ChangeProgresBar(int value)
        {
            tsProgresBar.Value = value;
        }
        private void RowChangeProgresBar(int Num1, int Num2)
        {
            int value = (int)(((double)Num1 / Num2) * 100);
            tsProgresBar.Value = value;
        }
        private void ChangeStatusLabel(string text)
        {
            tsStatusLabel.Text = text;
        }
        private void ColorComponents()
        {
            TraverseAllControls(this, control =>
            {
                if (control is not Panel)
                {
                    control.BackColor = Properties.Settings.Default.MainBackColor;
                    control.ForeColor = Properties.Settings.Default.MainForeColor;
                }
                if (control is DataGridView dgv)
                {
                    dgv.BackgroundColor = Properties.Settings.Default.MainBackColor;
                    dgv.DefaultCellStyle.BackColor = Properties.Settings.Default.MainBackColor;
                    dgv.DefaultCellStyle.ForeColor = Properties.Settings.Default.MainForeColor;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Properties.Settings.Default.MainBackColor;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Properties.Settings.Default.MainForeColor;
                }
            });
        }

        private void LoadImages()
        {
            tsbtnConfig.Image = Image.FromFile("images/config.png");
            tsbtnLog.Image = Image.FromFile("images/log.png");
            btnClear.BackgroundImage = Image.FromFile("images/clear.png");
            btnClear.ImageAlign = ContentAlignment.MiddleCenter;
            btnClear.BackgroundImageLayout = ImageLayout.Stretch;
            this.Icon = ConvertImageToIcon(Image.FromFile("images/icon.png"));
        }

        private void WindowConfig()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ControlBox = true;
            this.ShowIcon = true;
        }

        //Icon

        private Icon ConvertImageToIcon(Image img, int size = 256)
        {
            using (Bitmap original = new Bitmap(img))
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

        //DGV
        public static string ExtractSiteName(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                return string.Empty;

            string host = uri.Host.ToLower();

            // Quitar www
            if (host.StartsWith("www."))
                host = host.Substring(4);

            string[] parts = host.Split('.');

            if (parts.Length < 2)
                return host;

            // Manejo básico de dominios tipo co.uk
            if (parts.Length >= 3 && parts[^2].Length <= 3)
                return parts[^3];

            return parts[^2];
        }
        private void AddUrlToDGV(string url)
        {
            if (RepetedUrl(url))
                return;
            string siteName = ExtractSiteName(url);
            dgvUrls.Rows.Add(siteName, url);
        }
        private string dgvUrlsFusion()
        {
            string Urls = "";
            foreach (DataGridViewRow row in dgvUrls.Rows)
            {
                if (row.Cells[1].Value != null)
                {
                    string url = row.Cells[1].Value.ToString();
                    if (!string.IsNullOrWhiteSpace(url))
                        Urls += url + " ";
                }
            }
            return Urls;
        }

        private bool RepetedUrl(string url)
        {
            foreach (DataGridViewRow row in dgvUrls.Rows)
            {
                if (row.Cells[1].Value != null)
                {
                    string existingUrl = row.Cells[1].Value.ToString();
                    if (string.Equals(existingUrl, url, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }
        private int UrlCount()
        {
            int count = 0;
            foreach (DataGridViewRow row in dgvUrls.Rows)
            {
                if (row.Cells[1].Value != null)
                {
                    string url = row.Cells[1].Value.ToString();
                    if (!string.IsNullOrWhiteSpace(url))
                        count++;
                }
            }
            return count;
        }

        //TXB
        private void txbAddUrl_TextChanged(object sender, EventArgs e)
        {
            if (ValidUrl(txbAddUrl.Text))
            {
                AddUrlToDGV(txbAddUrl.Text);
                txbAddUrl.Clear();
            }
        }
    }
}
