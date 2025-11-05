using Autofac.Features.OwnedInstances;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.Share;
using RestSharp.Authenticators;
using RestSharp;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using System.IO;
using Propnex;
using System.Net;
using System.Net.Security;

namespace PropnexPoster.Console;

[DependsOn(typeof(AbpAutofacModule))]
public class ConsoleModule : AbpModule
{
    public static AppConfiguration AppConfiguration { get; set; }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //context.Services.AddTransient<PosterRun>();
        context.Services.AddSingleton<Propnex.Poster.Share.AppConfiguration>();
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var configuration = context.ServiceProvider.GetService<IConfiguration>();
        AppConfiguration = context.ServiceProvider.GetService<AppConfiguration>();
        WebServer.BaseUrl = configuration["BaseUrl"];
        WebServer.MachindNumber = (await WebServer.GetMachineIdAsync(configuration["MachineNumber"])).Trim('\"');
        AppConfiguration = configuration.Get<AppConfiguration>();
    }
    public override async Task OnPreApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        //AutoUpdater.Start("http://testposter.propnex.net/PropnexPoster.Guru/PropnexPoster.Console.AutoUpdater.xml");

        //var result = await PosterResultUpload.XWebItem(new XWebItemDto()
        //{
        //    account_name = "emilyyep@ymail.com",
        //    account_password = "fqM&w8#U",
        //    task_id = "1224418",
        //    taskitem_id = "5824283",
        //    status = "Done",
        //    time_cost = "0",
        //    taskitem_note = "",
        //    listing_version=DateTime.Now.ToString(""),
        //    portal_link = "https://www.propertyguru.com.sg/listing/25629840"
        //});

        //var result1 = await PosterResultUpload.XWebEnd(new XWebEndDto()
        //{
        //    account_name = "emilyyep@ymail.com",
        //    account_password = "fqM&w8#U",
        //    task_id = "1224418",
        //    status = "Done",
        //    time_cost = "0",
        //    note=""
        //});


        await Task.CompletedTask;
    }

    public RestRequest GetRequest(Method method, string resource)
    {
        RestRequest request = new RestRequest();
        //request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
        //request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
        request.AddOrUpdateHeader(KnownHeaders.UserAgent, "sg;agentnet;android;2023.12.6;LIO-AN00;null");
        request.Method = method;
        request.Timeout = new TimeSpan(0, 5, 0);
        request.Resource = resource;
        return request;
    }

    public async Task<HttpResult<string>> Test()
    {
        var request = GetRequest(Method.Post, "/sf2-agent/ajax/listings/24876387/media");
        //request.Authenticator = new JwtAuthenticator(Token.accessToken);
        request.AddParameter("ownerId", 24876387);
        request.AddParameter("mediaClass", "UPHO");
        request.AddParameter("mediaType", "IMAGE");
        request.AddParameter("userId", $"555386");
        request.AddParameter("source", "AgentNet");
        request.AddParameter("sortOrder", 1);
        request.AddParameter("caption", "");
        request.AddParameter("statusCode", "ACT");
        var filePath = "C:\\Users\\worker_fg\\Documents\\雷电模拟器\\Pictures\\3_image.jpg";
        var filePathLower = filePath.ToLower();
        if (filePathLower.Contains("youtube") ||
            filePathLower.Contains("youtu.be") ||
            filePathLower.Contains("vimeo") ||
                filePathLower.Contains("dailymotion") ||
                filePathLower.Contains("<iframe") ||
                filePathLower.Contains("havelock2") ||
                filePathLower.Contains("new-vr")
                )
        {
            if (filePath.Contains("#"))
            {
                filePath = filePath.Split('#')[0];
            }
            request.AddParameter("videoEmbedHtml", filePath);
            request.AlwaysMultipartFormData = true;
        }
        else
        {
            if (filePath == "")
            {
                return new HttpResult<string>() { };
            }
            if (File.Exists(filePath) == false)
            {
                return new HttpResult<string>() { };
            }
            var files = File.ReadAllBytes(filePath);
            var fileName = Path.GetExtension(filePath);
            request.AddFile("mediaFile", files, $"{Guid.NewGuid()}{filePath}");
        }

        using (var c = new RestSharp.RestClient(new RestClientOptions()
        {
            BaseUrl = new Uri("https://agentnet.propertyguru.com.sg"),
            MaxTimeout = 1000 * 60 * 10,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36"
        }))
        {
            request.AlwaysMultipartFormData = true;

            request.AddHeader("origin", "https://agentnet.propertyguru.com.sg");
            request.AddHeader("referer", "https://agentnet.propertyguru.com.sg/v2/create-listing/media/24876387");
            request.AddHeader("cookie", "PG_U=klennkoh%40gmail.com; PG_T=aHJZFrhWmSNvVd9fsfVbKdJgkh5BYRJ7; loggedIn=true; PHPSESSID2=f99646a0b9767c2ab8928f10afbb31c7; __cf_bm=TgM1jMTzD1f7vYsVytsbb0JkVcY3sYz_YZDE54QezJ4-1703207396-1-AWM+3q0UtLxzlYNAjOTHHlxJYHrdLHDhr/wt2Syx240KPvVroLT5oUvptNkBSPFYtQhyWJWyxP2ZbZMJWRaiHD253Kd7dLnw1ofjAFUgq2qo; PGURU_REMEMBERME_ID=555386; PGID1=555386; PGID2=klennkoh%40gmail.com; sixpack_client_id=27EC477C-9A9A-E5F9-F6E5-6F628FBF240F");
            var response = await c.ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new HttpResult<string>()
                {
                    Data = response.Content,
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                };
            }
            return null;
        }
    }
}
