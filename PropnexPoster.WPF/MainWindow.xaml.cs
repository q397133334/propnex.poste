using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Threading;


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
            Title += $" Version {Assembly.GetEntryAssembly().GetFileVersion()}";
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

        private async void ThreadPoster()
        {
            while (IsStart)
            {
                IsRun = true;
                var run = _serviceProvider.GetService<PosterRun>();//  new PosterRun();
                run.MessageEvent = Log;
                run.TaskInfoEvent = TaskInfoEvent;
                await run.Run();
                run = null;
                IsRun = false;
                await Task.Delay(1000);
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Close();
            }));
        }

        private void Log(string message, bool isRefresh = false)
        {
            //logBox.Dispatcher.BeginInvoke((Action)delegate
            // {
            //     logBox.AppendText($"[{DateTime.Now}]");
            //     logBox.AppendText(message);
            //     logBox.AppendText(Environment.NewLine);
            //     logBox.ScrollToEnd();
            //     if (logBox.Text.Length > 100000)
            //     {
            //         logBox.Text = logBox.Text.Substring(0, logBox.Text.Length - 50000);
            //     }
            // });
            if (isRefresh==false)
            {
                AppendLineAndScroll(logBox,$"[{DateTime.Now}]{message}");
            }
            else
            {
                RefreshLastLine(logBox,$"[{DateTime.Now}]{message}");
            }
        }

        /// <summary>
        /// 在 TextBox 末尾追加一行并滚动到最后（在非 UI 线程调用也安全）。
        /// </summary>
        public static void AppendLineAndScroll(TextBox tb, string newLine, int maxLines = 5000)
        {
            if (tb == null) throw new ArgumentNullException(nameof(tb));
            if (maxLines < 1) maxLines = maxLines;

            bool result = false;
            tb.Dispatcher.Invoke(() =>
            {
                if (tb.Text.Length > 0 && !tb.Text.EndsWith("\n")) tb.AppendText(Environment.NewLine);
                tb.AppendText(newLine);

                // 如果超出行数，删除开头多余行
                if (tb.LineCount > maxLines)
                {
                    try
                    {
                        int excess = tb.LineCount - maxLines;
                        int removeToIndex = tb.GetCharacterIndexFromLineIndex(excess);
                        if (removeToIndex > 0)
                        {
                            tb.Text = tb.Text.Substring(removeToIndex);
                        }
                    }
                    catch
                    {
                        // 若出现异常（极少），不抛出以免影响 UI 更新
                    }
                }

                tb.CaretIndex = tb.Text.Length;
                tb.ScrollToEnd();
            }, DispatcherPriority.Background);
            //return result;
        }

        /// <summary>
        /// 用 newLine 替换最后一行文本并保持滚动到该行（在非 UI 线程调用也安全）。
        /// 返回是否成功（当 TextBox 没有行时返回 false）。
        /// </summary>
        public static bool RefreshLastLine(TextBox tb, string newLine)
        {
            if (tb == null) throw new ArgumentNullException(nameof(tb));
            bool result = false;
            tb.Dispatcher.Invoke(() =>
            {
                if (tb.LineCount == 0)
                {
                    // 无内容则直接写入
                    tb.Text = newLine ?? string.Empty;
                    tb.CaretIndex = tb.Text.Length;
                    tb.ScrollToEnd();
                    result = true;
                    return;
                }

                int lastLineIndex = tb.LineCount - 1;
                int start = tb.GetCharacterIndexFromLineIndex(lastLineIndex);
                int end;
                // 如果最后一行是最后一个字符，end = Text.Length
                if (lastLineIndex == tb.LineCount - 1)
                    end = tb.Text.Length;
                else
                    end = tb.GetCharacterIndexFromLineIndex(lastLineIndex + 1);

                // 替换区间 [start, end)
                if (start < 0) { result = false; return; }
                var text = tb.Text ?? string.Empty;
                var newText = text.Remove(start, Math.Max(0, end - start)).Insert(start, newLine ?? string.Empty);
                tb.Text = newText;

                // 将光标移动到最后并滚动到该行
                tb.CaretIndex = tb.Text.Length;
                tb.ScrollToLine(lastLineIndex);
                result = true;
            }, DispatcherPriority.Background);
            return result;
        
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
