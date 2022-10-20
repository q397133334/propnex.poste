using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Dom;
using CefSharp.WinForms;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru;
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
            await getDevToolsContext();
            ChromiumWebBrowser.ShowDevTools();

            for (int i = 0; i < guruTasks.Tasks.Count; i++)
            {
                var task = guruTasks.Tasks[i];
                await login(guruTasks.Tasks[i]);
                await getLisints();
                if (task.TaskType.ToLower() == "post only")
                {
                    await postOnlyAsync(task);
                }
                if (task.TaskType.ToLower() == "repost")
                {
                    await repost(task);
                }
            }
        }

        #region task 任务功能

        private async Task postOnlyAsync(GuruTask guruTask)
        {
            for (var i = 0; i < guruTask.Listings.Listings.Count; i++)
            {
                var item = guruTask.Listings.Listings[i];
                var listing = IsExtis(item, guruTask);
                if (listing == null)
                {
                    var result = await createListingAsync(item);

                    if (result.Id == 0)
                    {
                        if (result.errors.ToString().ToLower().Contains("headline"))
                        {
                            item.Listing.LocalizedHeadline = DefaultTitles.GetTitle();
                            item.Listing.Headlines.En = item.Listing.LocalizedHeadline;
                        }
                        result = await createListingAsync(item);
                    }
                    item.Listing.Id = result.Id;
                    if (result.Id != 0)
                    {
                        await uploadPhotosAsync(item);
                        await uploadVideos(item);
                        await uploadVirtualTours(item);
                        await uploadFlooplan(item);
                    }
                }
            }
        }

        private async Task update(GuruTask guruTask)
        {
            for (var i = 0; i < guruTask.Listings.Listings.Count; i++)
            {
                var item = guruTask.Listings.Listings[i];
                try
                {
                    //get adcredits 

                    await getAgentId(item);
                    await uploadPhotosAsync(item);
                    await uploadVideos(item);
                    await uploadVirtualTours(item);
                    await uploadFlooplan(item);
                }
                catch
                {

                }
            }
        }

        private async Task repost(GuruTask guruTask)
        {
            for (int i = 0; i < guruTask.Listings.Listings.Count; i++)
            {
                var item = guruTask.Listings.Listings[i];
                if (IsExtis(item, guruTask) != null)
                {
                    try
                    {
                        if (item.FastRepost == "0")
                        {
                            await getAgentId(item);
                            await update(guruTask);
                        }
                        else
                        {
                            await repost(guruTask);
                        }

                    }
                    catch
                    {

                    }
                }
                else
                {
                    await createListingAsync(item);
                }

            }
            await Task.CompletedTask;
        }

        private async Task remove(GuruTask guruTask) { }

        private async Task post(GuruTask guruTask) { }

        private async Task retrieve(GuruTask guruTask) { }


        List<ListingInfo> ListingInfos = null;
        private ListingInfo IsExtis(GuruTaskListing guruTaskListing, GuruTask guruTask)
        {
            ListingInfo listingInfo = null;
            if (guruTaskListing.Listing.Id.HasValue)
            {
                listingInfo = ListingInfos.Where(q => q.Id == guruTaskListing.Listing.Id).FirstOrDefault();
            }

            if (listingInfo == null)
            {
                if (guruTask.Source.ToLower() == "chope")
                {
                    listingInfo = ListingInfos.Where(q => q.Sqft == guruTaskListing.Listing.Sizes.floorArea[0].text.Trim()
                                                 && q.Title == guruTaskListing.Listing.Property.name && q.TypeCode == guruTaskListing.Listing.TypeCode
                                                 && q.Prece == guruTaskListing.Listing.Price.value.ToString()
                                                 && q.StreetName == guruTaskListing.Listing.Location.streetName1
                                                 && q.StreetNumber == guruTaskListing.Listing.Location.streetNumber
                                                 && q.PostCode == guruTaskListing.Listing.Location.postalCode
                                                 ).FirstOrDefault();
                    if (listingInfo == null)
                    {
                        if (guruTaskListing.Listing.TypeCode.ToUpper() == "ROOM")
                        {
                            listingInfo = ListingInfos.Where(q => q.Sqft == guruTaskListing.Listing.Sizes.floorArea[0].text.Trim()
                                                    && q.Title == guruTaskListing.Listing.Property.name && q.TypeCode == "RENT"
                                                    && q.Prece == guruTaskListing.Listing.Price.value.ToString()
                                                    && q.StreetName == guruTaskListing.Listing.Location.streetName1
                                                    && q.StreetNumber == guruTaskListing.Listing.Location.streetNumber
                                                    && q.PostCode == guruTaskListing.Listing.Location.postalCode
                                                    ).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    listingInfo = ListingInfos.Where(q => q.Sqft == guruTaskListing.Listing.Sizes.floorArea[0].text.Trim()
                                                 && q.Title == guruTaskListing.Listing.Property.name && q.TypeCode == guruTaskListing.Listing.TypeCode
                                                 && q.StreetName == guruTaskListing.Listing.Location.streetName1
                                                    && q.StreetNumber == guruTaskListing.Listing.Location.streetNumber
                                                    && q.PostCode == guruTaskListing.Listing.Location.postalCode
                                                 ).FirstOrDefault();
                    if (listingInfo == null)
                    {
                        if (guruTaskListing.Listing.TypeCode.ToUpper() == "ROOM")
                        {
                            listingInfo = ListingInfos.Where(q => q.Sqft == guruTaskListing.Listing.Sizes.floorArea[0].text.Trim()
                                                 && q.Title == guruTaskListing.Listing.Property.name && q.TypeCode == "RENT"
                                                 && q.StreetName == guruTaskListing.Listing.Location.streetName1
                                                    && q.StreetNumber == guruTaskListing.Listing.Location.streetNumber
                                                    && q.PostCode == guruTaskListing.Listing.Location.postalCode).FirstOrDefault();
                        }
                    }


                }
            }
            if (listingInfo != null)
            {
                guruTaskListing.Listing.Id = listingInfo.Id;
            }
            return listingInfo;
        }

        /// <summary>
        /// 获取 listing
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task getLisints()
        {
            await getDevToolsContext();
            await devToolsContext.GoToAsync("https://agentnet.propertyguru.com.sg/v2/listing_management");
            await randoTime();
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
                ListingInfos = infos;
            }
        }

        private async Task getAgentId(GuruTaskListing guruTaskUpdateListing)
        {
            // var agentId = await getCookie("PGID1");
            await getDevToolsContext();
            var agentId = await devToolsContext.EvaluateFunctionAsync<int>("()=> guruApp.user_id");
            if (agentId != 0)
                guruTaskUpdateListing.Listing.Agent.id = agentId;
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
            await getDevToolsContext();
            try
            {
                await ChromiumWebBrowser.LoadUrlAsync("https://agentnet.propertyguru.com.sg/ex_logout");
                await randoTime();
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
            await randoTime();
            var loginUserPwd = await devToolsContext.QuerySelectorAsync<HtmlInputElement>("#login-password");
            if (loginUserPwd != null)
            {
                await loginUserPwd.ClickAsync();
                await loginUserPwd.SetValueAsync(guruTask.Password.Replace("\n", "").Replace("\r", ""));
            }
            await randoTime();
            var loginSubmit = await devToolsContext.QuerySelectorAsync<HtmlFormElement>("#login-form");
            if (loginSubmit != null)
            {
                await loginSubmit.SubmitAsync();
                await randoTime();
                await watiForIsLoading();

                if (devToolsContext.Url != "https://agentnet.propertyguru.com.sg/dash?" &&
                            devToolsContext.Url != "https://agentnet.propertyguru.com.sg/v2/dash")
                {

                }
                else
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

        private async Task getDevToolsContext()
        {
            devToolsContext = await ChromiumWebBrowser.CreateDevToolsContextAsync();
            devToolsContext.DefaultTimeout = 6000 * 60;
            devToolsContext.DefaultNavigationTimeout = 1000 * 60;
        }

        private async Task<CreateOrUpdateListingResult> createListingAsync(GuruTaskListing guruTaskUpdateListing)
        {
            await getAgentId(guruTaskUpdateListing);
            var jsonFomrate = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            var createOrUpdateListing = new CreateOrUpdateListing();
            createOrUpdateListing.Create(guruTaskUpdateListing.Listing);
            var json = JsonConvert.SerializeObject(createOrUpdateListing, jsonFomrate);
            var ajaxResult = await AjaxJsonPost<object>("https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings", "", data: json);
            if (ajaxResult.GetType().Name == "String")
            {
                var r = new CreateOrUpdateListingResult()
                {
                    Id = 0,
                    errors = ajaxResult.ToString()
                };
                if (r.errors.ToString().ToLower().Contains("headline"))
                {
                    createOrUpdateListing.localizedHeadline = "Call now to enquire";
                    createOrUpdateListing.headlines.En = "Call now to enquire";

                    json = JsonConvert.SerializeObject(createOrUpdateListing, jsonFomrate);
                    ajaxResult = await AjaxJsonPost<object>("https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings", "", data: json);
                }
            }
            if (ajaxResult.GetType().Name == "String")
            {
                var r = new CreateOrUpdateListingResult()
                {
                    Id = 0,
                    errors = ajaxResult.ToString()
                };
            }
            return JsonConvert.DeserializeObject<CreateOrUpdateListingResult>(JsonConvert.SerializeObject(ajaxResult));
        }

        private async Task<CreateOrUpdateListingResult> updateListingAsync(GuruTaskListing guruTaskListing)
        {
            var jsonFomrate = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            var listing =await getListing(guruTaskListing.Listing.Id.ToString());
            listing.Update(guruTaskListing.Listing);
            var json = JsonConvert.SerializeObject(listing, jsonFomrate);
            var result =await AjaxJsonPost<object>($"https://agentnet.propertyguru.com.sg/sf2-agent/ajax/update/{guruTaskListing.Listing.Id}", "https://agentnet.propertyguru.com.sg/create-listing/detail/{guruTaskUpdateListing.Listing.Id}", "PUT", json);
            return JsonConvert.DeserializeObject<CreateOrUpdateListingResult>(JsonConvert.SerializeObject(result));
        }

        private async Task<CreateOrUpdateListing> getListing(string id)
        {
            var json = new object();
            try
            {
                json=await ajaxJsonGet<object>($"https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings/{id}");
                var jsonStr = JsonConvert.SerializeObject(json);
                return JsonConvert.DeserializeObject<CreateOrUpdateListing>(jsonStr);
            }
            catch(Exception ex)
            {

            }
            return null;
        }


        private async Task uploadPhotosAsync(GuruTaskListing guruTaskListing)
        {
            bool result = true;
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            for (int i = 0; i < guruTaskListing.Photos.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;

                var filePath = $"{path}{i}_image.jpg";
                try
                {
                    WebClientEx webClient = new WebClientEx();
                    webClient.DownloadFile(guruTaskListing.Photos[i], filePath);
                    await devToolsContext.EvaluateExpressionAsync(@"window.base64ToFile=function (dataurl, filename) { 
	    var arr = dataurl.split(','),
	        mime = arr[0].match(/:(.*?);/)[1],
	        bstr = atob(arr[1]),
	        n = bstr.length,
	        u8arr = new Uint8Array(n);
	    while (n--) {
	        u8arr[n] = bstr.charCodeAt(n);
	    }
	    return new File([u8arr], filename, { type: mime });
	}");
                    string dataString = "data:image/jpeg;base64," + Convert.ToBase64String(System.IO.File.ReadAllBytes(filePath));
                    await devToolsContext.EvaluateFunctionAsync($"(value)=>{{ window.file_{i}_img=window.base64ToFile(value,'{i}_img.jpg')}}", dataString);
                    var formData = new Dictionary<string, string>();
                    formData.Add("ownerId", $"{guruTaskListing.Listing.Id}");
                    formData.Add("mediaType", "'IMAGE'");// Videos=MOVIE ,Virtual Tours=VTOUR,Floorplan=IMAGE
                    formData.Add("mediaClass", "'UPHO'");// Videos=UMOV,Virtual Tours=UTOUR,Floorplan=UFLOO
                    formData.Add("source", "' AgentNet'");
                    formData.Add("userId", $"{guruTaskListing.Listing.Agent.id}");
                    formData.Add("caption", "''");
                    formData.Add("language", "'en'");
                    formData.Add("sortOrder", $"{i + 1}");

                    formData.Add("mediaFile", $"window.file_{i}_img");
                    //await devToolsContext.EvaluateExpressionAsync($"var file=window.document.createElement('input');file.type='file';file.id='file_1_img';document.body.appendChild(file)");

                    //var file=await devToolsContext.QuerySelectorAsync<CefSharp.Dom.HtmlInputElement>("#file_1_img");
                    StringBuilder sb = new StringBuilder();
                    sb.Append("var fd= new FormData();");
                    foreach (var item in formData)
                    {
                        sb.Append($"fd.append('{item.Key}',{item.Value});");
                    }
                    //string jscode = @$"var t=new FormData();t.append('ownerId', '23280908');t.append('mediaType', 'IMAGE');t.append('mediaClass', 'UPHO');t.append('source',' AgentNet');t.append('userId', '375435');t.append('caption', '');t.append('sortOrder', 4);t.append('mediaFile', document.getElementById('text').files[0]);return $.ajax({{url:'{url}',async:false,data:t,method:'POST',processData: false, contentType: false}})".Replace('\n', ' ').Replace('\r', ' ');
                    //sb.Append($"return $.ajax({{url:'{url}',async:false,data:fd,method:'POST',processData: false, contentType: false}})"/*.Replace('\n', ' ').Replace('\r', ' ')*/);
                    sb.Append($"fetch(\"https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings/{guruTaskListing.Listing.Id}/media\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.json())");
                    var jscode = sb.ToString();

                    var r = await devToolsContext.EvaluateExpressionAsync<object>(jscode);
                }
                catch (Exception ex)
                {
                    result = false;
                    continue;
                }
            }
        }

        private async Task uploadVideos(GuruTaskListing guruTaskListing)
        {
            bool result = true;
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            for (int i = 0; i < guruTaskListing.Videos.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;

                var url = guruTaskListing.Videos[i].ToLower();
                var formData = new Dictionary<string, string>();
                formData.Add("ownerId", guruTaskListing.Listing.Id.ToString());
                formData.Add("mediaType", "'MOVIE'");// Videos=MOVIE ,Virtual Tours=VTOUR,Floorplan=IMAGE
                formData.Add("mediaClass", "'UMOV'");// Videos=UMOV,Virtual Tours=UTOUR,Floorplan=UFLOO
                formData.Add("source", "' AgentNet'");
                formData.Add("userId", $"'{guruTaskListing.Listing.Agent.id}'");
                formData.Add("caption", "''");
                formData.Add("language", "'en'");
                formData.Add("sortOrder", $"{i + 1}");

                if (url.Contains("youtube") ||
                    url.Contains("vimeo") ||
                    url.Contains("dailymotion") ||
                    url.Contains("<iframe")
                    )
                {

                    if (url.Contains("<iframe"))
                    {
                        formData.Add("videoEmbedHtml", $"\"{guruTaskListing.Videos[i]}\"");
                    }
                    else
                    {
                        formData.Add("videoEmbedHtml", $"'{guruTaskListing.Videos[i]}'");
                    }
                }
                else
                {
                    var filePath = $"{path}{i}_movie.jpg";
                    try
                    {
                        WebClientEx webClient = new WebClientEx();
                        webClient.DownloadFile(guruTaskListing.Videos[i], filePath);
                        await devToolsContext.EvaluateExpressionAsync(@"window.base64ToFile=function (dataurl, filename) { 
	                            var arr = dataurl.split(','),
	                                mime = arr[0].match(/:(.*?);/)[1],
	                                bstr = atob(arr[1]),
	                                n = bstr.length,
	                                u8arr = new Uint8Array(n);
	                            while (n--) {
	                                u8arr[n] = bstr.charCodeAt(n);
	                            }
	                            return new File([u8arr], filename, { type: mime });
	                        }");
                        string dataString = "data:image/jpeg;base64," + Convert.ToBase64String(System.IO.File.ReadAllBytes(filePath));
                        await devToolsContext.EvaluateFunctionAsync($"(value)=>{{ window.file_{i}_move=window.base64ToFile(value,'{i}_move.jpg')}}", dataString);

                        formData.Add("mediaFile", $"window.file_{i}_move");
                    }
                    catch
                    {

                    }

                }
                StringBuilder sb = new StringBuilder();
                sb.Append("var fd= new FormData();");
                foreach (var item in formData)
                {
                    sb.Append($"fd.append('{item.Key}',{item.Value});");
                }
                sb.Append($"fetch(\"https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings/{guruTaskListing.Listing.Id}/media\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.json())");
                var jscode = sb.ToString();

                try
                {


                    var r = await devToolsContext.EvaluateExpressionAsync<object>(jscode);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private async Task uploadVirtualTours(GuruTaskListing guruTaskListing)
        {
            bool result = true;
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            for (int i = 0; i < guruTaskListing.Tours.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;

                var url = guruTaskListing.Tours[i].ToLower();
                var formData = new Dictionary<string, string>();
                formData.Add("ownerId", guruTaskListing.Listing.Id.ToString());
                formData.Add("mediaType", "'VTOUR'");// Videos=MOVIE ,Virtual Tours=VTOUR,Floorplan=IMAGE
                formData.Add("mediaClass", "'UTOUR'");// Videos=UMOV,Virtual Tours=UTOUR,Floorplan=UFLOO
                formData.Add("source", "' AgentNet'");
                formData.Add("userId", $"'{guruTaskListing.Listing.Agent.id}'");
                formData.Add("caption", "''");
                formData.Add("language", "'en'");
                formData.Add("sortOrder", $"{i + 1}");

                if (url.Contains("youtube") ||
                    url.Contains("vimeo") ||
                    url.Contains("dailymotion") ||
                    url.Contains("<iframe")
                    )
                {

                    if (url.Contains("<iframe"))
                    {
                        formData.Add("videoEmbedHtml", $"\"{guruTaskListing.Tours[i]}\"");
                    }
                    else
                    {
                        formData.Add("videoEmbedHtml", $"'{guruTaskListing.Tours[i]}'");
                    }
                }
                else
                {
                    var filePath = $"{path}{i}_vt.jpg";
                    try
                    {
                        WebClientEx webClient = new WebClientEx();
                        webClient.DownloadFile(guruTaskListing.Tours[i], filePath);
                        await devToolsContext.EvaluateExpressionAsync(@"window.base64ToFile=function (dataurl, filename) { 
	                            var arr = dataurl.split(','),
	                                mime = arr[0].match(/:(.*?);/)[1],
	                                bstr = atob(arr[1]),
	                                n = bstr.length,
	                                u8arr = new Uint8Array(n);
	                            while (n--) {
	                                u8arr[n] = bstr.charCodeAt(n);
	                            }
	                            return new File([u8arr], filename, { type: mime });
	                        }");
                        string dataString = "data:image/jpeg;base64," + Convert.ToBase64String(System.IO.File.ReadAllBytes(filePath));
                        await devToolsContext.EvaluateFunctionAsync($"(value)=>{{ window.file_{i}_vt=window.base64ToFile(value,'{i}_vt.jpg')}}", dataString);

                        formData.Add("mediaFile", $"window.file_{i}_vt");
                    }
                    catch
                    {

                    }

                }
                StringBuilder sb = new StringBuilder();
                sb.Append("var fd= new FormData();");
                foreach (var item in formData)
                {
                    sb.Append($"fd.append('{item.Key}',{item.Value});");
                }
                sb.Append($"fetch(\"https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings/{guruTaskListing.Listing.Id}/media\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.json())");
                var jscode = sb.ToString();

                try
                {


                    var r = await devToolsContext.EvaluateExpressionAsync<object>(jscode);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private async Task uploadFlooplan(GuruTaskListing guruTaskListing)
        {
            bool result = true;
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            for (int i = 0; i < guruTaskListing.FloorPlan.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;

                var filePath = $"{path}{i}_fp.jpg";
                try
                {
                    WebClientEx webClient = new WebClientEx();
                    webClient.DownloadFile(guruTaskListing.FloorPlan[i], filePath);
                    await devToolsContext.EvaluateExpressionAsync(@"window.base64ToFile=function (dataurl, filename) { 
	    var arr = dataurl.split(','),
	        mime = arr[0].match(/:(.*?);/)[1],
	        bstr = atob(arr[1]),
	        n = bstr.length,
	        u8arr = new Uint8Array(n);
	    while (n--) {
	        u8arr[n] = bstr.charCodeAt(n);
	    }
	    return new File([u8arr], filename, { type: mime });
	}");
                    string dataString = "data:image/jpeg;base64," + Convert.ToBase64String(System.IO.File.ReadAllBytes(filePath));
                    await devToolsContext.EvaluateFunctionAsync($"(value)=>{{ window.file_{i}_fp=window.base64ToFile(value,'{i}_fp.jpg')}}", dataString);
                    var formData = new Dictionary<string, string>();
                    formData.Add("ownerId", $"{guruTaskListing.Listing.Id}");
                    formData.Add("mediaType", "'IMAGE'");// Videos=MOVIE ,Virtual Tours=VTOUR,Floorplan=IMAGE
                    formData.Add("mediaClass", "'UFLOO'");// Videos=UMOV,Virtual Tours=UTOUR,Floorplan=UFLOO
                    formData.Add("source", "' AgentNet'");
                    formData.Add("userId", $"{guruTaskListing.Listing.Agent.id}");
                    formData.Add("caption", "''");
                    formData.Add("language", "'en'");
                    formData.Add("sortOrder", $"{i + 1}");

                    formData.Add("mediaFile", $"window.file_{i}_fp");
                    StringBuilder sb = new StringBuilder();
                    sb.Append("var fd= new FormData();");
                    foreach (var item in formData)
                    {
                        sb.Append($"fd.append('{item.Key}',{item.Value});");
                    }
                    sb.Append($"fetch(\"https://agentnet.propertyguru.com.sg/sf2-agent/ajax/listings/{guruTaskListing.Listing.Id}/media\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.json())");
                    var jscode = sb.ToString();

                    var r = await devToolsContext.EvaluateExpressionAsync<object>(jscode);
                }
                catch (Exception ex)
                {
                    result = false;
                    continue;
                }
            }
        }
        #endregion

        #region 辅助功能

        private string checkFileDirectory(string taskId)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\task\\{taskId}file\\";
            if (System.IO.Directory.Exists(path) == false)
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return path;
        }
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
            string jscode = $"()=> fetch('{url}',{{ method:\"{type}\",referrer:'{referrerUrl}',headers:{{'authorization':'Bearer {await getJwt()}','content-type': 'application/json;charset=UTF-8'}},body:{(data == "" ? "''" : "JSON.stringify(" + data.Replace('\"', '"') + ")")}}}) .then(response => response.json())";
            T result;
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
        private async Task watiForIsLoading()
        {
            while (ChromiumWebBrowser.IsLoading)
            {
                await randoTime(5000, 5000);
            }
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
        private Task randoTime(int min = 500, int max = 5000)
        {
            //System.Threading.Thread.Sleep();
            return Task.Delay(random.Next(min, max));
        }
        #endregion

    }
}
