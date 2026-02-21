using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WK.Libraries.SharpClipboardNS;
using static Gallery_dl_UI.LogForm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Gallery_dl_UI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ColorComponents(this);
            LoadImages();
            WindowConfig();
            FontChange(this);
            ForceRefresh(this);
            LoadArguments();
            CheckGalleryDlUpdate();
        }

        public static void TraverseAllControls(Control parent, Action<Control> action)
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

            NotificationShow("Operacion Iniciada", Urls.Count.ToString() + " Urls a descargar");

            await LinksQueue();
        }
        private void tsbtnConfig_Click(object sender, EventArgs e)
        {
            Config config = new Config();
            _currentScale = 1f;
            config.ShowDialog();
            ColorComponents(this);
            if (btnStartdownload.Font != Properties.Settings.Default.MainFont)
            {
                Application.Restart();
            }
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

            ChangeStatusLabel($"{0}/{total} Completadas");
            Status = "Downloading";

            var tasks = Urls.Select(async url =>
            {
                await RunGalleryDl(url);

                int done = Interlocked.Increment(ref completed);

                Invoke(() =>
                {
                    ChangeStatusLabel($"{done}/{total} Completadas");
                    SaveLog(url);
                    RowChangeProgresBar(done, total);
                });

            }).ToList();

            await Task.WhenAll(tasks);

            TraverseAllControls(this, control =>
            {
                control.Enabled = true;
            });

            dgvUrls.Rows.Clear();
            if (!Errors())
            {
                Urls.Clear();
                NotificationShow("Completado", "Operación realizada con exito");
            }
            ChangeStatusLabel("sleeping...");
            Status = "Idle";
            ChangeProgresBar(0);
        }

        //Log
        public class Log
        {
            public DateTime Date { get; set; }
            public string Site { get; set; }
            public string Location { get; set; }
            public string Url { get; set; }

        public Log(string url)
            {
                Date = DateTime.Now;
                Site = ExtractSiteName(url);
                Location = getPath();
                Url = url;
            }
             public string getPath()
            {
                string path = "";
                Argument arg = DirectoryArgsFilter();
                if ( arg != null)
                {
                    path = arg.Value;
                }
                else
                {
                    path = Path.Combine(
                        Environment.CurrentDirectory,
                        "gallery-dl");
                }
                return path;
            }
            public Argument DirectoryArgsFilter()
            {
                Argument arg = null;
                var directoryArgs = Args.Where(a => a.Command == "-D" || a.Command == "-d").ToList();
                var activeArgs = Args.Where(a => a.Enabled).ToList();
                if(activeArgs.Count == 1)
                {
                    arg = activeArgs[1];
                }
                else if (activeArgs.Count == 0)
                {
                    arg = activeArgs[0];
                }
                return arg;
            }
            public void AddToSaveFile()
            {
                try
                {
                    string filePath = Path.Combine(
                        Environment.CurrentDirectory,
                        "log.json");

                    List<Log> logs = new List<Log>();

                    if (File.Exists(filePath))
                    {
                        string existingJson = File.ReadAllText(filePath);
                        logs = JsonSerializer.Deserialize<List<Log>>(existingJson)
                               ?? new List<Log>();
                    }

                    logs.Add(this);

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    string json = JsonSerializer.Serialize(logs, options);
                    File.WriteAllText(filePath, json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error saving log:\n{ex.Message}",
                        "Log Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        private void SaveLog(string url)
        {
            Log log = new Log(url);
            log.AddToSaveFile();
        }

        //Args
        public static List<Argument> Args = new();
        string Arguments = "";

        void BuilArgs()
        {
            Arguments = "";
            foreach (var arg in Args)
            {
                string cmd = arg.ToCommandString();
                if (!string.IsNullOrEmpty(cmd))
                    Arguments += cmd;
            }
        }

        Argument DestinationPATH = new Argument("Destination Path", Properties.Settings.Default.DestinationPath, "-d", "Target location for file downloads. Files will be distribute in the selected folder", Args, true);
        Argument DirectoryPATH = new Argument("Download Path", Properties.Settings.Default.DirectoryPath, "-D", "Exact location for file downloads. All files will go to selected folder", Args, true, false);
        Argument FileName = new Argument("File Name", Properties.Settings.Default.FileName, "-f", "Files naming structure", Args, true, false);
        Argument NoOverwrites = new Argument("No overwrites", " ", "--no-overwrites", "Skip files that already exist", Args, false);
        Argument NoProgress = new Argument("No Progress", " ", "--no-progress", "Removes console progress", Args, false);
        Argument ErrorLog = new Argument("Error Log", Properties.Settings.Default.ErrorLog, "-e", "Path to save error logs", Args, false);
        Argument Retries = new Argument("Retries", Properties.Settings.Default.Retries, "-R", "Maximum number of retries for failed HTTP requests. -1 for infinite retries", Args, true);
        Argument Sleep = new Argument("Sleep", Properties.Settings.Default.Sleep, "--sleep", " Number of seconds to wait before each download. This can be either a constant value or a range (e.g. 2.7 or 2.0-3.5)", Args, true, false);
        Argument Range = new Argument("Range", Properties.Settings.Default.Range, "--range", "Index range(s) specifying which files to download. These can be either a constant value, range, or slice (e.g. '5', '8-20', or '1:24:3')", Args, true, false);
        Argument UserName = new Argument("Username", Properties.Settings.Default.Username, "-u", "Username for sites that require authentication", Args, true, false);
        Argument Password = new Argument("Password", Properties.Settings.Default.Password, "-p", "Password for sites that require authentication", Args, true, false);
        Argument BrowserCookies = new Argument("Cookies from Browser", Properties.Settings.Default.BrowserCookies, "--cookies-from-browser", "Use cookies from a supported browser to authenticate requests. (e.g. 'edge' or 'firefox')", Args, true, true);
        Argument ExtraArgs = new Argument("Extra Arguments", Properties.Settings.Default.ExtraArgs, "", "Any additional arguments you want to add to the command. Make sure to use the correct syntax", Args, true, false);



        public class Argument
        {
            public string Name { get; }
            public string Value { get; set; }
            public string Command { get; }
            public string Description { get; }
            public bool Enabled { get; set; }
            public bool Visible { get; }
            List<Argument> Container;

            public Argument(string name, string value, string command, string description, List<Argument> container, bool visible, bool enabled = true)
            {
                Name = name;
                Value = value;
                Command = command;
                Description = description;
                Enabled = enabled;
                Visible = visible;
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
        public class ArgumentState
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Enabled { get; set; }
        }
        public static void SaveArguments()
        {
            var state = MainForm.Args.Select(a => new ArgumentState
            {
                Name = a.Name,
                Value = a.Value,
                Enabled = a.Enabled
            }).ToList();

            Properties.Settings.Default.ArgumentsState = JsonSerializer.Serialize(state);

            Properties.Settings.Default.Save();
        }

        public void LoadArguments()
        {
            string json = Properties.Settings.Default.ArgumentsState;

            if (string.IsNullOrWhiteSpace(json))
                return;

            var state = JsonSerializer.Deserialize<List<ArgumentState>>(json);

            foreach (var saved in state)
            {
                var arg = MainForm.Args
                    .FirstOrDefault(a => a.Name == saved.Name);

                if (arg != null)
                {
                    arg.Value = saved.Value;
                    arg.Enabled = saved.Enabled;
                }
            }
        }

        //Clipboard detector
        private void sharpClipboard1_ClipboardChanged(object sender, WK.Libraries.SharpClipboardNS.SharpClipboard.ClipboardChangedEventArgs e)
        {
            if (Status == "Downloading")
            {
                NotificationShow("Operacion en curso", "Espera a que termine la descarga para añadir nuevas Urls");
                return;
            }  
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                string contenido = e.Content.ToString().Trim();

                if (ValidUrl(contenido) && !RepetedUrl(contenido))
                {
                    AddUrlToDGV(contenido);

                    int count = UrlCount();
                    if(Properties.Settings.Default.NotificationPerLink != -1 && Properties.Settings.Default.NotificationPerLink > 0 && count % Properties.Settings.Default.NotificationPerLink == 0)
                    {
                        NotificationShow("La URL fue copiada correctamente.", "Has añadido " + count + " Urls");
                    }
                    CurrentUrlsUpd();
                }
            }
        }

        string Status = "Idle";
        //Notification

        public static void NotificationShow(string title, string desc, bool playsound = true)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(desc);
                builder.AddAppLogoOverride(new Uri(Path.Combine(Environment.CurrentDirectory, "images/icon.png")), ToastGenericAppLogoCrop.Default);
                if (!playsound)
                {
                    builder.AddAudio(null);
                }
                else
                {
                    builder.AddAudio(new Uri("ms-winsoundevent:Notification.Default"));
                }
                builder.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing notification:\n{ex.Message}", "Notification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        public static void ColorComponents(Control parent)
        {
            TraverseAllControls(parent, control =>
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
            tsbtnArgs.Image = Image.FromFile("images/args.png");
            btnClear.BackgroundImage = Image.FromFile("images/clear.png");
            btnClear.ImageAlign = ContentAlignment.MiddleCenter;
            btnClear.BackgroundImageLayout = ImageLayout.Stretch;
            this.Icon = ConvertImageToIcon("images/icon.png");
        }

        private void WindowConfig()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ControlBox = true;
            this.ShowIcon = true;
        }

        private void PaintErrors()
        {
            foreach (DataGridViewRow row in dgvUrls.Rows)
            {
                if (row.Cells[1].Value != null)
                {
                    string url = row.Cells[1].Value.ToString();
                    if (!string.IsNullOrWhiteSpace(url) && Urls.Contains(url))
                    {
                        row.DefaultCellStyle.ForeColor = Color.Red;
                    }
                }
            }
        }

        public static float _currentScale = 1f;

        public static void FontChange(Control form)
        {
            if(Properties.Settings.Default.MainFont == null)
                return;
            TraverseAllControls(form, control =>
            {
                control.Font = null;
            });

            var newFont = Properties.Settings.Default.MainFont;

            float newScale = newFont.Size / form.Font.Size;
            float deltaScale = newScale / MainForm._currentScale;

            form.SuspendLayout();

            form.Font = newFont;
            form.Scale(new SizeF(deltaScale, deltaScale));

            _currentScale = newScale;

        }
        private void ForceRefresh(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is ToolStrip ts)
                {
                    ts.SuspendLayout();
                    ts.Font = this.Font;

                    foreach (ToolStripItem item in ts.Items)
                    {
                        item.Font = this.Font;
                        item.AutoSize = true;
                    }

                    ts.PerformLayout();
                    ts.ResumeLayout();
                }
                if (control is Button btn)
                {
                    if (btn.Name == "btnStartdownload")
                    {

                    }
                }

                if (control.HasChildren)
                    ForceRefresh(control);
            }
        }
        private void CurrentUrlsUpd()
        {
            ChangeStatusLabel($"{UrlCount()} Urls");
        }
        private void dgvUrls_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            if (e.Button == MouseButtons.Left)
            {
                OpenUrl(dgvUrls.Rows[e.RowIndex].Cells[1].Value.ToString());
            }
            else if (e.Button == MouseButtons.Right)
            {
                dgvUrls.Rows.RemoveAt(e.RowIndex);
                CurrentUrlsUpd();
            }
        }

        //Icon
        public static Icon ConvertImageToIcon(string imagePath, int size = 256)
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
        public static void OpenUrl(string url)
        {
            try
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("La URL no es válida.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el enlace.\n" + ex.Message);
            }
        }


        //TXB
        private void txbAddUrl_TextChanged(object sender, EventArgs e)
        {
            foreach (string url in UrlExtractor(txbAddUrl.Text))
            {
                if (!RepetedUrl(url))
                {
                    AddUrlToDGV(url);
                }
            }
            txbAddUrl.Clear();
        }

        //Errors Manager
        private bool Errors()
        {
            string errorLogPath = Properties.Settings.Default.ErrorLog;
            if (string.IsNullOrWhiteSpace(errorLogPath) || !File.Exists(errorLogPath))
                return false;
            try
            {
                string[] errorLines = File.ReadAllLines(errorLogPath);
                NotificationShow("Error Detectado", errorLines.Length.ToString() + " Urls");
                foreach (string UrlError in errorLines)
                {
                    AddUrlToDGV(UrlError);
                    switch (ExtractSiteName(UrlError))
                    {
                        case "pixiv":
                            PixivTokenMissing();
                            break;
                        case "x":
                            MessageBox.Show("Twitter ha cambiado su sistema de autenticación, es necesario añadir las cookies de tu navegador para seguir descargando de esta plataforma", "Twitter Error");
                            break;
                    }
                }
                File.Delete(errorLogPath);
                PaintErrors();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer el archivo: " + ex.Message);
                return true;
            }
        }

        private void PixivTokenMissing()
        {
            MessageBox.Show("Comprueba o añade tu refresh-token de Pixiv, \nSigue las instrucciones del CMD", "Pixiv Error");
            var startInfo = new ProcessStartInfo()
            {
                FileName = "gallery-dl",
                Arguments = "oauth:pixiv",
                UseShellExecute = false,
                CreateNoWindow = false
            };
            
            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
            }
        }

        private void tsbtnArgs_Click(object sender, EventArgs e)
        {
            ArgsForm argsForm = new ArgsForm();
            argsForm.ShowDialog();
        }
        //Checks for updates
        private void CheckGalleryDlUpdate()
        {
            try
            {
                // 1️⃣ Obtener versión instalada
                var installedInfo = new ProcessStartInfo()
                {
                    FileName = "gallery-dl",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                string installedVersion;

                using (var process = new Process())
                {
                    process.StartInfo = installedInfo;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();

                    var match = Regex.Match(output, @"\d+\.\d+\.\d+");
                    if (!match.Success)
                        return;

                    installedVersion = match.Value;
                }

                // 2️⃣ Obtener versión más reciente desde pip
                var latestInfo = new ProcessStartInfo()
                {
                    FileName = "py",
                    Arguments = "-m pip index versions gallery-dl",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                string latestVersion = null;

                using (var process = new Process())
                {
                    process.StartInfo = latestInfo;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();

                    // Buscar línea LATEST: x.x.x
                    var match = Regex.Match(output, @"LATEST:\s+(\d+\.\d+\.\d+)");
                    if (match.Success)
                        latestVersion = match.Groups[1].Value;
                }

                if (latestVersion == null)
                    return;

                // 3️⃣ Comparar versiones
                Version installed = new Version(installedVersion);
                Version latest = new Version(latestVersion);

                if (installed < latest)
                {
                    MessageBox.Show(
                        $"Nueva versión disponible.\n\nInstalada: {installed}\nÚltima: {latest}",
                        "Actualización disponible",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch
            {
                // Opcional: manejar errores silenciosamente
            }
        }
        private void tsbtnLog_Click(object sender, EventArgs e)
        {
            LogForm log = new LogForm();
            log.ShowDialog(); 
        }
        public class MiniForm : Form
        {
            private readonly FlowLayoutPanel FieldPanel;
            private readonly FlowLayoutPanel _buttonPanel;

            public void MiniWindowConfig()
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.MaximizeBox = false;
                this.MinimizeBox = true;
                this.ControlBox = true;
                this.ShowIcon = true;
                this.Icon = MainForm.ConvertImageToIcon("images/icon.png");
            }

            public void FontAndColorMini()
            {
                MainForm.FontChange(this);
                MainForm.TraverseAllControls(this, control =>
                {
                    if (control is not Panel)
                    {
                        control.BackColor = Properties.Settings.Default.MainBackColor;
                        control.ForeColor = Properties.Settings.Default.MainForeColor;
                    }
                });
            }

            public MiniForm(string title = "")
            {
                Text = title;

                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;

                AutoScaleMode = AutoScaleMode.Font;

                AutoSize = true;
                MaximumSize = new Size(800, 800);
                Padding = new Padding(10);

                FlowLayoutPanel fieldPanel = new FlowLayoutPanel();
                fieldPanel.FlowDirection = FlowDirection.TopDown;
                fieldPanel.AutoSize = true;
                fieldPanel.Margin = new Padding(0);
                fieldPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                fieldPanel.WrapContents = true;
                fieldPanel.Dock = DockStyle.Fill;
                fieldPanel.AutoScroll = true;

                FieldPanel = fieldPanel;

                _buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 40
                };

                this.Controls.Add(FieldPanel);
                this.Controls.Add(_buttonPanel);
            }


            // 🔹 Agregar cualquier control
            public void AddControl(Control control, bool flow = false)
            {
                control.AutoSize = true;
                control.Margin = new Padding(6);
                FieldPanel.Controls.Add(control);
                FieldPanel.SetFlowBreak(control, flow);
            }

            public Button AddButton(string text, Action onClick, bool closeOnClick = true)
            { 
                var button = new Button 
                { 
                    Text = text, 
                    AutoSize = true 
                }; 
                button.Click += (s, e) => { onClick?.Invoke(); 
                if (closeOnClick) Close(); }; 
                _buttonPanel.Controls.Add(button); 
                return button; 
            }
        }
    }
}
