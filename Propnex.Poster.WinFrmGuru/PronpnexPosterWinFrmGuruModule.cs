using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Modules;
using Abp.Castle.Logging.Log4Net;
using System.Windows.Forms;
using Abp.Reflection.Extensions;
using CefSharp.WinForms;
using CefSharp;
using System.IO;
using System.Reflection;

namespace Propnex.Poster.Guru
{
    [DependsOn(
        typeof(AbpCastleLog4NetModule)
        )]
    public class PronpnexPosterWinFrmGuruModule : AbpModule
    {

        public static string StartupPath = "StartupPath";
        public static string ConfigPath = "ConfigPath";

        public override void PreInitialize()
        {
            Configuration.Set("StartupPath", Application.StartupPath);
            Configuration.Set("ConfigPath", Directory.GetDirectoryRoot(Application.StartupPath));
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }

        public override void PostInitialize()
        {
            if(Directory.Exists(Configuration.Get<string>(ConfigPath))==false)
            {
                Directory.CreateDirectory(Configuration.Get<string>(ConfigPath));
            }
            cefInitialize();
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }


        private void cefInitialize()
        {
#if ANYCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
#endif
            // Programmatically enable DPI Aweness
            // Can also be done via app.manifest or app.config
            // https://github.com/cefsharp/CefSharp/wiki/General-Usage#high-dpi-displayssupport
            // If set via app.manifest this call will have no effect.
            Cef.EnableHighDPISupport();

            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Configuration.Get<string>(PronpnexPosterWinFrmGuruModule.ConfigPath), "CefSharp\\Cache")
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

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }
    }
}
