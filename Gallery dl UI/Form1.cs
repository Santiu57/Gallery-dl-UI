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
            ApiLoad();
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
                control.Enabled = false;
            });

            BuilArgs();

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

                    foreach (var api in ApiSites)
                    {
                        if (api.Condition(site))
                        {
                            await api.ExecuteAsync(url, Arguments);
                            handled = true;
                            break;
                        }
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
            if (Status == "Downloading")
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

            public UrlFilter(Func<string, bool> condition, Func<string, string> action, List<UrlFilter> container)
            {
                Condition = condition;
                Action = action;
                container.Add(this);
            }
        }

        UrlFilter CunnyX = new UrlFilter(url => ExtractSiteName(url) == "cunnyx", url => url.Replace("cunnyx", "x"), UrlsFilters);
        UrlFilter SkibidiX = new UrlFilter(url => ExtractSiteName(url) == "skibidix", url => url.Replace("skibidix", "x"), UrlsFilters);


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

        public static void NotificationShow(string title, string desc, bool playsound = true)
        {
            if (!Properties.Settings.Default.ShowNotifs)
                return;
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
            tsbtnApiSites.Image = Image.FromFile("images/api.png");
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
            MessageBox.Show(Lang.PixivError, "Pixiv Error");
            var startInfo = new ProcessStartInfo()
            {
                FileName = "gallery-dl",
                Arguments = "oauth:pixiv",
                UseShellExecute = true,
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

            public MiniForm(string title = "", Size? size = null)
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
            MiniForm Api = new MiniForm("API Sites Manager");
            DataGridView sites = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeight = Properties.Settings.Default.MainFont != null ? (int)(Properties.Settings.Default.MainFont.Size * 2.5) : 40
            };
            sites.CellMouseClick += (s, e) =>
            {
                if (e.RowIndex == sites.RowCount - 1)
                    return;
                if (e.Button != MouseButtons.Right)
                    return;

                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                string site = sites.Rows[e.RowIndex].Cells[0].Value?.ToString();

                if (string.IsNullOrWhiteSpace(site))
                {
                    sites.Rows.RemoveAt(e.RowIndex);
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

                sites.Rows.RemoveAt(e.RowIndex);
            };
            Api.FormClosing += (s, e) =>
            {
                var newSites = sites.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells[0].Value != null)
                    .Select(r => r.Cells[0].Value.ToString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                // Eliminar los que ya no existen
                ApiSites.RemoveAll(a => !newSites.Contains(a.Site));

                // Agregar nuevos
                foreach (var site in newSites)
                {
                    if (!ApiSites.Any(a => a.Site == site && a.Site != null))
                        CreateApi(site);
                }

                ApiSave();
            };
            Api.SetMainControl(sites);
            sites.Columns.Add("Sites", "Sites");
            foreach (var api in ApiSites)
            {
                if (!string.IsNullOrWhiteSpace(api.Site))
                    sites.Rows.Add(api.Site);
            }
            Api.FontAndColorMini();
            Api.ShowDialog();

        }
    }
}
