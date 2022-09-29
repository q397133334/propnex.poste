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
using Newtonsoft.Json.Linq;
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
                var task = guruTasks.Tasks[i];
                await login(guruTasks.Tasks[i]);
                await getLisints();
                if (task.TaskType.ToLower() == "post only")
                {

                }
            }
        }

        #region task 任务功能

        private async Task postOnly(GuruTask guruTask)
        {
            for (var i = 0; i < guruTask.Listings.Listings.Count; i++)
            {
                var item = guruTask.Listings.Listings[i];
                await getAgentId(item);
            }
        }

        private async Task update(GuruTask guruTask) { }

        private async Task repost(GuruTask guruTask) { }

        private async Task remove(GuruTask guruTask) { }

        private async Task post(GuruTask guruTask) { }

        private async Task retrieve(GuruTask guruTask) { }

        /// <summary>
        /// 获取 listing
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task getLisints()
        {

            await devToolsContext.GoToAsync("https://agentnet.propertyguru.com.sg/v2/listing_management");

            await watiForIsLoading();
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

                var func = $@"()=>{{
                           return fetch('https://bff-mobile.propertyguru.com/v1/listingManagement?region=sg&locale=en&status_code=ACT&sort=start_date&order=desc&page=1&limit=20000&timestamp=1616142255393',
                        {{ headers:{{'authorization':'Bearer {await getJwt()}'}}}}).then(res=>{{
                                      return res.json()
                                }})}}";

                //var result1 = await devToolsContext.EvaluateFunctionAsync<TJson>(func);
                var result = await devToolsContext.EvaluateFunctionAsync<ListingsResult>(func);
                var jsonResult = result; // JsonConvert.DeserializeObject<ListingsResult>(JsonConvert.SerializeObject(result));
                if (jsonResult.listings == null)
                    return;
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

        private async Task getAgentId(GuruTaskListing guruTaskUpdateListing)
        {
            var agentId = await getCookie("PGID1");
            if (agentId != null && agentId != "")
                guruTaskUpdateListing.Listing.Agent.id = int.Parse(agentId);
        }

        /// <summary>
        /// 获取jwt
        /// </summary>
        /// <returns></returns>
        private async Task<string> getJwt()
        {
            if (_token == null || _token == "")
            {
                var result = await ajaxJsonGet<JwtResult>("https://agentnet.propertyguru.com.sg/sf2-agent/ajax/agent/jwt");
                // var jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject<JwtResult>(JsonConvert.SerializeObject(result));
                _token = result.accessToken;
            }

            return _token;
        }

        /// <summary>
        /// 登陆账号
        /// </summary>
        /// <param name="guruTask"></param>
        /// <returns></returns>
        private async Task login(GuruTask guruTask)
        {
            try
            {
                await ChromiumWebBrowser.LoadUrlAsync("https://agentnet.propertyguru.com.sg/ex_logout");
                randoTime();
                await watiForIsLoading();
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

        /// <summary>
        /// 解析xml
        /// </summary>
        /// <returns></returns>
        private Task read()
        {
            var lenght = context.IndexOf("Xpressor-Listing-File===");
            var taskContext = context.Substring(0, lenght == -1 ? context.Length : lenght);
            guruTasks = new GuruTasks(context, taskContext);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取xml
        /// </summary>
        /// <returns></returns>
        private async Task get()
        {
            taskDto = await Api.WebServer.GetTask();
            if (taskDto != null)
            {
                context = await Api.WebServer.GetTaskContent(taskDto);
            }
        }
        #endregion


        #region task 操作功能

        private async Task<CreateOrUpdateListingResult> createListing(GuruTaskListing guruTaskUpdateListing)
        {
            var jsonFomrate = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            await getAgentId(guruTaskUpdateListing);

            var listing = new CreateOrUpdateListing();
            listing.Create(guruTaskUpdateListing.Listing);
            var json = JsonConvert.SerializeObject(listing, jsonFomrate);

            var result = await AjaxJsonPost<object>("https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings", "", data: json);
            if (result.GetType().Name == "String")
            {
                var r = new CreateOrUpdateListingResult()
                {
                    Id = 0,
                    errors = result.ToString()
                };
                if (r.errors.ToString().ToLower().Contains("headline"))
                {
                    listing.localizedHeadline = "Call now to enquire";
                    listing.headlines.En = "Call now to enquire";

                    json = JsonConvert.SerializeObject(listing, jsonFomrate);
                    result =await AjaxJsonPost<object>("https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings", "", data: json);
                }
                else
                {
                    return r;
                }
            }
            if (result.GetType().Name == "String")
            {
                var r = new CreateOrUpdateListingResult()
                {
                    Id = 0,
                    errors = result.ToString()
                };
                return r;
            }

            return JsonConvert.DeserializeObject<CreateOrUpdateListingResult>(JsonConvert.SerializeObject(result));
        }

        #endregion

        #region 辅助功能

        /// <summary>
        /// 获取json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task<T> ajaxJsonGet<T>(string url, string data = "")
        {
            try
            {
                string jscode = $@"()=> {{return fetch(""{url}"", {{
                                  ""headers"": {{
                                    ""accept"": ""application/json, text/plain, */*""
                                  }},
                                  ""method"": ""GET"",
                                  ""mode"": ""cors""
                                }}).then(res=>{{
                                      return res.json()
                                }})}}";

                var result = await devToolsContext.EvaluateFunctionAsync<T>(jscode);

                return result;
            }
            catch (Exception ex)
            {
                return default(T);
            }

        }

        /// <summary>
        /// 获取post 数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="referrerUrl"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task<T> AjaxJsonPost<T>(string url, string referrerUrl, string type = "POST", string data = "")
        {
            string jscode = $"()=> fetch('{url}',{{ method:\"{type}\",referrer:'{referrerUrl}',headers:{{'authorization':'Bearer {await getJwt()}','content-type': 'application/json;charset=UTF-8'}},body:{(data == "" ? "''" : "JSON.stringify(" + data.Replace('\"', '"') + ")")}}}) .then(response => response.json());}}";
            object result;
            try
            {
                result = await devToolsContext.EvaluateFunctionAsync<T>(jscode);
            }
            catch (Exception ex)
            {
                result = default(T);
            }
            return (T)result;
        }

        /// <summary>
        ///  等待页面加载完成
        /// </summary>
        /// <returns></returns>
        private Task watiForIsLoading()
        {
            while (ChromiumWebBrowser.IsLoading)
            {
                randoTime(5000, 5000);
            }
            return Task.CompletedTask;
        }


        private async Task<string> getCookie(string nameKey)
        {
            var cookies = await devToolsContext.EvaluateFunctionAsync<JArray>("()=> window.cookieStore.getAll()");
            var cookie = cookies.Where(q => q["name"].ToString() == nameKey).FirstOrDefault();
            if (cookie != null)
            {
                return cookie.Value<string>("value");
            }
            return "";
        }
        /// <summary>
        /// 随机延迟
        /// </summary>
        /// <param name="min">最小时间</param>
        /// <param name="max">最大时间</param>
        private void randoTime(int min = 500, int max = 5000)
        {
            System.Threading.Thread.Sleep(random.Next(min, max));
        }
        #endregion

    }
}
