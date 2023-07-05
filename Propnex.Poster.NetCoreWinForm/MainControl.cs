using Propnex.Poster.Share;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Propnex.Poster.NetCoreWinForm
{
    public partial class MainControl : UserControl, ILocalEventHandler<LogEvent>, ISingletonDependency
    {
        private bool IsRun = false;
        private bool IsStop = false;

        public Func<CefFrom> GetForm { get; set; }

        private CefFrom CefPoster { get; set; }

        public MainControl()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            IsRun = true;
            timerWorker.Start();
            btnStart.Enabled = !IsRun;
            btnStop.Enabled = IsRun;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (IsRun == false)
            {
                Application.Exit();
                Environment.Exit(0);
            }
            IsStop = true;
        }

        private  void timerWorker_Tick(object sender, EventArgs e)
        {
            if (CefPoster == null && IsStop == false)
            {
                IsStop = false;
                CefPoster = GetForm();
                CefPoster.FormClosed += (s, ev) =>
                {
                    //CefPoster.Dispose();
                    CefPoster = null;
                    if (IsStop)
                    {
                        Application.Exit();
                        Environment.Exit(0);
                    }
                };
                CefPoster.Show();
                Console.WriteLine("PosterStart");
                if (CefPoster != null)
                {
                    CefPoster.StartAsync();
                }
            }
            else
            {
                if (CefPoster == null && IsStop)
                {
                    Application.Exit();
                }
            }
        }

        public object reichTextLock = new object();

        public async Task HandleEventAsync(LogEvent eventData)
        {
            richTextBox1.BeginInvoke(() =>
            {
                lock (reichTextLock)
                {
                    richTextBox1.AppendText($"{eventData.Message}{Environment.NewLine}");
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                }

            });
            await Task.CompletedTask;
        }
    }
}
