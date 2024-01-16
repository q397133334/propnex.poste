using Propnex.Poster.PropertyGuru.Mobile;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using Propnex.Poster.PropertyGuru.Mobile.Model;
using Flurl;
using Flurl.Http;
using System.Collections;
using System.Linq;

namespace Propnex.Poster.GetGuruPorperty
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var list = new List<string>();

            var GuruMissingId = await "https://pa-production.propnex.net/index.php/scrape/getGuruMissingId".GetJsonAsync<GuruMissingIdResult>();

            if (GuruMissingId.status != "ok")
            {
                return;
            }
            list = GuruMissingId.data;
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
            var jsonList = new List<string>();

            foreach (var item in list)
            {
                ProjectsApi api = new ProjectsApi(token);
                var projectResult = await api.GetProjectAsync(int.Parse(item));
                if (projectResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var project = projectResult.Data;
                    jsonList.Add(project.ToJson());

                    //https://pa-production.propnex.net/index.php/scrape/guruProjects
                    //https://pa-staging.propnex.net/index.php/scrape/guruProjects
                    var ok = await $"https://pa-production.propnex.net/index.php/scrape/guruProjects".PostUrlEncodedAsync(new
                    {
                        id = item,
                        json = project.ToJson()
                    });

                    var msg = await ok.GetStringAsync();
                    Console.WriteLine(msg);
                }
            }

            //await System.IO.File.WriteAllLinesAsync($"{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt", jsonList);
        }
    }

    public class GuruMissingIdResult
    {
        public string status { get; set; }

        public List<string> data { get; set; }
    }
}