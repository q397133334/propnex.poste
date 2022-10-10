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
            //var six=await devToolsContext.EvaluateFunctionAsync<dynamic>("async () => await Promise.resolve(6)");
            //var six1 = devToolsContext.EvaluateFunctionAsync<dynamic>("() => Promise.resolve(6)");
            //var six2 = devToolsContext.EvaluateFunctionAsync<dynamic>("() => {return Promise.resolve(6);}");
            //var cookies = await devToolsContext.EvaluateFunctionAsync<JArray>("()=> window.cookieStore.getAll()");
            //var v = cookies.Where(q => q["name"].ToString() == "PSTM").FirstOrDefault();

            //var v1 = v.Value<string>("value");

            var formData = new Dictionary<string, string>();
            formData.Add("ownerId", "0");
            formData.Add("mediaType", "'IMAGE'");// Videos=MOVIE ,Virtual Tours=VTOUR,Floorplan=IMAGE
            formData.Add("mediaClass", "'UPHO'");// Videos=UMOV,Virtual Tours=UTOUR,Floorplan=UFLOO
            formData.Add("source", "' AgentNet'");
            formData.Add("userId", $"'0'");
            formData.Add("caption", "''");
            formData.Add("language", "'en'");
            formData.Add("sortOrder", $"1");
            await devToolsContext.EvaluateExpressionAsync($"var file=window.document.createElement('input');file.type='file';file.id='file_1_img';document.body.appendChild(file)");

            var file=await devToolsContext.QuerySelectorAsync<CefSharp.Dom.HtmlInputElement>("#file_1_img");
        

            await file.SetValueAsync("C:\\Users\\worker_fg\\Desktop\\微信截图_20220929154433.png");
            formData.Add("mediaFile", $"document.getElementById('file_1_img').files[0]");
            return;
            try
            {
                await action.Start();
            }
            catch(Exception ex)
            {

            }

            Close();
        }
    }
}
