using CefSharp.WinForms;
using CefSharp;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using Flurl;

namespace Propnex.Poster.NetCoreWinForm
{
    internal static class Program
    {

        private static IAbpApplicationWithInternalServiceProvider _abpApplication;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {

#if ANYCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
#endif

            //var a= System.Web.HttpUtility.UrlEncode("The Elements @ Ampang");
           var b= Url.Encode("The Elements @ Ampang");

            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            };

            //Example of setting a command line argument
            //Enables WebRTC
            // - CEF Doesn't currently support permissions on a per browser basis see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access
            // - CEF Doesn't currently support displaying a UI for media access permissions
            //
            //NOTE: WebRTC Device Id's aren't persisted as they are in Chrome see https://bitbucket.org/chromiumembedded/cef/issues/2064/persist-webrtc-deviceids-across-restart
            settings.CefCommandLineArgs.Add("enable-media-stream");
            //https://peter.sh/experiments/chromium-command-line-switches/#use-fake-ui-for-media-stream
            settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            //For screen sharing add (see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access#comment-58677180)
            settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
            settings.CefCommandLineArgs.Add("--disable-web-security", "1");//关闭同源策略,允许跨域
            settings.CefCommandLineArgs.Add("--disable-site-isolation-trials", "1");//关闭站点隔离策略,允许跨域
            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            Application.EnableVisualStyles();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            //Application.Run(new IPropertyCefPoster());

            Serilog.Log.Logger = new LoggerConfiguration()
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
                Serilog.Log.Information("Starting WPF host.");

                _abpApplication = await AbpApplicationFactory.CreateAsync<NetCoreWinFormModule>(options =>
                {
                    options.UseAutofac();
                    options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
                });
                await _abpApplication.InitializeAsync();

                Application.Run(_abpApplication.Services.GetRequiredService<IPropertyMain>());

            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal(ex, "Host terminated unexpectedly!");
            }
        }
    }
}