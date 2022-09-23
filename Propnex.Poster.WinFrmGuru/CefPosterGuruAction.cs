using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Dom;
using CefSharp.WinForms;
using Newtonsoft.Json;
using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Tasks;

namespace Propnex.Poster.Guru
{
    public class CefPosterGuruAction : ICefPosterAction
    {
        ChromiumWebBrowser ChromiumWebBrowser { get; set; }
        DevToolsContext devToolsContext { get; set; }

        private PnTaskDto taskDto;

        private GuruTasks guruTasks;

        private string context = "";

        private string _token = "";
        public string token
        {
            get => getJwt();
        }

        private Random random;

        public CefPosterGuruAction(ChromiumWebBrowser chromiumWebBrowser)
        {
            ChromiumWebBrowser = chromiumWebBrowser;
            random = new Random(this.GetHashCode());
        }

        public async Task Start()
        {
            await get();
            await read();
            devToolsContext = await ChromiumWebBrowser.CreateDevToolsContextAsync();
            devToolsContext.DefaultTimeout = 6000 * 60;
            devToolsContext.DefaultNavigationTimeout = 1000 * 60;
            ChromiumWebBrowser.ShowDevTools();

            for (int i = 0; i < guruTasks.Tasks.Count; i++)
            {
                await Login(guruTasks.Tasks[i]);
                await GetLisints();
            }
        }

        private async Task Login(GuruTask guruTask)
        {
            try
            {
                await ChromiumWebBrowser.LoadUrlAsync("https://agentnet.propertyguru.com.sg/ex_logout");
                //ChromiumWebBrowser.Load("https://agentnet.propertyguru.com.sg/ex_logout");
                randoTime();
                await WatiForIsLoading();
            }
            catch (Exception ex)
            {

            }

            var loginUserId = await devToolsContext.QuerySelectorAsync<HtmlInputElement>("#login-userid");
            if (loginUserId == null)
            {
                System.Threading.Thread.Sleep(1000 * 10);
            }
            await loginUserId.ClickAsync();
            await loginUserId.SetValueAsync(guruTask.Account.Replace("\n", "").Replace("\r", ""));
            randoTime();
            var loginUserPwd = await devToolsContext.QuerySelectorAsync<HtmlInputElement>("#login-password");
            if (loginUserPwd != null)
            {
                await loginUserPwd.ClickAsync();
                await loginUserPwd.SetValueAsync(guruTask.Password.Replace("\n", "").Replace("\r", ""));
            }
            randoTime();
            var loginSubmit = await devToolsContext.QuerySelectorAsync<HtmlFormElement>("#login-form");
            if (loginSubmit != null)
            {
                await loginSubmit.SubmitAsync();
                await devToolsContext.WaitForNavigationAsync();

                if (devToolsContext.Url != "https://agentnet.propertyguru.com.sg/dash?" &&
                            devToolsContext.Url != "https://agentnet.propertyguru.com.sg/v2/dash")
                {

                }
            }

        }

        private Task WatiForIsLoading()
        {
            while (ChromiumWebBrowser.IsLoading)
            {
                randoTime(5000, 5000);
            }
            return Task.CompletedTask;
        }

        private async Task GetLisints()
        {

            await devToolsContext.GoToAsync("https://agentnet.propertyguru.com.sg/v2/listing_management");
    
            await WatiForIsLoading();
            var infos = new List<ListingInfo>();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await getListingsV2();
                    if (infos.Count > 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("GetListings error");
                }
            }
            async Task getListingsV2()
            {

                var func = @"()=>{
                           return fetch('https://bff-mobile.propertyguru.com/v1/listingManagement?region=sg&locale=en&status_code=ACT&sort=start_date&order=desc&page=1&limit=20000&timestamp=1616142255393',{ headers:{'authorization':'Bearer {" + token + "}'}})" +
                        "}";

                //var func = @"()=>{
                //           return fetch('https://bff-mobile.propertyguru.com/v1/listingManagement?region=sg&locale=en&status_code=ACT&sort=start_date&order=desc&page=1&limit=20000&timestamp=1616142255393')" +
                //        "}";

                var result = await devToolsContext.EvaluateFunctionAsync<object>(func);
                var jsonResult = JsonConvert.DeserializeObject<ListingsResult>(JsonConvert.SerializeObject(result));
                foreach (var item in jsonResult.listings)
                {
                    var info = new ListingInfo();
                    info.Id = item.id.Value;
                    info.Title = item.localizedTitle;
                    info.Score = item.qualityScore.ToString();
                    info.TypeCode = item.typeCode;
                    info.StatusCode = item.statusCode;
                    info.PropertyTypeCode = item.property.typeCode;
                    info.Prece = item.price.value.ToString();
                    info.StreetNumber = item.location.streetNumber;
                    info.StreetName = item.location.streetName1;
                    info.PostCode = item.location.postalCode;
                    if (item.products != null && item.products.Count > 0)
                    {
                        info.IsBoosted = item.products[0].productType == "boost-v2";
                    }
                    //turbo
                    if (item.products != null && item.products.Count > 0)
                    {
                        info.IsTurbo = item.products[0].productType == "turbo";
                    }
                    if (item.charges != null)
                    {
                        info.RepostCharge = item.charges.repost;
                    }
                    try
                    {
                        info.Sqft = Convert.ToInt32(item.sizes.floorArea[0].value).ToString();
                    }
                    catch
                    {
                        info.Sqft = Convert.ToInt32(item.sizes.landArea[0].value).ToString();
                    }
                    infos.Add(info);
                }
            }
        }

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
            guruTasks = new GuruTasks(context, taskContext);
        }

        private string getJwt()
        {
            if (_token == "")
            {
                var result = AjaxJsonGet<object>("https://agentnet.propertyguru.com.sg/sf2-agent/ajax/agent/jwt");
                var jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject<JwtResult>(JsonConvert.SerializeObject(result));
                _token = jsonResult.accessToken;
            }

            return _token;
        }

        private async Task<T> AjaxJsonGet<T>(string url, string data = "")
        {
            string jscode = $@"()=>{{ return $.ajax({{url:'{url}',async:false,type:'GET',contentType: 'application/json' ,data:{(data == "" ? "''" : "JSON.stringify(" + data.Replace('\"', '"') + ")")}}});}}";
           
            var result = await devToolsContext.EvaluateFunctionAsync<T>(jscode);
            return result;
        }

        private void randoTime(int min = 500, int max = 5000)
        {
            System.Threading.Thread.Sleep(random.Next(min, max));
        }
    }
}
