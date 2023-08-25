using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Abp.Dependency;
using CefSharp;
using CefSharp.Dom;
using CefSharp.WinForms;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru;
using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Tasks;
using Propnex.Poster.Share;

namespace Propnex.Poster.Guru
{
    public class CefPosterGuruAction : IPosterAction<GuruTaskListing>, ITransientDependency
    {
        ChromiumWebBrowser ChromiumWebBrowser { get; set; }
        DevToolsContext devToolsContext { get; set; }
        public string Source { get; set; }

        private Random random;

        private string _token = "";

        private readonly Serilog.ILogger _logger;

        public CefPosterGuruAction(ChromiumWebBrowser chromiumWebBrowser, Serilog.ILogger logger, string source = "")
        {
            ChromiumWebBrowser = chromiumWebBrowser;
            Source = source;
            _logger = logger;
            random = new Random(this.GetHashCode());
        }

        public async Task Start()
        {
            await getDevToolsContext();
            ChromiumWebBrowser.ShowDevTools();
        }

        #region task 任务功能

        public async Task<PosterActionResult> PostOnly(GuruTaskListing task)
        {
            //document.querySelector("#listing-management-component > div > div > div > div > div > div.headline > p > strong")

            var listing = IsExtis(task, true);
            if (listing == null)
            {
                var result = await createListingAsync(task);
                if (result.errors != null && result.errors.ToString().ToLower().Contains("postcode") && task.Listing.Location.id == null)
                {
                    var locales = await ajaxJsonGet<List<QueryLocale>>($"https://prefix-search.propertyguru.com/v1/my/autocomplete?locale=en&limit=25&object_type=PROPERTY&query={task.Listing.Location.postalCode}&property_type_group_exclude=COMMERCIAL");
                    if (locales.Count > 0)
                    {
                        var t = await ajaxJsonGetWithJwt<QueryProject>($"https://projects-api-projectnet.propertyguru.com/v1/project?property_id={locales[0].ObjectId}&country=malaysia&language=en");
                        if (t != null && t.Addresses != null && t.Addresses.Count > 0)
                        {
                            task.Listing.Location.id = int.Parse(t.Addresses[0].external_id);
                            result = await createListingAsync(task);
                        }

                    }

                }
                //await Start();
                //await randoTime(1000 * 60);

                if (result.Id == 0)
                {
                    if (result.errors.ToString().ToLower().Contains("headline"))
                    {
                        task.Listing.LocalizedHeadline = DefaultTitles.GetTitle();
                        task.Listing.Headlines.En = task.Listing.LocalizedHeadline;
                    }
                    result = await createListingAsync(task);
                }
                task.Listing.Id = result.Id;
                if (result.Id != 0)
                {
                    await uploadPhotosAsync(task);
                    await uploadVideos(task);
                    await uploadVirtualTours(task);
                    await uploadFlooplan(task);
                    await changeStatusAct(task);
                    //await randoTime(1000 * 20);
                    await getLisints();
                    if (IsExtis(task) == null)
                    {
                        //await AjaxJsonPost<object>("https://agent-service.propertyguru.com/v1/sg/getPropertyNames", "",
                        //    data: $@"{{""statusCode"":""DRAFT"",""agentId"":{task.Listing.Agent.id}}}");
                        await ChromiumWebBrowser.LoadUrlAsync($"https://agentnet.propertyguru.com.my/v2/dash");
                        await watiForIsLoading();
                        await ChromiumWebBrowser.LoadUrlAsync("https://agentnet.propertyguru.com.my/v2/listing_management#draft");
                        await watiForIsLoading();
                        var postBtn = await devToolsContext.QuerySelectorAsync($"#listing-management-component > div > div > div > div > div > div > div.listing-card.listing-card-{result.Id} > div.listing-card-content > div > div > button");
                        if (postBtn != null)
                        {
                            await postBtn.ClickAsync();
                            await randoTime(1000 * 5);
                            var postNewBtn = await devToolsContext.QuerySelectorAsync(".MuiDialog-root.component-listing-reactivation-dialog > div.MuiDialog-container.MuiDialog-scrollPaper > div > div.MuiDialogActions-root.MuiDialogActions-spacing > div.action-buttons.centered > button");
                            await randoTime(1000 * 5);
                            await postNewBtn.ClickAsync();
                        }
                    }
                    await getLisints();
                    if (IsExtis(task, true) == null)
                    {
                        _logger.Information($"lsting id is {result.Id},in draft");
                        return new PosterActionResult()
                        {
                            Status = PosterActionResultStatus.Error,
                            Message = "Listing couldn't be saved. Error: [listing] Duplicate listing detected: 24289160"// $"poster success ,but listing in draft. listing id is ${result.Id}"
                        };
                    }
                }
                else
                {
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Error,
                        Message = result.errors.ToString()
                    };
                }

                return new PosterActionResult()
                {
                    Status = PosterActionResultStatus.Success,
                    Message = $"{result.Id}"
                };
            }
            else
            {
                return new PosterActionResult()
                {
                    Status = PosterActionResultStatus.Error,
                    Message = $"Existing listing detected {listing.Id}. Pls delete manually if you wish to create as new"
                };
            }
        }


