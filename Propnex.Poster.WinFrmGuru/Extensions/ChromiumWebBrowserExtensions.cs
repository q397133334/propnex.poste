using CefSharp.Dom;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.Guru.Extensions
{
    public static class ChromiumWebBrowserExtensions
    {
        public static async Task<T> AjaxJsonPost<T>(this ChromiumWebBrowser chromiumWebBrowser, string url, string referrerUrl, string type = "POST", string header = "", string data = "")
        {
            var devTools = await chromiumWebBrowser.CreateDevToolsContextAsync();
            string jscode = $@"()=> fetch('{url}',{{ method:'{type}',
                                                    referrer:'{referrerUrl}',
                                                    headers:'{header}',
                                                    'content-type': 'application/json;charset=UTF-8'}},
                                                    body:{(data == "" ? "''" : "JSON.stringify(" + data.Replace('\"', '"') + ")")}
                                                   }}).then(response => response.json())";
            T result;
            try
            {
                result = await devTools.EvaluateFunctionAsync<T>(jscode);
            }
            catch (Exception ex)
            {
                result = default(T);
            }
            return (T)result;
        }
    }
}
