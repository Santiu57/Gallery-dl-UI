using Gallery_dl_UI.Properties;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WK.Libraries.SharpClipboardNS;
using static Gallery_dl_UI.LogForm;
using static Gallery_dl_UI.MainForm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Gallery_dl_UI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {

            // Coloca esto en el inicio de tu programa (ej. Form_Load o Main)
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Extraemos los argumentos
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Ejecutamos en el hilo de la UI si es necesario
                Application.OpenForms?[0]?.Invoke(new Action(() =>
                {
                    if (args.Contains("action"))
                    {
                        string value = args["action"];

                        if (value == "getPixivToken")
                        {
                            getPixivToken(); // Tu método existente
                        }

                        // Lógica por defecto: Traer al frente
                        var window = Application.OpenForms[0];
                        if (window?.WindowState == FormWindowState.Minimized)
                            window.WindowState = FormWindowState.Normal;

                        window?.Activate();
                        window?.Focus();
                    }
                }));
            };

            InitializeComponent();
            ColorComponents(this);
            LoadImages();
            WindowConfig();
            FontChange(this);
            ForceRefresh(this);
            LoadArguments();
            ApiLoad();
            FiltersLoad();
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
            CheckGalleryDlUpdate();
        }

        //Buttons
        private async void button1_Click(object sender, EventArgs e)
        {
            TraverseAllControls(this, control =>
            {
                if (control is ToolStrip)
                {
                    TraverseAllControls(control, tscontrols =>
                    {
                        tscontrols.Enabled = false;
                    });
                }
                if (control is Button)
                {
                    control.Enabled = false;
                }
            });

            InitYTArgs();

            BuilArgs();
            BuildYTArgs();

            Urls = UrlExtractor(dgvUrlsFusion());

            NotificationShow(Lang.OperationInit, $"{Urls.Count.ToString()} {Lang.UrlsToDownload}");

            await LinksQueue();
        }
        private void tsbtnConfig_Click(object sender, EventArgs e)
        {
            Config config = new Config();
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
            CurrentUrlsUpd();
        }
        // GalleryDl Manager
        List<string> Urls = new List<string>();
        private SemaphoreSlim _semaphore;

        public static async Task RunGalleryDl(string url, string arguments)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "gallery-dl",
                Arguments = arguments + url,
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

            ChangeStatusLabel($"{0}/{total} {Lang.Completed}");
            Status = "Downloading";

            var tasks = Urls.Select(async url =>
            {
                await _semaphore.WaitAsync(); // GLOBAL
                try
                {
                    string site = ExtractSiteName(url);

                    bool handled = false;

                    foreach (var filter in UrlsFilters)
                    {
                        if (filter.Condition(url))
                        {
                            url = filter.Action(url);
                        }
                    }

                    foreach (var api in ApiSites)
                    {
                        if (api.Condition(site))
                        {
                            await api.ExecuteAsync(url, Arguments);
                            handled = true;
                            break;
                        }
                    }

                    if (YTsites.Contains(site))
                    {
                        await RunYTdlp(url);
                        handled = true;
                    }

                    if (!handled)
                    {
                        await RunGalleryDl(url, Arguments);
                    }

                    int done = Interlocked.Increment(ref completed);

                    Invoke(() =>
                    {
                        ChangeStatusLabel($"{done}/{total} {Lang.Completed}");
                        SaveLog(url);
                        RowChangeProgresBar(done, total);
                    });
                }
                finally
                {
                    _semaphore.Release(); // GLOBAL RELEASE
                }
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
                NotificationShow(Lang.Completed, Lang.OperationCompleted);
            }
            ChangeStatusLabel(Lang.Sleeping);
            Status = "Idle";
            ChangeProgresBar(0);
        }

        public void AddUrl(string urls)
        {
            foreach (string url in UrlExtractor(urls))
            {
                string extractedUrl = url;
                if (!RepetedUrl(extractedUrl))
                {
                    foreach (var filter in UrlsFilters)
                    {
                        if (filter.Condition(extractedUrl))
                        {
                            extractedUrl = filter.Action(extractedUrl);
                        }
                    }
                    AddUrlToDGV(extractedUrl);
                    int count = UrlCount();
                    if (Properties.Settings.Default.NotificationPerLink != -1 && Properties.Settings.Default.NotificationPerLink > 0 && count % Properties.Settings.Default.NotificationPerLink == 0)
                    {
                        NotificationShow(Lang.UrlCopied, string.Format(Lang.UrlsAdded, count));
                    }
                    CurrentUrlsUpd();
                }
            }
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
                if (arg != null)
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
                bool hasDirectoryArg = Args.Any(a => (a.Command == "-D" || a.Command == "-d") && a.Enabled);
                if (hasDirectoryArg)
                {
                    if (Args.Any(a => a.Command == "-D" && a.Enabled))
                    {
                        arg = Args.First(a => a.Command == "-D" && a.Enabled);
                    }
                    else
                    {
                        arg = Args.First(a => a.Command == "-d" && a.Enabled);
                    }
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
        Argument NoOverwrites = new Argument("No overwrites", "--no-overwrites", "", "Skip files that already exist", Args, false);
        Argument NoProgress = new Argument("No Progress", "--no-progress", " ", "Removes console progress", Args, false);
        Argument ErrorLog = new Argument("Error Log", Properties.Settings.Default.ErrorLog, "-e", "Path to save error logs", Args, false);
        Argument Retries = new Argument("Retries", Properties.Settings.Default.Retries, "-R", "Maximum number of retries for failed HTTP requests. -1 for infinite retries", Args, true);
        Argument Sleep = new Argument("Sleep", Properties.Settings.Default.Sleep, "--sleep", " Number of seconds to wait before each download. This can be either a constant value or a range (e.g. 2.7 or 2.0-3.5)", Args, true, false);
        Argument Range = new Argument("Range", Properties.Settings.Default.Range, "--range", "Index range(s) specifying which files to download. These can be either a constant value, range, or slice (e.g. '5', '8-20', or '1:24:3')", Args, true, false);
        Argument UserName = new Argument("Username", Properties.Settings.Default.Username, "-u", "Username for sites that require authentication", Args, true, false);
        Argument Password = new Argument("Password", Properties.Settings.Default.Password, "-p", "Password for sites that require authentication", Args, true, false);
        Argument BrowserCookies = new Argument("Cookies from Browser", Properties.Settings.Default.BrowserCookies, "--cookies-from-browser", "Use cookies from a supported browser to authenticate requests. (e.g. 'edge' or 'firefox')", Args, true, true);
        Argument ExtraArgs = new Argument("Extra Arguments", Properties.Settings.Default.ExtraArgs, "", "Any additional arguments you want to add to the command. Make sure to use the correct syntax", Args, true, false);

        // YT-DLP Args
        public static List<Argument> YTArgs = new();

        Argument YTPath = new Argument(
            "YTPath",
            Path.Combine(Properties.Settings.Default.DestinationPath
            ?? Properties.Settings.Default.DirectoryPath
            ?? Environment.CurrentDirectory, "youtube"),
            "-P",
            "Output template path",
            YTArgs,
            true
        );

        Argument YTFormat = new Argument(
            "Format",
            Properties.Settings.Default.YTFormat,
            "--merge-output-format",
            "Video format selector",
            YTArgs,
            true,
            !Properties.Settings.Default.YTExtractAu
        );

        Argument YTExtractAudio = new Argument(
            "Extract audio",
            "--extract-audio",
            "",
            "Extract audio only",
            YTArgs,
            false,
            Properties.Settings.Default.YTExtractAu
        );

        Argument YTAudioFormat = new Argument(
            "Audio format",
            Properties.Settings.Default.YTAuFormat,
            "--audio-format",
            "Audio format",
            YTArgs,
            true,
            Properties.Settings.Default.YTExtractAu
        );

        Argument YTResolution = new Argument(
            "Video Resolution",
            Properties.Settings.Default.YTResolution,
            "-f",
            "Audio format",
            YTArgs,
            true,
            !Properties.Settings.Default.YTExtractAu
        );

        Argument YTffmpeg = new Argument(
            "ffmpeg location",
            Properties.Settings.Default.ffmpeg,
            "--ffmpeg-location",
            "",
            YTArgs,
            true,
            true
            );

        Argument YTRemoteComponents = new Argument(
            "remote components",
            "ej:sjithub",
            "--remote-components",
            "(Enables JS challenge solver using deno)",
            YTArgs,
            true,
            true
        );

        void InitYTArgs()
        {
            YTArgs.Clear();

            YTPath = new Argument(
                "YTPath",
                Properties.Settings.Default.YTOutput,
                "-P",
                "",
                YTArgs,
                true
            );

            YTFormat = new Argument(
                "Format",
                Properties.Settings.Default.YTFormat,
                "--merge-output-format",
                "",
                YTArgs,
                true,
                !Properties.Settings.Default.YTExtractAu
            );

            YTExtractAudio = new Argument(
                "Extract audio",
                "--extract-audio",
                "",
                "",
                YTArgs,
                false,
                Properties.Settings.Default.YTExtractAu
            );

            YTAudioFormat = new Argument(
                "Audio format",
                Properties.Settings.Default.YTAuFormat,
                "--audio-format",
                "",
                YTArgs,
                true,
                Properties.Settings.Default.YTExtractAu
            );

            YTResolution = new Argument(
                "Video Resolution",
                Properties.Settings.Default.YTResolution,
                "-f",
                "",
                YTArgs,
                true,
                !Properties.Settings.Default.YTExtractAu
            );
            YTffmpeg = new Argument(
                "ffmpeg location",
                Properties.Settings.Default.ffmpeg,
                "--ffmpeg-location",
                "",
                YTArgs,
                true,
                true
            );

            YTRemoteComponents = new Argument(
                "Remote components",
                "ej:sjithub",
                "--remote-components",
                "(Enables JS challenge solver using deno)",
                YTArgs,
                true,
                true
            );
        }

        void BuildYTArgs()
        {
            YTArguments = "";
            foreach (var arg in YTArgs)
            {
                string cmd = arg.ToCommandString();
                if (!string.IsNullOrEmpty(cmd))
                    YTArguments += cmd;
            }
        }

        public string YTArguments = "";

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
            string contenido = e.Content.ToString().Trim();
            if (Status == "Downloading" && ValidUrl(contenido))
            {
                NotificationShow(Lang.OperationInProgress, Lang.WaitForFinish);
                return;
            }
            AddUrl(contenido);
        }

        string Status = "Idle";

        //Urls filter

        public static List<UrlFilter> UrlsFilters = new List<UrlFilter>();
        public class UrlFilter
        {
            public Func<string, bool> Condition { get; set; }
            public Func<string, string> Action { get; set; }
            public string ModSite { get; set; }
            public string Site { get; set; }

            public UrlFilter(string modsite, string site)
            {
                ModSite = modsite;
                Site = site;
                Condition = url => ExtractSiteName(url).Equals(ModSite, StringComparison.OrdinalIgnoreCase);

                Action = url =>
                {
                    var uri = new Uri(url);
                    var builder = new UriBuilder(uri);
                    builder.Host = builder.Host.Replace(ModSite, Site, StringComparison.OrdinalIgnoreCase);
                    return builder.Uri.ToString();
                };
            }
        }
        public void FiltersSave()
        {
            List<string> filters = new List<string> { };
            foreach (var filter in UrlsFilters)
            {
                string fullFilter = $"{filter.ModSite}-{filter.Site}";
                filters.Add(fullFilter);
            }

            Properties.Settings.Default.Filters = string.Join("|", filters);
            Properties.Settings.Default.Save();
        }
        public void FiltersLoad()
        {
            UrlsFilters.Clear();

            var saved = Properties.Settings.Default.Filters;

            if (string.IsNullOrWhiteSpace(saved))
                return;

            var filters = saved.Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var filter in filters.Distinct())
            {
                var parts = filter.Split('-', 2);

                if (parts.Length != 2)
                    continue;

                CreateFilter(parts[0], parts[1]);
            }
        }
        public void CreateFilter(string modsite, string site)
        {
            UrlFilter filter = new UrlFilter(modsite, site);
            UrlsFilters.Add(filter);
        }

        //Sites to have just one instance at the same time
        public static List<ApiSite> ApiSites = new List<ApiSite>();
        public class ApiSite
        {
            public string Site { get; }
            public Func<string, bool> Condition => site => site == Site;

            private readonly SemaphoreSlim _Apisemaphore = new(1, 1);

            public ApiSite(string site)
            {
                Site = site;
            }


            public async Task ExecuteAsync(string url, string Arguments)
            {
                await _Apisemaphore.WaitAsync();
                try
                {
                    await RunGalleryDl(url, Arguments);
                }
                finally
                {
                    _Apisemaphore.Release();
                }
            }
        }
        public void ApiSave()
        {
            var sites = ApiSites
                .Where(a => !string.IsNullOrWhiteSpace(a.Site))
                .Select(a => a.Site);

            Properties.Settings.Default.Apis = string.Join("|", sites);
            Properties.Settings.Default.Save();
        }
        public void ApiLoad()
        {
            ApiSites.Clear();

            var saved = Properties.Settings.Default.Apis;

            if (string.IsNullOrWhiteSpace(saved))
                return;

            var apis = saved.Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var api in apis.Distinct())
            {
                CreateApi(api);
            }
        }
        public void CreateApi(string site)
        {
            ApiSite api = new ApiSite(site);
            ApiSites.Add(api);
        }

        //Notification

        public static void NotificationShow(string title, string desc, bool playsound = true, ToastDuration duration = ToastDuration.Short, string actionArgs = "action=viewMain")
        {
            if (!Properties.Settings.Default.ShowNotifs)
                return;
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(desc)
                    // Este argumento se envía a la app cuando el usuario hace clic
                    .AddArgument("action", actionArgs);

                builder.AddAppLogoOverride(new Uri(Path.Combine(Environment.CurrentDirectory, "images/icon.png")), ToastGenericAppLogoCrop.Default);

                if (!playsound)
                    builder.AddAudio(null);
                else
                    builder.AddAudio(new Uri("ms-winsoundevent:Notification.Default"));

                builder.SetToastDuration(duration);
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
            tsbtnFiltersApis.Image = Image.FromFile("images/filter.png");
            tsbtnytdlp.Image = Image.FromFile("images/yt.png");
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

        public static void FontChange(Control form)
        {
            if (Properties.Settings.Default.MainFont == null)
                return;

            TraverseAllControls(form, control =>
            {
                control.Font = null;
            });

            var newFont = Properties.Settings.Default.MainFont;

            float scale = newFont.Size / form.Font.Size;

            form.SuspendLayout();

            form.Font = newFont;
            form.Scale(new SizeF(scale, scale));

            form.ResumeLayout();
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
                if (Status == "Downloading")
                    return;
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
                    MessageBox.Show("Invalid URL");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't open the URL.\n" + ex.Message);
            }
        }


        //TXB
        private void txbAddUrl_TextChanged(object sender, EventArgs e)
        {
            AddUrl(txbAddUrl.Text);
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
                NotificationShow(Lang.ThereWereErrors, string.Format(Lang.UrlsWithErrors, errorLines.Length.ToString()));
                foreach (string UrlError in errorLines)
                {
                    AddUrlToDGV(UrlError);
                    switch (ExtractSiteName(UrlError))
                    {
                        case "pixiv":
                            PixivTokenMissing();
                            break;
                        case "x":
                            NotificationShow(Lang.TwitterError, "Twitter Error");
                            break;
                    }
                }
                File.Delete(errorLogPath);
                PaintErrors();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading the error log: \n" + ex.Message);
                return true;
            }
        }

        private void PixivTokenMissing()
        {
            NotificationShow("Pixiv Error", Lang.PixivError, true, ToastDuration.Long, "getPixivToken");
        }
        private void getPixivToken()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = "gallery-dl oauth:pixiv",
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
                    NotificationShow(
                    Lang.GalleryDlUpdateTitle,
                    string.Format($"A new version of gallery-dl is available. \n Installed version: {installed} \n Latest version: {latest}")
                );
                }
            }
            catch
            {

            }
        }
        private void tsbtnLog_Click(object sender, EventArgs e)
        {
            LogForm log = new LogForm();
            log.ShowDialog();
        }
        public class MiniForm : Form
        {
            private readonly Panel _contentPanel;
            private readonly FlowLayoutPanel _buttonPanel;

            public Panel ContentPanel => _contentPanel;

            public MiniForm(string title = "", Size? size = null, bool bottomButton = false)
            {
                Text = title;

                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;

                AutoScaleMode = AutoScaleMode.Font;

                Size = size ?? new Size(600, 400);
                MinimumSize = new Size(100, 100);
                Padding = new Padding(10);

                // 🔹 Panel principal de contenido
                _contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true
                };

                // 🔹 Panel inferior de botones
                _buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 45,
                    Padding = new Padding(5)
                };

                Controls.Add(_contentPanel);

                if (bottomButton)
                    Controls.Add(_buttonPanel);
            }

            // 🔹 Agregar cualquier control manualmente
            public void AddControl(Control control, DockStyle dock = DockStyle.Top)
            {
                control.Dock = dock;
                _contentPanel.Controls.Add(control);
                _contentPanel.Controls.SetChildIndex(control, 0); // mantiene orden natural
            }

            // 🔹 Agregar control que ocupe todo el espacio
            public void SetMainControl(Control control)
            {
                _contentPanel.Controls.Clear();
                control.Dock = DockStyle.Fill;
                _contentPanel.Controls.Add(control);
            }

            // 🔹 Agregar botón inferior
            public Button AddButton(string text, Action onClick, bool closeOnClick = true)
            {
                var button = new Button
                {
                    Text = text,
                    AutoSize = true
                };

                button.Click += (s, e) =>
                {
                    onClick?.Invoke();
                    if (closeOnClick)
                        Close();
                };

                _buttonPanel.Controls.Add(button);
                return button;
            }
            public void FontAndColorMini()
            {
                FontChange(this);
                ColorComponents(this);
            }
            public void MiniWindowConfig()
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.MaximizeBox = false;
                this.MinimizeBox = true;
                this.ControlBox = true;
                this.ShowIcon = true;
                this.Icon = MainForm.ConvertImageToIcon("images/icon.png");
            }
        }

        private void tsbtnApiSites_Click(object sender, EventArgs e)
        {
            MiniForm FiltersApi = new MiniForm("Filters and Api Sites");

            DataGridView Filters = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeight = Properties.Settings.Default.MainFont != null ? (int)(Properties.Settings.Default.MainFont.Size * 2.5) : 40
            };

            Filters.CellMouseClick += (s, e) =>
            {
                if (e.RowIndex == Filters.RowCount - 1)
                    return;
                if (e.Button != MouseButtons.Right)
                    return;

                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                string filter = Filters.Rows[e.RowIndex].Cells[0].Value?.ToString();

                if (string.IsNullOrWhiteSpace(filter))
                {
                    Filters.Rows.RemoveAt(e.RowIndex);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Delete Filter to '{filter}'?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;

                // Buscar primero
                var filterToRemove = UrlsFilters.FirstOrDefault(a => a.ModSite == filter);

                if (filterToRemove != null)
                {
                    UrlsFilters.Remove(filterToRemove);
                }

                Filters.Rows.RemoveAt(e.RowIndex);
            };

            FiltersApi.AddControl(new Label { Text = "Filters" });
            FiltersApi.AddControl(Filters);
            Filters.Columns.Add("Modificated Site", "ModSite");
            Filters.Columns.Add("Original Site", "Site");
            foreach (var filter in UrlsFilters)
            {
                if (!string.IsNullOrWhiteSpace(filter.Site) || !string.IsNullOrWhiteSpace(filter.ModSite))
                    Filters.Rows.Add(filter.ModSite, filter.Site);
            }

            DataGridView ApiSite = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeight = Properties.Settings.Default.MainFont != null ? (int)(Properties.Settings.Default.MainFont.Size * 2.5) : 40
            };
            ApiSite.CellMouseClick += (s, e) =>
            {
                if (e.RowIndex == ApiSite.RowCount - 1)
                    return;
                if (e.Button != MouseButtons.Right)
                    return;

                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                string site = ApiSite.Rows[e.RowIndex].Cells[0].Value?.ToString();

                if (string.IsNullOrWhiteSpace(site))
                {
                    ApiSite.Rows.RemoveAt(e.RowIndex);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Delete API site '{site}'?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;

                // Buscar primero
                var apiToRemove = ApiSites.FirstOrDefault(a => a.Site == site);

                if (apiToRemove != null)
                {
                    ApiSites.Remove(apiToRemove);
                }

                ApiSite.Rows.RemoveAt(e.RowIndex);
            };
            FiltersApi.FormClosing += (s, e) =>
            {
                var newSites = ApiSite.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells[0].Value != null)
                    .Select(r => r.Cells[0].Value?.ToString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                ApiSites.RemoveAll(a => !newSites.Contains(a.Site));

                foreach (var site in newSites)
                {
                    if (!ApiSites.Any(a => a.Site == site && a.Site != null))
                        CreateApi(site);
                }

                UrlsFilters.Clear();

                foreach (DataGridViewRow row in Filters.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    var modSite = row.Cells[0].Value?.ToString();
                    var site = row.Cells[1].Value?.ToString();

                    if (string.IsNullOrWhiteSpace(modSite) || string.IsNullOrWhiteSpace(site))
                        continue;

                    CreateFilter(modSite.Trim(), site.Trim());
                }

                FiltersSave();
                ApiSave();
            };

            FiltersApi.AddControl(new Label { Text = "Api Sites" });
            FiltersApi.AddControl(ApiSite);
            ApiSite.Columns.Add("Sites", "Sites");
            foreach (var api in ApiSites)
            {
                if (!string.IsNullOrWhiteSpace(api.Site))
                    ApiSite.Rows.Add(api.Site);
            }


            FiltersApi.FontAndColorMini();
            FiltersApi.ShowDialog();

        }
        private Button Createbtn(string text, Action click)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Click += (s, e) => click();
            return btn;
        }
        private void tsbtnytdlp_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(Properties.Settings.Default.YTOutput))
            {
                Properties.Settings.Default.YTOutput = Path.Combine(Environment.CurrentDirectory, "youtube");
                Properties.Settings.Default.Save();
            }

            MiniForm ytdlp = new MiniForm("YT-DLP config (Youtube)", new Size(300, 300));

            //Resolution
            ComboBox resolution = new ComboBox();
            resolution.DropDownStyle = ComboBoxStyle.DropDownList;
            resolution.Enabled = !Properties.Settings.Default.YTExtractAu;

            foreach (string res in YTdlpResolutions)
            {
                resolution.Items.Add(res);
            }
            string Res = Properties.Settings.Default.YTResolution;

            switch (Res)
            {
                case "bestvideo+bestaudio/best":
                    Res = "Best";
                    break;

                case "bestvideo[height<=4320]+bestaudio/best[height<=4320]":
                    Res = "4320p -> 8K";
                    break;

                case "bestvideo[height<=2160]+bestaudio/best[height<=2160]":
                    Res = "2160p -> 4K";
                    break;

                case "bestvideo[height<=1440]+bestaudio/best[height<=1440]":
                    Res = "1440p -> Quad HD";
                    break;

                case "bestvideo[height<=1080]+bestaudio/best[height<=1080]":
                    Res = "1080p -> Full HD";
                    break;

                case "bestvideo[height<=720]+bestaudio/best[height<=720]":
                    Res = "720p ->HD";
                    break;

                case "bestvideo[height<=480]+bestaudio/best[height<=480]":
                    Res = "480p ->SD";
                    break;

                case "bestvideo[height<=360]+bestaudio/best[height<=360]":
                    Res = "360p ->SD";
                    break;

                case "bestvideo[height<=240]+bestaudio/best[height<=240]":
                    Res = "240p -> SD";
                    break;

                case "bestvideo[height<=144]+bestaudio/best[height<=144]":
                    Res = "144p -> SD";
                    break;
            }

            resolution.SelectedIndex = resolution.Items.IndexOf(Res);

            Button install = Createbtn("Install YT-dlp", () => installYtdlp());
            Button uptd = Createbtn("Update YT-dlp", () => UpdYtdlp());


            //Video Format
            ComboBox VidFormat = new ComboBox();
            VidFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            VidFormat.Enabled = !Properties.Settings.Default.YTExtractAu;

            foreach (string formats in YTdlpVidFormats)
            {
                VidFormat.Items.Add(formats);
            }
            VidFormat.SelectedIndex = VidFormat.Items.IndexOf(Properties.Settings.Default.YTFormat);

            CheckBox ExtractAu = new CheckBox
            {
                Text = "Extract Audio",
                Checked = Properties.Settings.Default.YTExtractAu,
            };

            ComboBox AuFormat = new ComboBox();
            AuFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            AuFormat.Enabled = Properties.Settings.Default.YTExtractAu;

            foreach (string formats in YTdlpAuFormats)
            {
                AuFormat.Items.Add(formats);
            }
            AuFormat.SelectedIndex = AuFormat.Items.IndexOf(Properties.Settings.Default.YTAuFormat);

            ExtractAu.CheckedChanged += (s, e) =>
            {
                switch (ExtractAu.Checked)
                {
                    case true:
                        resolution.Enabled = false;
                        VidFormat.Enabled = false;
                        AuFormat.Enabled = true;
                        break;
                    case false:
                        resolution.Enabled = true;
                        VidFormat.Enabled = true;
                        AuFormat.Enabled = false;
                        break;
                }
            };

            var ffmpeg = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            TextBox ffmpegdir = new TextBox() { PlaceholderText = "Path to ffmpeg", Text = Properties.Settings.Default.ffmpeg, Width = 200 };
            Button selectffdir = new Button() { BackgroundImage = Image.FromFile("images/folder.png"), Width = 40, BackgroundImageLayout = ImageLayout.Stretch };

            selectffdir.Click += (s, e) =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    ffmpegdir.Text = dialog.SelectedPath;
                }
            };

            ffmpeg.Controls.Add( ffmpegdir );
            ffmpeg.Controls.Add( selectffdir );

            var Output = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            TextBox Outputdir = new TextBox() { PlaceholderText = "Path to ffmpeg", Text = Properties.Settings.Default.YTOutput, Width = 200 };
            Button selectOutputdir = new Button() { BackgroundImage = Image.FromFile("images/folder.png"), Width = 40, BackgroundImageLayout = ImageLayout.Stretch };

            selectffdir.Click += (s, e) =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    selectOutputdir.Text = dialog.SelectedPath;
                }
            };

            Output.Controls.Add(Outputdir);
            Output.Controls.Add(selectOutputdir);

            ytdlp.FormClosing += (s, e) =>
            {
                Properties.Settings.Default.YTExtractAu = ExtractAu.Checked;
                Properties.Settings.Default.YTAuFormat = AuFormat.Text;
                Properties.Settings.Default.YTFormat = VidFormat.Text;

                Res = resolution.Text;

                switch (Res)
                {
                    case "Best":
                        Res = "bestvideo+bestaudio/best";
                        break;

                    case "4320p -> 8K":
                        Res = "bestvideo[height<=4320]+bestaudio/best[height<=4320]";
                        break;

                    case "2160p -> 4K":
                        Res = "bestvideo[height<=2160]+bestaudio/best[height<=2160]";
                        break;

                    case "1440p -> Quad HD":
                        Res = "bestvideo[height<=1440]+bestaudio/best[height<=1440]";
                        break;

                    case "1080p -> Full HD":
                        Res = "bestvideo[height<=1080]+bestaudio/best[height<=1080]";
                        break;

                    case "720p ->HD":
                        Res = "bestvideo[height<=720]+bestaudio/best[height<=720]";
                        break;

                    case "480p ->SD":
                        Res = "bestvideo[height<=480]+bestaudio/best[height<=480]";
                        break;

                    case "360p ->SD":
                        Res = "bestvideo[height<=360]+bestaudio/best[height<=360]";
                        break;

                    case "240p -> SD":
                        Res = "bestvideo[height<=240]+bestaudio/best[height<=240]";
                        break;

                    case "144p -> SD":
                        Res = "bestvideo[height<=144]+bestaudio/best[height<=144]";
                        break;
                }

                Properties.Settings.Default.YTResolution = Res;
                Properties.Settings.Default.ffmpeg = ffmpegdir.Text;
                Properties.Settings.Default.YTOutput = Outputdir.Text;

                Properties.Settings.Default.Save();
                MainForm.SaveArguments();
            };

            ytdlp.AddControl(new Label { Text = "Videos Resolution:" });
            ytdlp.AddControl(resolution);

            ytdlp.AddControl(new Label { Text = "Videos Format:" });
            ytdlp.AddControl(VidFormat);

            ytdlp.AddControl(new Label { Text = "Videos output folder:" });
            ytdlp.AddControl(Output);

            ytdlp.AddControl(ExtractAu);

            ytdlp.AddControl(new Label { Text = "Audio Format:" });
            ytdlp.AddControl(AuFormat);

            ytdlp.AddControl(new Label { Text = "Ffmpeg location:" });
            ytdlp.AddControl(ffmpeg);

            ytdlp.AddControl(new Label { Text = "" });
            ytdlp.AddControl(install);
            ytdlp.AddControl(uptd);

            ytdlp.MiniWindowConfig();
            ytdlp.FontAndColorMini();

            ytdlp.ShowDialog();

        }
        private void installYtdlp()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = "pip install yt-dlp",
                CreateNoWindow = false
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
            }
        }
        private void UpdYtdlp()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = "pip install -U yt-dlp",
                CreateNoWindow = false
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
            }
        }

        public async Task RunYTdlp(string url)
        {
            string errorLogPath = Properties.Settings.Default.ErrorLog;
            if (!File.Exists(Path.Combine(Properties.Settings.Default.ffmpeg, "ffmpeg.exe")))
            {
                NotificationShow("YT-dlp missing dependencies","FFmpeg not found, install it and specify the path to it");
                try
                {
                    string logEntry =
                        $"{url}{Environment.NewLine}";

                    File.AppendAllText(errorLogPath, logEntry);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to write to error log:\n" + ex.Message);
                }

                return;
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = "yt-dlp",
                Arguments = $"{YTArguments} \"{url}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    MessageBox.Show($"YT-DLP Error:\n{error}");
                    try
                    {
                        string logEntry =
                            $"{url}{Environment.NewLine}";

                        File.AppendAllText(errorLogPath, logEntry);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to write to error log:\n" + ex.Message);
                    }
                }
            }
        }



        public static readonly List<string> YTdlpResolutions = new List<string>
        {
            "Best",
            "4320p -> 8K",
            "2160p -> 4K",
            "1440p -> Quad HD",
            "1080p -> Full HD",
            "720p ->HD",
            "480p ->SD",
            "360p ->SD",
            "240p -> SD",
            "144p -> SD"
        };
        public static readonly List<string> YTdlpVidFormats = new List<string>
        {
            "mp4",
            "webm",
            "mkv",
        };
        public static readonly List<string> YTdlpAuFormats = new List<string>
        {
            "mp3",
            "aac",
            "alac",
            "opus",
            "vorbis",
            "m4a",
            "flac",
            "wav"
        };
        public static readonly List<string> YTsites = new List<string>
        {
            "youtube",
            "youtu"
        };

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dgvUrls.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "There are pending URLs. Do you really want to close?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
