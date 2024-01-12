using Propnex.Poster.PropertyGuru.Mobile;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using Propnex.Poster.PropertyGuru.Mobile.Model;
using Flurl;
using Flurl.Http;

namespace Propnex.Poster.GetGuruPorperty
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var loginResult = await new Auth().LoginAsync(new AuthLogin()
            {
                UserName = "davidytp@gmail.com",
                Password = "calista"
            });
            Token token;
            if (loginResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                token = loginResult.Data;
            }
            else
            {
                return;
            }

            var list = new List<string>()
            {
                "25272",
                "25297",
                "25343",
                "24662",
                "25306",
                "25429",
                "25305",
                "25339",
                "25395",
                "24699",
                "25466",
                "25403",
                "25495",
                "25565",
                "25431",
                "25432",
                "25393",
                "25462",
                "25464",
                "25463",
                "25289",
                "25299",
                "25436",
                "25433",
                "25373",
                "25374",
                "25478",
                "25406",
                "25309",
                "25310",
                "25286",
                "25634",
                "25516",
                "25497",
                "25517",
                "25503",
                "25639",
                "25600",
                "25636",
                "25413",
                "25643",
                "25447",
                "25555"
            };


            var jsonList = new List<string>();

            foreach (var item in list)
            {
                ProjectsApi api = new ProjectsApi(token);
                var projectResult = await api.GetProjectAsync(int.Parse(item));
                if (projectResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var project = projectResult.Data;
                    jsonList.Add(project.ToJson());

                    var ok = await $"https://pa-staging.propnex.net/index.php/scrape/guruProjects".PostUrlEncodedAsync(new { 
                        id=item,
                        json=System.Web.HttpUtility.UrlEncode(project.ToJson())
                });

                   var msg=await ok.GetStringAsync();
                   Console.WriteLine(msg);
                }
            }

            //await System.IO.File.WriteAllLinesAsync($"{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt", jsonList);
        }
    }
}