using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Propnex.Poster.WebServer.Entities;
using Flurl.Http;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using Propnex.Poster.PropertyGuru.Mobile.Model;
using System.Threading.Tasks;
using Propnex;

namespace Propnex.Poster.WebServer.BackgroundJobs
{
    public class GuruMissingIdResult
    {
        public string status { get; set; }

        public List<string> data { get; set; }
    }

    public class GetPropertyWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public GetPropertyWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 1000 * 60 * 60;//1 hour
        }

        [Volo.Abp.Uow.UnitOfWork(false)]
        protected async override Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var list = new List<string>();

            var GuruMissingId = await "https://pa-production.propnex.net/index.php/scrape/getGuruMissingId".GetJsonAsync<GuruMissingIdResult>();

            if (GuruMissingId.status != "ok")
            {
                //await SlackBotMessage.SendAsync($"GetProperty 0 <@U01DQLBLWNL>");
                return;
            }
            list = GuruMissingId.data;

            //var loginResult = await new Auth().LoginAsync(new AuthLogin()
            //{
            //    UserName = "davidytp@gmail.com",
            //    Password = "calista"
            //});
            Token token = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>((await PropnexPoster.WPF.WebServer.GetUser("")).TokenJson);
            //if (loginResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
            //{
            //    token = loginResult.Data;
            //}
            //else
            //{
            //    return;
            //}
            var jsonList = new List<string>();
            var listname = new List<string>();
            foreach (var item in list)
            {
                ProjectsApi api = new ProjectsApi(token);
                var projectResult = await api.GetProjectAsync(int.Parse(item));
                if (projectResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var project = projectResult.Data;
                    jsonList.Add(project.ToJson());
                    listname.Add(project.name);
                    //https://pa-production.propnex.net/index.php/scrape/guruProjects
                    //https://pa-staging.propnex.net/index.php/scrape/guruProjects
                    var ok = await $"https://pa-production.propnex.net/index.php/scrape/guruProjects".PostUrlEncodedAsync(new
                    {
                        id = item,
                        json = projectResult.Message
                    });

                    var msg = await ok.GetStringAsync();
                    Console.WriteLine(msg);
                }
            }
            await SlackBotMessage.SendAsync($"GetProperty {listname.JoinAsString(",")} <@U01DQLBLWNL><@U9WPWQYGH>");

        }
    }
}
