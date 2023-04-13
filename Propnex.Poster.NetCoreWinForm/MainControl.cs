using CefSharp.DevTools.CSS;
using Propnex.Poster.Share;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Propnex.Poster.NetCoreWinForm
{
    public partial class MainControl : UserControl, ILocalEventHandler<LogEvent>, ISingletonDependency
    {
        private bool IsRun = false;
        private bool IsStop = false;

        public Func<Form> GetForm { get; set; }

        private Form CefPoster { get; set; }

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

        private async void timerWorker_Tick(object sender, EventArgs e)
        {
            if (CefPoster == null && IsStop == false)
            {
                IsStop = false;
                CefPoster = GetForm();
                CefPoster.FormClosed += (s, ev) =>
                {
                    CefPoster = null;
                    if (IsStop)
                    {
                        Application.Exit();
                        Environment.Exit(0);
                    }
                };
                CefPoster.Show();
                await Task.Delay(1000);
                if (CefPoster != null)
                {
                    await (CefPoster as IPosterStart).StartAsync();
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

        public async Task HandleEventAsync(LogEvent eventData)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(() =>
                {
                    richTextBox1.AppendText($"{eventData.Message}{Environment.NewLine}");
                });
            }
            else
            {
                richTextBox1.AppendText($"{eventData.Message}{Environment.NewLine}");
            }
            await Task.CompletedTask;
        }
    }
}
