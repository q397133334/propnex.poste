using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;


namespace PropnexPoster.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            Title += $" Version{Assembly.GetEntryAssembly().GetFileVersion()}";
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            IsStart = false;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            IsStart = true;
            Thread thread = new Thread(ThreadPoster);
            thread.Start();
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        public bool IsStart = false;
        public bool IsRun = false;

        private void ThreadPoster()
        {
            while (IsStart)
            {
                IsRun = true;
                var run = _serviceProvider.GetService<PosterRun>();//  new PosterRun();
                run.MessageEvent = Log;
                run.TaskInfoEvent = TaskInfoEvent;
                run.Run().Wait();
                IsRun = false;
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Close();
            }));
        }

        private void Log(string message)
        {
            logBox.Dispatcher.BeginInvoke((Action)delegate
             {
                 logBox.ScrollToEnd();
                 logBox.AppendText($"[{DateTime.Now.ToString()}]");
                 logBox.AppendText("-----");
                 logBox.AppendText(message);
                 logBox.AppendText(Environment.NewLine);
             });
        }

        public void TaskInfoEvent(PosterRunInfo posterRunInfo)
        {
            Dispatcher.Invoke(() =>
            {
                lblTaskNumber.Content = posterRunInfo.TaskNumber;
                lblAccount.Content = posterRunInfo.Account;
                lblAgentId.Content = posterRunInfo.AgentId;
                lblTaskType.Content = posterRunInfo.TaskType;
                lblListingCount.Content = posterRunInfo.ListingCount;
                lblTaskItemId.Content = posterRunInfo.TaskItemId;
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !(IsStart == false && IsRun == false);
        }
    }
}