        public async Task<PosterActionResult> Login(string userName, string password)
        {

            PosterActionResult result = new PosterActionResult();
            result.Status = PosterActionResultStatus.Error;
            for (int i = 0; i < 5; i++)
            {
                await getDevToolsContext();
                _logger.Information($"Login-{i}");
                try
                {
                    await ChromiumWebBrowser.LoadUrlAsync("https://agentnet.propertyguru.com.my/ex_logout");
                    await randoTime();
                    await watiForIsLoading();

                    if (devToolsContext.Url == "chrome-error://chromewebdata/")
                    {
                        await Api.WebServer.PingAsync();
                        i = 0;
                        continue;
                    }
                    var add = ChromiumWebBrowser.Address;
                    await devToolsContext.GoToAsync(add);

                    if (ChromiumWebBrowser.Address.StartsWith("https://accounts.propertyguru.com.my/account/login") == false)
                    {
                        await randoTime(1000 * 60 * 5);
                        result.Message = "Verification Code";
                        break;
                    }

                    var loginUserId = await devToolsContext.QuerySelectorAsync<HtmlInputElement>("input[name='username']");
                    if (loginUserId == null)
                    {
                        result.Message = "Verification Code";
                        break;
                    }
                    //await loginUserId.ClickAsync();
                    await randoTime();
                    //await loginUserId.SetValueAsync(userName.Replace("\n", "").Replace("\r", ""));
                    await devToolsContext.EvaluateExpressionAsync(@"window.setValue=function(query, value) { 
                        let element=document.querySelector(query);
                        let lastValue = element.value;
                        element.value = value;
                        let event = new Event('input', { target: element, bubbles: true });
                        // React 15
                        event.simulated = true;
                        // React 16
                        let tracker = element._valueTracker;
                        if (tracker) {
                            tracker.setValue(lastValue);
                        }
                        element.dispatchEvent(event);
                    }");
                    await devToolsContext.EvaluateFunctionAsync($@"()=>{{window.setValue(""input[name='username']"",'{userName.Replace("\n", "").Replace("\r", "")}')}}");
                    await randoTime();
                    //await loginUserId.ClickAsync();
                    await randoTime();
                    var loginUserPwd = await devToolsContext.QuerySelectorAsync<HtmlInputElement>("input[name='password']");
                    if (loginUserPwd != null)
                    {
                        //await loginUserPwd.ClickAsync();
                        await randoTime();
                        //await loginUserPwd.SetValueAsync(password.Replace("\n", "").Replace("\r", ""));
                        await devToolsContext.EvaluateFunctionAsync($@"()=>{{window.setValue(""input[name='password']"",'{password.Replace("\n", "").Replace("\r", "")}')}}");
                        await randoTime();
                        //await loginUserPwd.ClickAsync();
                    }
                    await randoTime();
                    var loginSubmit = await devToolsContext.QuerySelectorAsync<HtmlFormElement>("form");
                    if (loginSubmit != null)
                    {
                        await loginSubmit.SubmitAsync();
                        await randoTime();
                        await watiForIsLoading();

                        if (devToolsContext.Url != "https://agentnet.propertyguru.com.my/dash?" &&
                                    devToolsContext.Url != "https://agentnet.propertyguru.com.my/v2/dash")
                        {
                            if (devToolsContext.Url == "chrome-error://chromewebdata/")
                            {
                                await Api.WebServer.PingAsync();
                                i = 0;
                                continue;
                            }
                            var warningElement = await devToolsContext.QuerySelectorAsync<CefSharp.Dom.HtmlElement>("#error-message > div");
                            if (warningElement != null)
                            {
                                var text = await warningElement.GetInnerTextAsync();

                                if (text == "Invalid captcha value" || text.Contains("attempts"))
                                {
                                    result.Message = "Verification Code";
                                    _logger.Information(text);
                                    break;
                                }
                                else
                                {
                                    result.Message = text;
                                    _logger.Information(text);
                                    //await randoTime(1000 * 60 * 3);
                                    break;
                                }
                            }

                        }
                        else
                        {
                            var appdashboard = await devToolsContext.QuerySelectorAsync("#app-agent-dashboard");
                            var dashboard = await devToolsContext.QuerySelectorAsync("#dashboard");
                            if (dashboard == null && appdashboard == null)
                            {
                                await randoTime(1000 * 60 * 5);
                                result.Message = "not find app-agent-dashboard";
                                _logger.Information(result.Message);
                                break;
                            }

                            result.Status = PosterActionResultStatus.Success;
                            result.Message = "login success";
                            await getLisints();
                            await getJwt();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "login error");
                    result.Status = PosterActionResultStatus.Expection;
                    result.Message = ex.Message;
                }
            }
            return result;
        }


        public Task<PosterActionResult> Post(GuruTaskListing task)
        {
            throw new NotImplementedException();
        }

        public async Task<PosterActionResult> Update(GuruTaskListing task)
        {
            _logger.Information("Update");
            try
            {
                //get adcredits 
                if (IsExtis(task) != null)
                {
                    await getAgentId(task);
                    await updateListingAsync(task);
                    await deleteMedias(task.Listing.Id.Value);
                    await uploadPhotosAsync(task);
                    await uploadVideos(task);
                    await uploadVirtualTours(task);
                    await uploadFlooplan(task);
                    await changeStatusActUpdate(task);
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Success,
                        Message = task.Listing.Id.ToString()
                    };
                }
                else
                {
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Error,
                        Message = "Oops, we can’t find and match the above listing to perform any action. Please check your guru direct as you could have modified previously."
                    };
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update");
                return new PosterActionResult()
                {
                    Status = PosterActionResultStatus.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<PosterActionResult> Repost(GuruTaskListing task)
        {
            _logger.Information("Repost");
            if (IsExtis(task) != null)
            {
                try
                {
                    if (task.FastRepost == "0")
                    {
                        var result = await Update(task);
                        if (result.Status == PosterActionResultStatus.Success)
                        {
                            await AjaxJsonPost<object>($"https://agentnet.propertyguru.com.my/repost_listing?listing_id[]={task.Listing.Id}&statusCode=ACT&expectedCredits=", "");
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        await AjaxJsonPost<object>($"https://agentnet.propertyguru.com.my/repost_listing?listing_id[]={task.Listing.Id}&statusCode=ACT&expectedCredits=", "");
                    }
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Success,
                        Message = task.Listing.Id.ToString()
                    };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Repost");
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Expection,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                return await PostOnly(task);
            }
        }

        public async Task<PosterActionResult> Retrieve(string account, string targetPortal, string password, string id)
        {
            var listing = this.ListingInfos;
            foreach (var list in ListingInfos)
            {
                try
                {
                    var guruListing =await this.getListing(list.Id.ToString());
                    await Retrieve(guruListing, account, targetPortal, password, id);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Retrieve Error" + list.Id);
                }
            }
            return null;
        }
        public async Task<PosterActionResult> Retrieve(CreateOrUpdateListing task, string account, string targetPortal, string password, string id)
        {
            var postActionResult = new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success
            };
            try
            {
                _logger.Information("Retrieve");
                var url = "http://3.0.87.74/propnex/index.php/";
                //var guruListing = await this.getListing(task.Id.ToString());
                var retrieveListing =await RetrieveListing.Converter(task, account, targetPortal,id);
                retrieveListing.Account = account;
                var result = RetrieveListing.GetData(retrieveListing, account, password, id);
                if (result.Item2)
                {
                    var data = result.Item1;

                    FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(data);

                    HttpClient httpClient = new HttpClient();
                    var ok =await httpClient.PostAsync($"{url}listings/post", formUrlEncodedContent);
                    var httpResult =await ok.Content.ReadAsStringAsync();
                    string[] ss = httpResult.Split(new char[] { ',' });
                    string xpid = "";
                    if (ss.Length > 1 && ss[0] == "ok")
                    {
                        xpid = ss[1].Trim();
                        retrieveListing.Details["xpressorID"] = xpid;

                        Dictionary<string, string> files = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(retrieveListing.Photos)) retrieveListing.Photos = "";
                        string[] photos = retrieveListing.Photos.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        //int i = 0;
                        var formName = "ListingAttachments";
                        foreach (string photo in photos)
                        {
                            string[] vnames = photo.Split(new char[] { '#' });
                            string p = vnames[0].Trim();
                            if (!System.IO.File.Exists(p)) continue;
                            files = new Dictionary<string, string>();
                            files[formName + "[attachfile]"] = p;
                            data = new Dictionary<string, string>();
                            data[formName + "[category]"] = "photo";
                            data["xpressor"] = "";

                            data[formName + "[title]"] = "";
                            if (retrieveListing.UseFileName)
                            {
                                string fn = System.IO.Path.GetFileName(p);
                                string[] parts = fn.Split(new char[] { '.' });
                                if (parts.Length > 0)
                                {
                                    data[formName + "[title]"] = parts[0].Replace("-", " ").Replace("_", " ");
                                };
                                if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[1].Trim()))
                                {
                                    data[formName + "[title]"] = vnames[1].Trim();
                                }
                            }
                            try
                            {
                                using (var multipartFormDataContent = new MultipartFormDataContent())
                                {
                                    foreach (var content in data)
                                    {
                                        multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                                    }
                                    using (var client = new HttpClient())
                                    {
                                        using (var stream = new StreamContent(new System.IO.FileStream(p, System.IO.FileMode.Open)))
                                        {
                                            multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(p));
                                            ok =await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                            httpResult =await ok.Content.ReadAsStringAsync();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //DoProgress("Exception in PostWebPageMultipart", -1, "");
                                //throw;
                            }
                        }


                        if (!string.IsNullOrEmpty(retrieveListing.FloorPlan))
                        {
                            string[] floorplans = retrieveListing.FloorPlan.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string floorplan in floorplans)
                            {
                                string[] vnames = floorplan.Split(new char[] { '#' });
                                string p = vnames[0].Trim();
                                if (System.IO.File.Exists(p))
                                {
                                    files = new Dictionary<string, string>();
                                    files[formName + "[attachfile]"] = p;
                                    data = new Dictionary<string, string>();
                                    data[formName + "[category]"] = "floorplan";
                                    data["xpressor"] = "";

                                    data[formName + "[title]"] = "";
                                    if (retrieveListing.UseFileName)
                                    {
                                        string fn = System.IO.Path.GetFileName(p);
                                        string[] parts = fn.Split(new char[] { '.' });
                                        data[formName + "[title]"] = parts[0].Replace("-", " ");
                                    };
                                    if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[1].Trim()))
                                    {
                                        data[formName + "[title]"] = vnames[1].Trim();
                                    }
                                    try
                                    {
                                        using (var multipartFormDataContent = new MultipartFormDataContent())
                                        {
                                            foreach (var content in data)
                                            {
                                                multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                                            }
                                            using (var client = new HttpClient())
                                            {
                                                using (var stream = new StreamContent(new System.IO.FileStream(p, System.IO.FileMode.Open)))
                                                {
                                                    multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(p));
                                                    ok = await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                                    httpResult =await ok.Content.ReadAsStringAsync();
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //DoProgress("Exception in PostWebPageMultipart", -1, "");
                                        //throw;
                                    }

                                }
                            }
                        }

                        string[] tours = retrieveListing.Tours.Split(new string[] { "\n" }, StringSplitOptions.None);
                        string[] toursThumbnail = retrieveListing.TourThumbnails.Split(new string[] { "\n" }, StringSplitOptions.None);
                        int i = 0;
                        for (i = 0; i < tours.Length; i++)
                        {
                            string tour = tours[i].Trim();
                            if (string.IsNullOrEmpty(tour)) continue;
                            string tourThumbnail = toursThumbnail[i].Trim();
                            data = new Dictionary<string, string>();
                            data[formName + "[category]"] = "tour";
                            data[formName + "[thumbnail]"] = tourThumbnail;

                            string[] vnames = tour.Split(new char[] { '#' });
                            string p = vnames[0].Trim();
                            if (vnames.Length > 2)
                            {
                                p = string.Join("#", vnames, 0, vnames.Length - 1);
                            }
                            files = new Dictionary<string, string>();
                            data[formName + "[title]"] = "";
                            if (p.EndsWith(".swf", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".mov", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".flv", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (!System.IO.File.Exists(p)) continue;
                                files[formName + "[attachfile]"] = p.Trim();
                                if (retrieveListing.UseFileName)
                                {
                                    string fn = System.IO.Path.GetFileName(p.Trim());
                                    string[] parts = fn.Split(new char[] { '.' });
                                    data[formName + "[title]"] = parts[0].Replace("-", " ");
                                }
                            }
                            else
                            {
                                data[formName + "[embed_code]"] = p;
                            }

                            data["xpressor"] = "";

                            if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[vnames.Length - 1].Trim()))
                            {
                                data[formName + "[title]"] = vnames[vnames.Length - 1].Trim();
                            }
                            try
                            {
                                using (var multipartFormDataContent = new MultipartFormDataContent())
                                {
                                    foreach (var content in data)
                                    {
                                        multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                                    }
                                    using (var client = new HttpClient())
                                    {
                                        using (var stream = new StreamContent(new System.IO.FileStream(p, System.IO.FileMode.Open)))
                                        {
                                            multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(p));
                                            ok =await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                            httpResult =await ok.Content.ReadAsStringAsync();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //DoProgress("Exception in PostWebPageMultipart", -1, "");
                                //throw;
                            }
                        }

                        string[] videos = retrieveListing.Videos.Split(new string[] { "\n" }, StringSplitOptions.None);
                        string[] videosThumbnail = retrieveListing.VideoThumbnails.Split(new string[] { "\n" }, StringSplitOptions.None);
                        i = 0;
                        for (i = 0; i < videos.Length; i++)
                        {
                            string video = videos[i];
                            if (string.IsNullOrEmpty(video)) continue;
                            string videoThumbnail = videosThumbnail.Length > i ? videosThumbnail[i] : "";
                            data = new Dictionary<string, string>();
                            data[formName + "[category]"] = "video";
                            data[formName + "[thumbnail]"] = videoThumbnail;
                            string[] vnames = video.Split(new char[] { '#' });
                            string p = vnames[0].Trim();
                            if (vnames.Length > 2) p = string.Join("#", vnames, 0, vnames.Length - 1);
                            files = new Dictionary<string, string>();
                            data[formName + "[title]"] = "";
                            if (p.EndsWith(".swf", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".mov", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".flv", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (!System.IO.File.Exists(p)) continue;
                                files[formName + "[attachfile]"] = p.Trim();
                                if (retrieveListing.UseFileName)
                                {
                                    string fn = System.IO.Path.GetFileName(p.Trim());
                                    string[] parts = fn.Split(new char[] { '.' });
                                    data[formName + "[title]"] = parts[0].Replace("-", " ");
                                };
                            }
                            else
                            {
                                data[formName + "[embed_code]"] = p;
                            };
                            data["xpressor"] = "";

                            if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[vnames.Length - 1].Trim()))
                            {
                                data[formName + "[title]"] = vnames[vnames.Length - 1].Trim();
                            };

                            try
                            {
                                using (var multipartFormDataContent = new MultipartFormDataContent())
                                {
                                    foreach (var content in data)
                                    {
                                        multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                                    }
                                    using (var client = new HttpClient())
                                    {
                                        using (var stream = new StreamContent(new System.IO.FileStream(p, System.IO.FileMode.Open)))
                                        {
                                            multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(p));
                                            ok =await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                            httpResult =await ok.Content.ReadAsStringAsync();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //DoProgress("Exception in PostWebPageMultipart", -1, "");
                                //throw;
                            }
                        }
                    }
                }
                else
                {
                    _logger.Error(result.Item3);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                postActionResult.Status = PosterActionResultStatus.Error;
            }
            return postActionResult;
        }

        public async Task<PosterActionResult> Remove(GuruTaskListing task)
        {
            _logger.Information($"remove {task.Listing.Id}");
            if (IsExtis(task) != null)
            {
                var listingInfo = IsExtis(task);
                if (listingInfo.IsBoosted == false && listingInfo.IsTurbo == false)
                {
                    await getAgentId(task);
                    var result = await AjaxJsonPost<object>("https://bff-mobile.propertyguru.com/v1/listingManagement/delist?region=my", "", data: $"{{'listingIds':[{task.Listing.Id}],'statusCode':'DEL','agentId':{task.Listing.Agent.id}}}");
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Success,
                        Message = ""
                    };
                }
                else
                {
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Error,
                        Message = "Sorry, listing detected as boost/turbo. Please remove directly at PG. "
                    };
                }
            }
            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Error,
                Message = "Oops, we can’t find and match the above listing to perform any action. Please check your guru direct as you could have modified previously."
            };
        }

        private async Task changeStatusAct(GuruTaskListing guruTask)
        {
            _logger.Information("changeStatusAct");
            var tryCount = 0;
            while (tryCount < 10)
            {
                try
                {
                    _logger.Information($"tryCount {tryCount}");
                    tryCount++;
                    await ChromiumWebBrowser.LoadUrlAsync($"https://agentnet.propertyguru.com.my/create-listing/media/{guruTask.Listing.Id}");
                    await watiForIsLoading();

                    var nextButtons = await devToolsContext.QuerySelectorAllAsync("#lcv2-bar-footer >div > div > button");
                    if (nextButtons != null && nextButtons.Length == 3)
                    {
                        await nextButtons[2].ClickAsync();
                        await randoTime(2000);
                        await watiForIsLoading();

                        var proceedButtons = await devToolsContext.QuerySelectorAllAsync("body > div > div > div > div > button");
                        if (proceedButtons != null && proceedButtons.Length == 3)
                        {
                            await proceedButtons[1].ClickAsync();
                            await randoTime(5000);
                            await watiForIsLoading();
                        }
                        else
                        {
                            _logger.Information("not find proceedButtons");
                        }

                        var postButtons = await devToolsContext.QuerySelectorAllAsync("#lcv2-bar-footer >div > div > button");
                        if (postButtons != null && postButtons.Length == 3)
                        {
                            await postButtons[2].ClickAsync();
                            await randoTime(2000);
                            await watiForIsLoading();
                            await randoTime(5000);

                            var okButtons = await devToolsContext.QuerySelectorAllAsync("body > div > div > div > div > button");
                            if (okButtons != null && okButtons.Length == 3)
                            {
                                await okButtons[2].ClickAsync();
                                await randoTime(2000);
                                await watiForIsLoading();
                            }
                            else
                            {
                                _logger.Information("not find okButtons");
                            }
                        }
                        else
                        {
                            _logger.Information("not find postButton");
                        }
                    }
                    else
                    {
                        _logger.Information("not find nextButton");
                    }
                    await randoTime(1000 * 10);
                    tryCount = 100;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "changeStatusAct");
                }
            }
        }

