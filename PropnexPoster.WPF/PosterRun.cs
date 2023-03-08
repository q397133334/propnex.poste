using Flurl.Http;
using Microsoft.Extensions.Logging;
using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using Propnex.Poster.PropertyGuru.Tasks;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace PropnexPoster.WPF
{
    public class PosterRun : Volo.Abp.DependencyInjection.ITransientDependency
    {

        public Action<string>? MessageEvent { get; set; }

        public Action<string, string, string> TaskInfoEvent { get; set; }

        private ILogger? _logger;

        private readonly ILogger<PosterRun> globleLogger;

        private PnTaskDto taskDto;

        public PosterRun()
        {

        }


        public async Task Run()
        {
            Log("Get Task .....");
            //1.获取任务信息
            var guruTasks = await getGuruTasks();
            //taskDto = new PnTaskDto()
            //{
            //    Number = "890991.guru.tsk"
            //};
            //var context = await File.ReadAllTextAsync("D:\\外包项目\\新加坡\\propnex.poster\\Propnex.Poster.WebServer\\wwwroot\\taskxml\\890991.guru.tsk");
            //var lenght = context.IndexOf("Xpressor-Listing-File===");
            //var taskContext = context.Substring(0, lenght == -1 ? context.Length : lenght);
            //var guruTasks = new GuruTasks(context, taskContext);
            if (guruTasks == null)
            {
                Log("Not find task ,delay 1 min");
                await Task.Delay(1000 * 10);
                return;
            }
            TaskInfoEvent?.Invoke(taskDto.Number, "", "");
            Log($"Get Tas success,{taskDto.Number}");
            //2.生成日志
            _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File($"{Directory.GetDirectoryRoot(System.AppDomain.CurrentDomain.BaseDirectory)}\\logs\\task\\{taskDto.Number}.txt", rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
            //4.处理任务
            for (int i = 0; i < guruTasks.Tasks.Count; i++)
            {
                //1.获取用户信息
                var task = guruTasks.Tasks[i];
                //3.登陆
                Log("Get Token .......");
                var token = await Login(task);
                if (token == null)
                {

                    foreach (var listing in task.Listings.Listings)
                    {
                        await ResultUpload(task, listing, listing.TaskItemId, "", "Failed", "Login Faile ,Please check password");
                        await End(task, listing.TaskItemId);
                    }
                    await XwebEnd(task);
                    return;
                }
                Log("Token success");

                Log($"{task.TaskType.ToLower()}");

                var _api = new Api() { Token = token };
                var _projectsApi = new ProjectsApi() { Token = token };
                var _adsProject = new AdsProduct(token);
                var _mobile = new Mobile(token);
                //4.执行操作
                if (task.TaskType.ToLower() == "post only")
                {

                    foreach (var listing in task.Listings.Listings)
                    {
                        //var listings = _mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString()));
                        //1. 获取邮政编号
                        //var locales = await _api.AutocompleteAsync(new QueryAutocomplete(listing.Listing.Location.postalCode));
                        //var locale = locales.Data.FirstOrDefault();
                        ////2. 获取loca 信息
                        //var project = (await _projectsApi.GetProjectAsync(int.Parse(locale.ObjectId))).Data;
                        //3. 组织 createlisting
                        var createOrUpdateListing = new CreateOrUpdateListing();
                        listing.Listing.Agent.id = token.User.AgentId;
                        createOrUpdateListing.Create(listing.Listing);
                        var result = await _api.CreateAsync(createOrUpdateListing);
                        //result = new HttpResult<CreateOrUpdateListingResult>() { Data = new CreateOrUpdateListingResult { Id = 24371139 } };
                        if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            listing.Listing.Id = result.Data.Id;
                            if (result.Data.Id != 0)
                            {
                                await uploadPhotosAsync(listing, _api);
                                await uploadVideos(listing, _api);
                                await uploadVirtualTours(listing, _api);
                                await uploadFloorPlanAsync(listing, _api);
                                var activateResult = await _adsProject.Activate(result.Data.Id);
                                if (activateResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                                }
                                else
                                {
                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", activateResult.Data);
                                }
                            }
                        }
                        else
                        {
                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", result.Message);
                        }
                        await End(task, listing.TaskItemId);
                    }
                    await XwebEnd(task);
                }

                if (task.TaskType.ToLower() == "repost")
                {
                    foreach (var listing in task.Listings.Listings)
                    {
                        if (IsExtis(task, listing) != null)
                        {
                            var listInfo = IsExtis(task, listing);
                            var taskListing = await _api.GetListing(listing.Listing.Id.Value);
                            if (listing.FastRepost == "0")
                            {
                                //更新任务

                                taskListing.Data.Update(listing.Listing);
                                await _api.UpdateAsync(taskListing.Data);
                                await _mobile.DeleteMediaAll(taskListing.Data);
                                await uploadPhotosAsync(listing, _api);
                                await uploadVideos(listing, _api);
                                await uploadVirtualTours(listing, _api);
                                await uploadFloorPlanAsync(listing, _api);
                            }
                            //Repost
                            await _adsProject.Repost(taskListing.Data.id.Value, listInfo.RepostCharge);
                        }
                        else
                        {
                            //Post Only
                            var createOrUpdateListing = new CreateOrUpdateListing();
                            listing.Listing.Agent.id = token.User.AgentId;
                            createOrUpdateListing.Create(listing.Listing);
                            var result = await _api.CreateAsync(createOrUpdateListing);
                            //result = new HttpResult<CreateOrUpdateListingResult>() { Data = new CreateOrUpdateListingResult { Id = 24371139 } };
                            if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                            {
                                listing.Listing.Id = result.Data.Id;
                                if (result.Data.Id != 0)
                                {
                                    await uploadPhotosAsync(listing, _api);
                                    await uploadVideos(listing, _api);
                                    await uploadVirtualTours(listing, _api);
                                    await uploadFloorPlanAsync(listing, _api);
                                    await _adsProject.Activate(result.Data.Id);
                                }
                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                            }
                            else
                            {

                            }
                        }
                        await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                        await End(task, listing.TaskItemId);
                    }
                    await XwebEnd(task);
                }

                if (task.TaskType.ToLower() == "update")
                {
                    foreach (var listing in task.Listings.Listings)
                    {
                        if (IsExtis(task, listing) != null)
                        {

                            var taskListing = await _api.GetListing(listing.Listing.Id.Value);
                            //更新任务

                            taskListing.Data.Update(listing.Listing);
                            await _api.UpdateAsync(taskListing.Data);
                            await _mobile.DeleteMediaAll(taskListing.Data);
                            await _mobile.DeleteMediaAll(taskListing.Data);
                            await uploadPhotosAsync(listing, _api);
                            await uploadVideos(listing, _api);
                            await uploadVirtualTours(listing, _api);
                            await uploadFloorPlanAsync(listing, _api);
                        }
                        await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                        await End(task, listing.TaskItemId);
                    }
                    await XwebEnd(task);
                }

                //remove from portals
                if (task.TaskType.ToLower() == "remove from portals")
                {

                }

                if (task.TaskType.ToLower().IndexOf("retrieve") > -1)
                {

                }
            }
            //5.
        }
        private List<ListingInfo> ListingInfos = new List<ListingInfo>();

        private ListingInfo IsExtis(GuruTask guruTask, GuruTaskListing guruTaskListing, bool isPostOnly = false)
        {
            ListingInfo listingInfo = null;
            if (guruTaskListing.Listing.Id.HasValue)
            {
                listingInfo = ListingInfos.FirstOrDefault(q => q.Id == guruTaskListing.Listing.Id);
            }
            if (isPostOnly)
            {
                return listingInfo;
            }

            if (listingInfo == null)
            {
                if (guruTask.Source.ToLower() == "chope")
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

        private async Task uploadPhotosAsync(GuruTaskListing guruTaskListing, Api _api)
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
                await guruTaskListing.Photos[i].DownloadFileAsync(path, $"{i}_image.jpg");

                await _api.UploadPhotoAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath);
            }
        }

        private async Task uploadVideos(GuruTaskListing guruTaskListing, Api _api)
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
                var filePath = $"{path}{i}_movie.mp4";
                if (url.Contains("youtube") ||
                    url.Contains("vimeo") ||
                    url.Contains("dailymotion") ||
                    url.Contains("<iframe")
                    )
                {
                    filePath = guruTaskListing.Videos[i];
                }
                else
                {
                    await guruTaskListing.Videos[i].DownloadFileAsync(path, $"{i}_movie.mp4");
                }
                await _api.UploadVideosAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath);
            }
        }

        private async Task uploadVirtualTours(GuruTaskListing guruTaskListing, Api _api)
        {
            bool result = true;
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            for (int i = 0; i < guruTaskListing.Tours.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;

                var url = guruTaskListing.Videos[i].ToLower();
                var filePath = $"{path}{i}_vt.jpg";
                if (url.Contains("youtube") ||
                    url.Contains("vimeo") ||
                    url.Contains("dailymotion") ||
                    url.Contains("<iframe")
                    )
                {
                    filePath = guruTaskListing.Tours[i];
                }
                else
                {
                    await guruTaskListing.Tours[i].DownloadFileAsync(path, $"{i}_movie.jpg");
                }
                await _api.UplaodVirtualTours($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath);
            }
        }

        private async Task uploadFloorPlanAsync(GuruTaskListing guruTaskListing, Api _api)
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
                await guruTaskListing.FloorPlan[i].DownloadFileAsync(path, $"{i}_fp.jpg");

                await _api.UploadPhotoAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath);
            }
        }

        private string checkFileDirectory(string taskId)
        {
            var path = $"{Directory.GetDirectoryRoot(AppDomain.CurrentDomain.BaseDirectory)}task\\{taskId}file\\";
            if (System.IO.Directory.Exists(path) == false)
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return path;
        }

        private async Task<Propnex.Poster.PropertyGuru.Mobile.Dto.Token> Login(GuruTask guruTask)
        {
            var pnUser = await getUser();
            var _Token = string.IsNullOrEmpty(pnUser.TokenJson) ? await auth() : await checkToken();
            if (_Token == null)
                return null;
            await getListing();

            async Task<PnUserDto> getUser()
            {
                var pnUser = await WebServer.GetUser(guruTask.Account);
                //2.验证用户信息
                if (pnUser.Id == Guid.Empty)
                {
                    pnUser = new PnUserDto();
                    pnUser.Account = guruTask.Account;
                    pnUser.Password = guruTask.Password;
                    await WebServer.PnUser(pnUser);
                    pnUser = await WebServer.GetUser(guruTask.Account);
                }
                return pnUser;
            }

            async Task<Propnex.Poster.PropertyGuru.Mobile.Dto.Token> auth()
            {
                var _auth = new Auth();
                var loginResult = await _auth.LoginAsync(new AuthLogin()
                {
                    UserName = guruTask.Account,
                    Password = guruTask.Password
                });
                if (loginResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _Token = loginResult.Data;
                    pnUser.TokenJson = Newtonsoft.Json.JsonConvert.SerializeObject(_Token);
                    await WebServer.UpdatePnUserToken(pnUser);
                    return loginResult.Data;
                }
                else
                {
                    //登陆失败
                    return null;
                }
            }
            async Task<Token> checkToken()
            {
                var token = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(pnUser.TokenJson);
                if (DateTime.Parse(token.accessTokenExpiresAt).AddHours(-1) < DateTime.Now)
                {
                    return await auth();
                }
                listings = await getListing();
                if (listings == null)
                    return await auth();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(pnUser.TokenJson);
            }


            async Task<List<ListingsListing>> getListing()
            {
                var token = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(pnUser.TokenJson);
                var mobile = new Mobile() { Token = token };
                var result = await mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString()));
                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    foreach (var item in result.Data.listings)
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
                        ListingInfos.Add(info);
                    }
                    return result.Data.listings;
                }
                return null;
            }
            return _Token;
        }
        private List<ListingsListing> listings = null;

        private async Task<GuruTasks> getGuruTasks()
        {
            string context = "";
            taskDto = await WebServer.GetTask();
            //var taskDto = new PnTaskDto()
            //{
            //    Id = Guid.Parse("3a096f11-6583-7283-5eea-693372dab84c"),
            //    Number = "881997.guru.tsk"
            //};

            if (taskDto != null)
            {
                context = await WebServer.GetTaskContent(taskDto);
                var lenght = context.IndexOf("Xpressor-Listing-File===");
                var taskContext = context.Substring(0, lenght == -1 ? context.Length : lenght);
                return new GuruTasks(context, taskContext);
            }
            else
            {
                return null;
            }
        }

        private async Task ResultUpload(GuruTask guruTask, GuruTaskListing taskListing, string queue_id, string listing_id, string status = "Done", string memo = "")
        {
            Log($"result upload queue_id is {queue_id},listing_id is {listing_id} ,status is {status},memo is {memo}");

            if (guruTask.Source.ToLower() == "chope")
            {
                await chopeItem(guruTask, queue_id, listing_id, status, memo);
            }
            else
            {
                await xwebItem(guruTask, taskListing, 0, status, memo);
            }
        }

        private async Task chopeItem(GuruTask guruTask, string queue_id, string listing_id, string status = "Done", string memo = "")
        {
            StringBuilder sbUrl = new StringBuilder("https://pa-production.propnex.net/index.php/pnapi/updateChopeTask?" +
           "super=1&" +
           $"queue_id={queue_id}&" +
           $"portal_id={listing_id}&");
            if (string.IsNullOrEmpty(listing_id) == false && status == "Done")
            {
                sbUrl.Append($"portal_link=https://www.propertyguru.com.sg/listing/{listing_id}&");
            }
            else
            {
                sbUrl.Append($"portal_link=&");
            }
            sbUrl.Append($"account_id={guruTask.AccountId}&portal=GURU&" +
                $"action={guruTask.TaskType}&" +
                $"status={status}&" +
                $"tm={unix_timestamp(DateTime.Now)}&" +
                $"memo={memo}");

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var res = await sbUrl.ToString().GetStringAsync();
                    //var res = webClient.DownloadString(sbUrl.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, $"upload result error {ex.Message}");
                    await WebServer.PingAsync();
                }
            }

        }

        private async Task xwebItem(GuruTask guruTask, GuruTaskListing taskListing, int time_cost = 0, string status = "Done", string note = "")
        {
            var net = true;
            for (int i = 0; i < 3; i++)
            {
                StringBuilder formData = new StringBuilder();
                Dictionary<string, string> data = new Dictionary<string, string>();
                formData.Append($"account_name={guruTask.Account}&");
                data.Add("account_name", guruTask.Account);

                formData.Append($"account_password={guruTask.Password}&");
                data.Add("account_password", guruTask.Password);

                formData.Append($"task_id={guruTask.Id}&");
                data.Add("task_id", guruTask.Id);

                formData.Append($"taskitem_id={taskListing.TaskItemId}&");
                data.Add("taskitem_id", taskListing.TaskItemId);

                formData.Append($"status={status}&");
                data.Add("status", status);

                formData.Append($"time_cost={time_cost}&");
                data.Add("time_cost", time_cost.ToString());

                formData.Append($"taskitem_note={note}&");
                data.Add("taskitem_note", note);

                if (taskListing.Listing.Id.HasValue && status == "Done")
                {
                    formData.Append($"portal_link=https://www.propertyguru.com.sg/listing/{taskListing.Listing.Id}&");
                    data.Add("portal_link", $"https://www.propertyguru.com.sg/listing/{taskListing.Listing.Id}");
                }
                else
                {
                    formData.Append($"portal_link=&");
                    data.Add("portal_link", "");
                }
                formData.Append($"listing_version={taskListing.UpdateTime}&");
                data.Add("listing_version", taskListing.UpdateTime);

                formData.Append("poster=mobileApi");
                data.Add("poster", "mobileApi");
                System.Net.Http.StringContent stringContent = new System.Net.Http.StringContent(formData.ToString());

                //new
                //{
                //    account_name = guruTask.Account,
                //    account_password = guruTask.Password,
                //    task_id = guruTask.Id,
                //    taskitem_id = taskListing.TaskItemId,
                //    status = status,
                //    time_cost = time_cost.ToString(),
                //    taskitem_note = note,
                //    portal_link = "",
                //    listing_version = taskListing.UpdateTime,
                //    poster = "cef"
                //}

                try
                {
                    var result = await "https://pa-production.propnex.net/index.php/tasks/updateStatus".PostUrlEncodedAsync(formData.ToString());
                    var s = await result.GetStringAsync();
                    net = false;
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, $"upload result error {ex.Message}");
                    await WebServer.PingAsync();
                }
            }
        }

        private async Task XwebEnd(GuruTask guruTask, string note = "")
        {
            if (guruTask.Source.ToLower() == "chope")
                return;

            StringBuilder formData = new StringBuilder();
            formData.Append($"account_name={guruTask.Account}&");
            formData.Append($"account_password={guruTask.Password}&");
            formData.Append($"task_id={guruTask.Id}&");
            formData.Append($"status=Done&");
            formData.Append($"time_cost=&");
            formData.Append($"note={note}&");
            formData.Append("poster=selenium");

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var result = await "https://pa-production.propnex.net/index.php/tasks/updateStatus".PostUrlEncodedAsync(formData.ToString());
                    var s = await result.GetStringAsync();
                    _logger.Information($"Xweb end success");
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, $"Xweb end upload result error {ex.Message}");
                    await WebServer.PingAsync();
                }
            }
        }

        private async Task End(GuruTask guruTask, string queue_id)
        {
            if (guruTask.Source.ToLower() == "chope")
            {
                string url = "https://pa-production.propnex.net/index.php/pnapi/updateChopeTask?" +
    $"super=1&queue_id={queue_id}&portal=GURU&&memo=&tm={unix_timestamp(DateTime.Now)}";
                for (int i = 0; i < 3; i++)
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        try
                        {
                            await url.GetStringAsync();
                            _logger?.Information($"chope end success");
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger?.Error(ex, $"chope end upload result error {ex.Message}");
                            await WebServer.PingAsync();
                        }
                    }
                }

            }
        }

        public long unix_timestamp(DateTime dt)
        {
            TimeSpan unix_time = (dt.Date - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)unix_time.TotalSeconds;
        }

        private void Log(string message)
        {
            MessageEvent?.Invoke(message);
            _logger?.Information(message);
        }
    }
}
