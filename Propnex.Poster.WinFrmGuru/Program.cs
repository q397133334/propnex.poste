using Abp;
using Abp.Castle.Logging.Log4Net;
using Castle.Core.Logging;
using Castle.Facilities.Logging;
using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Propnex.Poster.Guru
{
    internal static class Program
    {

        public static AbpBootstrapper Bootstrapper { get; set; }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ApplicationExit += Application_ApplicationExit;

            Bootstrapper = AbpBootstrapper.Create(typeof(PronpnexPosterWinFrmGuruModule));
            Bootstrapper.IocManager.IocContainer.AddFacility<LoggingFacility>(
                  f => f.UseAbpLog4Net().WithConfig(Environment.CurrentDirectory + "\\log4net.config")
              );
            Bootstrapper.Initialize();
            Application.Run(Bootstrapper.IocManager.Resolve<Main>());
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            Bootstrapper.IocManager.Resolve<ILogger>().Info("ApplicationExit");
            Bootstrapper.Dispose();
            Application.ExitThread();
            Environment.Exit(0);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Bootstrapper.IocManager.Resolve<ILogger>().Error((e.ExceptionObject as Exception).Message);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Bootstrapper.IocManager.Resolve<ILogger>().Error(e.Exception.Message);
        }
    }
}
