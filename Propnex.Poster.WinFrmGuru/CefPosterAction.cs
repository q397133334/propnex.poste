using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.WinForms;

namespace Propnex.Poster.Guru
{
    public class CefPosterAction
    {
        ChromiumWebBrowser ChromiumWebBrowser { get; set; }

        public CefPosterAction(ChromiumWebBrowser chromiumWebBrowser)
        {
            ChromiumWebBrowser = chromiumWebBrowser;
        }

        public async Task Start()
        {
            var task = await Api.WebServer.GetTask();
            if (task != null)
            {

            }
        }
    }
}
