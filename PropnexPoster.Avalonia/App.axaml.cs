using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System;
using Volo.Abp;

namespace PropnexPoster.Avalonia;

public partial class App : Application
{
    private IAbpApplicationWithInternalServiceProvider _abpApplication;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 在 Abp 异步初始化完成之前先阻止生命周期因为没有窗口而退出。
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .CreateLogger();

            try
            {
                Log.Information("Starting Avalonia host.");

                _abpApplication = await AbpApplicationFactory.CreateAsync<AvaloniaModule>(options =>
                {
                    options.UseAutofac();
                    options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
                });
                await _abpApplication.InitializeAsync();

                var mainWindow = _abpApplication.Services.GetRequiredService<MainWindow>();
                desktop.MainWindow = mainWindow;
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

                desktop.Exit += async (_, _) =>
                {
                    await _abpApplication.ShutdownAsync();
                    Log.CloseAndFlush();
                };
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
                desktop.Shutdown(-1);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
