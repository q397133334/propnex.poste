using CefSharp;
using CefSharp.Dom;
using CefSharp.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Propnex.Poster.Guru
{
    public partial class CefPoster : Form
    {
        public CefPoster()
        {
            InitializeComponent();
        }

        private async void CefPoster_Load(object sender, EventArgs e)
        {
            await chromiumWebBrowser1.LoadUrlAsync("www.baidu.com");
            CefPosterGuruAction action = new CefPosterGuruAction(chromiumWebBrowser1);
            var devToolsContext = await chromiumWebBrowser1.CreateDevToolsContextAsync();
            devToolsContext.DefaultTimeout = 6000 * 60;
            devToolsContext.DefaultNavigationTimeout = 1000 * 60;
            chromiumWebBrowser1.ShowDevTools();
            var six=await devToolsContext.EvaluateFunctionAsync<dynamic>("async () => await Promise.resolve(6)");
            var six1 = devToolsContext.EvaluateFunctionAsync<dynamic>("() => Promise.resolve(6)");
            var six2 = devToolsContext.EvaluateFunctionAsync<dynamic>("() => {return Promise.resolve(6);}");
            var cookies = await devToolsContext.EvaluateFunctionAsync<JArray>("()=> window.cookieStore.getAll()");
            var v = cookies.Where(q => q["name"].ToString() == "PSTM").FirstOrDefault();

            var v1 = v.Value<string>("value");
            //await action.Start();
            //Close();
        }
    }
}
