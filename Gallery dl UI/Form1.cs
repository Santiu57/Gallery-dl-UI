using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using WK.Libraries.SharpClipboardNS;

namespace Gallery_dl_UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            urlCount = 0;

            BuilArgs();

            Urls = UrlExtractor(textBox1.Text);

            NotificationShow("Operacion Iniciada", Urls.Count.ToString() + " Urls a descargar", 1000);

            await LinksQueue();
        }
        string A;
        //GalleryDl Manager
        List<string> Urls = new List<string>();
        private async Task RunGalleryDl(string url)
        {
            A = Arguments + $" \"{url}\"";
            var startInfo = new ProcessStartInfo()
            {
                FileName = "gallery-dl",
                Arguments = Arguments + $" \"{url}\"",
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
                    // log error
                    File.AppendAllText("error.log", error);
                }
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
            for (int i = 0; i < Urls.Count; ++i)
            {
                var url = Urls[i];
                await Task.Run(() => RunGalleryDl(url));
                NotificationShow("En progreso...", (i+1).ToString() + "/" + (Urls.Count).ToString() + " Completados", 300);
                textBox1.Text = A;
            }

            Urls.Clear();
            NotificationShow("Completado", "Operación realizada con exito", 600);
            
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

        void SetDefaultDestinationPath()
        {
            Properties.Settings.Default.DestinationPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "gallery-dl");

            Properties.Settings.Default.Save();

        }

        //Clipboard detector
        private string LastUrl = "";
        private int urlCount = 0;
        private void sharpClipboard1_ClipboardChanged(object sender, WK.Libraries.SharpClipboardNS.SharpClipboard.ClipboardChangedEventArgs e)
        {
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                string contenido = e.Content.ToString().Trim();

                if (ValidUrl(contenido) && contenido != LastUrl)
                {
                    LastUrl = contenido;
                    urlCount++;

                    textBox1.Text += " " + contenido;

                    NotificationShow("La URL fue copiada correctamente.", "Has añadido " + urlCount.ToString() + " Urls",500);
                }
            }
        }
        //Notification
        NotifyIcon trayIcon = new NotifyIcon();

        private void NotificationShow(string Title, string Text, int time)
        {
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Visible = true;
            trayIcon.BalloonTipTitle = Title;
            trayIcon.BalloonTipText = Text;
            trayIcon.BalloonTipIcon = ToolTipIcon.Info;

            trayIcon.ShowBalloonTip(time);
        }
    }
}
