using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.WinForms;
using Propnex.Poster.Dtos;

namespace Propnex.Poster.Guru
{
    public class CefPosterGuruAction : ICefPosterAction
    {
        ChromiumWebBrowser ChromiumWebBrowser { get; set; }

        public CefPosterGuruAction(ChromiumWebBrowser chromiumWebBrowser)
        {
            ChromiumWebBrowser = chromiumWebBrowser;
        }

        public async Task Start()
        {
            await get();
            await read();
        }

        private PnTaskDto taskDto;

        private PropertyGuru.Tasks.GuruTasks

        private string context = "";

        private async Task get()
        {
            taskDto = await Api.WebServer.GetTask();
            if (taskDto != null)
            {
                context = await Api.WebServer.GetTaskContent(taskDto);
            }
        }

        private async Task read()
        {
            var lenght = context.IndexOf("Xpressor-Listing-File===");
            var taskContext = context.Substring(0, lenght == -1 ? context.Length : lenght);


        }
    }
}
