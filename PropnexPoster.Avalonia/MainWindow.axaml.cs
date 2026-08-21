using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Controls;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PropnexPoster.Avalonia;

public partial class MainWindow : SukiWindow
{
    private const int MaxLogLines = 5000;

    private readonly IServiceProvider _serviceProvider;

    public PosterRunInfo RunInfo { get; } = new PosterRunInfo();

    public bool IsStart { get; private set; }
    public bool IsRun { get; private set; }

    public MainWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();

        DataContext = this;
        Title += $" Version {Assembly.GetEntryAssembly()?.GetName().Version}";
        Closing += MainWindow_Closing;
    }

    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
        IsStart = true;
        var thread = new Thread(ThreadPoster) { IsBackground = true };
        thread.Start();
        btnStart.IsEnabled = false;
        btnStop.IsEnabled = true;
    }

    private void btnStop_Click(object sender, RoutedEventArgs e)
    {
        IsStart = false;
    }

    private async void ThreadPoster()
    {
        while (IsStart)
        {
            IsRun = true;
            var run = _serviceProvider.GetService<PosterRun>();
            run.MessageEvent = Log;
            run.PosterRunInfo = RunInfo;
            await run.Run();
            run = null;
            IsRun = false;
            await Task.Delay(1000);
        }
        Dispatcher.UIThread.Post(Close);
    }

    private void Log(string message, bool isRefresh = false)
    {
        var line = $"[{DateTime.Now}]{message}";
        Dispatcher.UIThread.Post(() =>
        {
            var text = logBox.Text ?? "";
            var lines = text.Length == 0 ? Array.Empty<string>() : text.TrimEnd('\n').Split('\n');

            if (isRefresh && lines.Length > 0)
            {
                lines[^1] = line;
            }
            else
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[^1] = line;
            }

            if (lines.Length > MaxLogLines)
            {
                lines = lines[(lines.Length - MaxLogLines)..];
            }

            logBox.Text = string.Join(Environment.NewLine, lines) + Environment.NewLine;
            logBox.CaretIndex = logBox.Text.Length;
        });
    }

    private void MainWindow_Closing(object sender, WindowClosingEventArgs e)
    {
        e.Cancel = !(IsStart == false && IsRun == false);
    }
}
