using Microsoft.Extensions.Configuration;
using System;
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
        public MainWindow(IServiceProvider serviceProvider ,IConfiguration  configuration)
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
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
                var run = new PosterRun();
                run.MessageEvent = Log;
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
                 logBox.AppendText($"[{DateTime.Now.ToString()}]");
                 logBox.AppendText("-----");
                 logBox.AppendText(message);
                 logBox.AppendText(Environment.NewLine);
                 logBox.ScrollToEnd();
             });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !(IsStart == false && IsRun == false);
        }
    }
}