        private async Task changeStatusActUpdate(GuruTaskListing guruTask)
        {
            _logger.Information("changeStatusActUpdate");
            var tryCount = 0;
            while (tryCount < 10)
            {
                try
                {
                    _logger.Information($"tryCount {tryCount}");
                    tryCount++;
                    await ChromiumWebBrowser.LoadUrlAsync($"https://agentnet.propertyguru.com.my/create-listing/media/{guruTask.Listing.Id}");
                    await watiForIsLoading();

                    var buttons = await devToolsContext.QuerySelectorAllAsync("#lcv2-bar-footer >div > div > button");
                    if (buttons != null && buttons.Length == 3)
                    {
                        await buttons[2].ClickAsync();
                        await randoTime(2000);
                        await watiForIsLoading();

                        var proceedButtons = await devToolsContext.QuerySelectorAllAsync("body > div > div > div > div > button");
                        if (proceedButtons != null && proceedButtons.Length == 3)
                        {
                            await proceedButtons[1].ClickAsync();
                            await randoTime(5000);
                            await watiForIsLoading();
                        }
                        else
                        {
                            _logger.Information("not find proceedButtons");
                        }

                        var postButtons = await devToolsContext.QuerySelectorAllAsync("#lcv2-bar-footer >div > div > button");
                        if (postButtons != null && postButtons.Length == 3)
                        {
                            await postButtons[1].ClickAsync();
                            await randoTime(2000);
                            await watiForIsLoading();
                            _logger.Information("not click save");
                            //var okButtons = await devToolsContext.QuerySelectorAllAsync("body > div > div > div > div > button");
                            //if (okButtons != null && okButtons.Length == 3)
                            //{
                            //    await okButtons[2].ClickAsync();
                            //    await randoTime(1000);
                            //    await watiForIsLoading();
                            //}
                        }
                        else
                        {
                            _logger.Information("not find postButton");
                        }
                    }
                    else
                    {
                        _logger.Information("not find nextButton");
                    }
                    await randoTime(1000 * 10);
                    tryCount = 100;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "changeStatusAct");
                }
            }
        }

        private bool isErrorPage()
        {
            return devToolsContext.Url == "chrome-error://chromewebdata/";
        }

        private async Task GoToAsync(string url)
        {
            await getDevToolsContext();
            await devToolsContext.GoToAsync(url);
            await randoTime();
            await watiForIsLoading();
        }

        List<ListingInfo> ListingInfos = null;
        private ListingInfo IsExtis(GuruTaskListing guruTaskListing, bool isPostOnly = false)
        {
            _logger.Information("IsExtis");
            ListingInfo listingInfo = null;
            if (guruTaskListing.Listing.Id.HasValue)
            {
                listingInfo = ListingInfos.Where(q => q.Id == guruTaskListing.Listing.Id).FirstOrDefault();
            }
            if (isPostOnly)
            {
                return listingInfo;
            }

            if (listingInfo == null)
            {
                if (Source.ToLower() == "chope")
                {
                    listingInfo = ListingInfos.Where(q => q.Sqft == guruTaskListing.Listing.Sizes.floorArea[0].text.Trim()
                                                 && q.Title == guruTaskListing.Listing.Property.name && q.TypeCode == guruTaskListing.Listing.TypeCode
                                                 && q.Prece == guruTaskListing.Listing.Price.value.ToString()
                                                 //&& q.StreetName == guruTaskListing.Listing.Location.streetName1
                                                 //&& q.StreetNumber == guruTaskListing.Listing.Location.streetNumber
                                                 //&& q.PostCode == guruTaskListing.Listing.Location.postalCode
                                                 ).FirstOrDefault();
                    if (listingInfo == null)
                    {
                        if (guruTaskListing.Listing.TypeCode.ToUpper() == "ROOM")
                        {
                            listingInfo = ListingInfos.Where(q => q.Sqft == guruTaskListing.Listing.Sizes.floorArea[0].text.Trim()
                                                    && q.Title == guruTaskListing.Listing.Property.name && q.TypeCode == "RENT"
                                                    && q.Prece == guruTaskListing.Listing.Price.value.ToString()
                                                    //&& q.StreetName == guruTaskListing.Listing.Location.streetName1
                                                    //&& q.StreetNumber == guruTaskListing.Listing.Location.streetNumber
                                                    //&& q.PostCode == guruTaskListing.Listing.Location.postalCode
                                                    ).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    listingInfo = ListingInfos.Where(q => q.Sqft == guruTaskListing.Listing.Sizes.floorArea[0].text.Trim()
                                                 && q.Title == guruTaskListing.Listing.Property.name && q.TypeCode == guruTaskListing.Listing.TypeCode
                                                 //&& q.StreetName == guruTaskListing.Listing.Location.streetName1
                                                 //   && q.StreetNumber == guruTaskListing.Listing.Location.streetNumber
                                                 //   && q.PostCode == guruTaskListing.Listing.Location.postalCode
                                                 ).FirstOrDefault();
                    if (listingInfo == null)
                    {
                        if (guruTaskListing.Listing.TypeCode.ToUpper() == "ROOM")
                        {
                            listingInfo = ListingInfos.Where(
                                                    q => q.Sqft == guruTaskListing.Listing.Sizes.floorArea[0].text.Trim()
                                                 && q.Title == guruTaskListing.Listing.Property.name && q.TypeCode == "RENT"
                                                   //&& q.StreetName == guruTaskListing.Listing.Location.streetName1
                                                   //   && q.StreetNumber == guruTaskListing.Listing.Location.streetNumber
                                                   //   && q.PostCode == guruTaskListing.Listing.Location.postalCode
                                                   ).FirstOrDefault();
                        }
                    }


                }
            }
            if (listingInfo != null)
            {
                guruTaskListing.Listing.Id = listingInfo.Id;
            }
            if (listingInfo == null)
            {
                _logger.Information("not find listingInfo");
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
            var infos = new List<ListingInfo>();
            for (int i = 0; i < 3; i++)
            {
                ListingInfos = new List<ListingInfo>();
                await GoToAsync("https://agentnet.propertyguru.com.my/v2/listing_management");
                while (isErrorPage())
                {
                    await Api.WebServer.PingAsync();
                    await GoToAsync("https://agentnet.propertyguru.com.my/v2/listing_management");
                }

                infos = new List<ListingInfo>();
                try
                {
                    await getListingsV2();
                    if (ListingInfos.Count > 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //throw new Exception("GetListings error");
                    await Api.WebServer.PingAsync();
                }
            }
            async Task getListingsV2()
            {
                infos = new List<ListingInfo>();
                var func = $@"()=>{{
                           return fetch('https://bff-mobile.propertyguru.com/v1/listingManagement?region=my&locale=en&status_code=ACT&sort=start_date&order=desc&page=1&limit=20000&timestamp=1616142255393',
                        {{ headers:{{'authorization':'Bearer {await getJwt()}'}}}}).then(res=>{{
                                      return res.json()
                                }})}}";

                var result = await devToolsContext.EvaluateFunctionAsync<ListingsResult>(func);
                var jsonResult = result;
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
                _logger.Information($"get listings {ListingInfos.Count}");
            }
        }

        private async Task getAgentId(GuruTaskListing guruTaskUpdateListing)
        {
            // var agentId = await getCookie("PGID1");
            _logger.Information("GetAgentId");
            await getDevToolsContext();
            var agentId = await devToolsContext.EvaluateFunctionAsync<int>("()=> guruApp.user_id");
            if (agentId != 0)
                guruTaskUpdateListing.Listing.Agent.id = agentId;

            _logger.Information($"AgentId is :{agentId}");
        }

        private async Task deleteMedias(int id)
        {
            _logger.Information($"deleteMedias");
            await ChromiumWebBrowser.LoadUrlAsync($"https://agentnet.propertyguru.com.my/create-listing/media/{id}");
            await watiForIsLoading();
            var listing = await getListing(id.ToString());
            var medias = await getListingMediaStatus(id.ToString());
            if (listing != null)
            {
                if (listing.media != null && listing.media.listingVideos != null)
                {
                    for (var i = 0; i < listing.media.listingVideos.Count; i++)
                    {
                        var item = listing.media.listingVideos[i];
                        await deleteMedia(item.id.ToString());
                    }
                }
                if (listing.media != null && listing.media.listingVirtualTours != null)
                {
                    for (var i = 0; i < listing.media.listingVirtualTours.Count; i++)
                    {
                        var item = listing.media.listingVirtualTours[i];
                        await deleteMedia(item.id.ToString());
                    }
                }
            }
            if (medias != null && medias.listing != null)
            {
                for (var i = 0; i < medias.listing.Count; i++)
                {
                    var item = medias.listing[i];
                    await deleteMedia(item.id.ToString());
                }
            }
            if (medias != null && medias.listingFloorplans != null)
            {
                for (var i = 0; i < medias.listingFloorplans.Count; i++)
                {
                    var item = medias.listingFloorplans[i];
                    await deleteMedia(item.id.ToString());
                }
            }

            async Task deleteMedia(string mediaId)
            {
                var result = await ajaxJsonDelete<object>($"https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings/{id}/media/{mediaId}");
            }
        }


        /// <summary>
        /// 获取jwt
        /// </summary>
        /// <returns></returns>
        private async Task<string> getJwt()
        {
            var result = await ajaxJsonGet<JwtResult>("https://agentnet.propertyguru.com.my/sf2-agent/ajax/agent/jwt");
            _token = result.accessToken;
            _logger.Information($"getJwt:{_token}");

            return _token;
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
            _logger.Information($"createListingAsync");
            await getAgentId(guruTaskUpdateListing);
            var jsonFomrate = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            var createOrUpdateListing = new CreateOrUpdateListing();
            createOrUpdateListing.Create(guruTaskUpdateListing.Listing);
            var json = JsonConvert.SerializeObject(createOrUpdateListing, jsonFomrate);
            var ajaxResult = await AjaxJsonPost<object>("https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings", "", data: json);
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
                    ajaxResult = await AjaxJsonPost<object>("https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings", "", data: json);
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
            _logger.Information($"updateListingAsync");
            var jsonFomrate = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            var listing = await getListing(guruTaskListing.Listing.Id.ToString());
            listing.Update(guruTaskListing.Listing);
            var json = JsonConvert.SerializeObject(listing, jsonFomrate);
            var result = await AjaxJsonPost<object>($"https://agentnet.propertyguru.com.my/sf2-agent/ajax/update/{guruTaskListing.Listing.Id}", "https://agentnet.propertyguru.com.my/create-listing/detail/{guruTaskUpdateListing.Listing.Id}", "PUT", json);
            return JsonConvert.DeserializeObject<CreateOrUpdateListingResult>(JsonConvert.SerializeObject(result));
        }

        private async Task<CreateOrUpdateListing> getListing(string id)
        {
            _logger.Information($"getListing");
            var json = new object();
            try
            {
                json = await ajaxJsonGet<object>($"https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings/{id}");
                var jsonStr = JsonConvert.SerializeObject(json);
                return JsonConvert.DeserializeObject<CreateOrUpdateListing>(jsonStr);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private async Task<Media> getListingMediaStatus(string id)
        {
            _logger.Information($"getListingMediaStatus");
            try
            {
                return await ajaxJsonGet<Media>($"https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings/{id}/media-status");
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private async Task uploadPhotosAsync(GuruTaskListing guruTaskListing)
        {
            _logger.Information($"uploadPhotosAsync");
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
                    sb.Append($"fetch(\"https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings/{guruTaskListing.Listing.Id}/media\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.json())");
                    var jscode = sb.ToString();

                    var r = await devToolsContext.EvaluateExpressionAsync<object>(jscode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "uploadPhotosAsync");
                    result = false;
                    continue;
                }
            }
        }

        private async Task uploadVideos(GuruTaskListing guruTaskListing)
        {
            _logger.Information($"uploadVideos");
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
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "");
                    }

                }
                StringBuilder sb = new StringBuilder();
                sb.Append("var fd= new FormData();");
                foreach (var item in formData)
                {
                    sb.Append($"fd.append('{item.Key}',{item.Value});");
                }
                sb.Append($"fetch(\"https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings/{guruTaskListing.Listing.Id}/media\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.json())");
                var jscode = sb.ToString();

                try
                {


                    var r = await devToolsContext.EvaluateExpressionAsync<object>(jscode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "");
                }
            }
        }

        private async Task uploadVirtualTours(GuruTaskListing guruTaskListing)
        {
            _logger.Information("uploadVirtualTours");
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
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "uploadVirtualTours");
                    }

                }
                StringBuilder sb = new StringBuilder();
                sb.Append("var fd= new FormData();");
                foreach (var item in formData)
                {
                    sb.Append($"fd.append('{item.Key}',{item.Value});");
                }
                sb.Append($"fetch(\"https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings/{guruTaskListing.Listing.Id}/media\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.json())");
                var jscode = sb.ToString();

                try
                {


                    var r = await devToolsContext.EvaluateExpressionAsync<object>(jscode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "uploadVirtualTours");
                }
            }
        }

        private async Task uploadFlooplan(GuruTaskListing guruTaskListing)
        {
            _logger.Information("uploadFlooplan");
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
                    sb.Append($"fetch(\"https://agentnet.propertyguru.com.my/sf2-agent/ajax/listings/{guruTaskListing.Listing.Id}/media\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.json())");
                    var jscode = sb.ToString();

                    var r = await devToolsContext.EvaluateExpressionAsync<object>(jscode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "uploadFlooplan");
                    result = false;
                    continue;
                }
            }
        }
        #endregion

        #region 辅助功能

        private string checkFileDirectory(string taskId)
        {
            var path = $"{Directory.GetDirectoryRoot(Application.StartupPath)}\\task\\{taskId}file\\";
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
                                    ""accept"": ""application/json, text/plain, */*"",
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

        private async Task<T> ajaxJsonGetWithJwt<T>(string url, string data = "")
        {
            try
            {
                string jscode = $@"()=> {{return fetch(""{url}"", {{
                                  ""headers"": {{
                                    ""accept"": ""application/json, text/plain, */*"",
                                    ""authorization"":""Bearer {await getJwt()}""
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

        private async Task<T> ajaxJsonDelete<T>(string url)
        {
            var jscode = $"()=>fetch('{url}',{{method:'DELETE'}}).then(response => response.json())";
            try
            {
                var result = await devToolsContext.EvaluateFunctionAsync<T>(jscode);
                return result;
            }
            catch (Exception ex)
            {
                // Logger.Error(ex, "Delete Media Error ");
            }
            return default(T);
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
                _logger.Information("watiForIsLoading");
            }
            await randoTime(5000, 5000);
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
            var value = min;
            if (min > max)
            {
                value = min;
            }
            else
            {
                value = random.Next(min, max);
            }
            _logger.Information($"randoTime {value}");
            return Task.Delay(value);
        }

        #endregion

    }
}
