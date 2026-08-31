using Flurl.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Propnex;
using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Listing.V3;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using Propnex.Poster.PropertyGuru.Mobile.Model;
using Propnex.Poster.PropertyGuru.Tasks;
using Propnex.Poster.Share;
using Serilog;
using Serilog.Core;
using SlackBotMessages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace PropnexPoster.WPF
{

    public class PosterRunInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string taskNumber;
        private string account;
        private string agentId;
        private string taskType;
        private int listingCount = 0;
        private string taskItemId;
        private DateTime? startTime;

        public string TaskNumber
        {
            get => taskNumber;
            set { taskNumber = value; OnPropertyChanged(nameof(TaskNumber)); }
        }

        public string StartTime
        {
            get => startTime.HasValue ? $"StartTime: {startTime:yyyy-MM-dd HH:mm:ss}" : "";
            set { startTime = string.IsNullOrEmpty(value) ? (DateTime?)null : DateTime.Parse(value); OnPropertyChanged(nameof(StartTime)); }
        }

        public string Account
        {
            get => string.IsNullOrEmpty(account) ? "" : $"Account: {account}";
            set { account = value; OnPropertyChanged(nameof(Account)); }
        }

        public string AgentId
        {
            get => string.IsNullOrEmpty(agentId) ? "" : $"AgentId: {agentId}";
            set { agentId = value; OnPropertyChanged(nameof(AgentId)); }
        }

        public string TaskType
        {
            get => string.IsNullOrEmpty(agentId) ? "" : $"TaskType: {taskType}";
            set { taskType = value; OnPropertyChanged(nameof(TaskType)); }
        }

        public string TaskItemId
        {
            get => string.IsNullOrEmpty(taskItemId) ? "" : $"ListingNumber: {taskItemId}";
            set { taskItemId = value; OnPropertyChanged(nameof(TaskItemId)); }
        }

        public string ListingCount
        {
            get => listingCount == 0 ? "" : $"ListingCount: {listingCount}";
            set { listingCount = int.Parse(value); OnPropertyChanged(nameof(ListingCount)); }
        }
    }

    public class PosterRun : Volo.Abp.DependencyInjection.ITransientDependency
    {

        public Action<string, bool> MessageEvent { get; set; }

        public PosterRunInfo PosterRunInfo { get; set; } = new PosterRunInfo();

        private ILogger? _logger;

        public ILogger<PosterRun> globleLogger { get; set; }

        private PnTaskDto taskDto;

        public async Task Run()
        {
            var posterRunInfo = PosterRunInfo;
            posterRunInfo.TaskNumber = "Get Task ....";
            await WebServer.PosterPing();
            Log("Get Task .....");
            //1.获取任务信息
#if DEBUG
            taskDto = new PnTaskDto()
            {
                Number = "cp17881470235834.guru.tsk"
            };
            var context = await File.ReadAllTextAsync($"E:\\{taskDto.Number}");
            var lenght = context.IndexOf("Xpressor-Listing-File===");
            var taskContext = context.Substring(0, lenght == -1 ? context.Length : lenght);
            var guruTasks = new GuruTasks(context, taskContext);
#else
            var guruTasks = await getGuruTasks();
#endif
            if (guruTasks == null)
            {
                Log("Not find task ,delay 1 min");
                await Task.Delay(6000 * 10);
                //await Task.Delay(1000);
                return;
            }
            posterRunInfo.TaskNumber = taskDto.Number;
            posterRunInfo.StartTime = DateTime.Now.ToString();
            Log($"Get Task success,{taskDto.Number}");
            //2.生成日志

            try
            {
                _logger = new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .WriteTo.Async(c => c.File($"{Directory.GetDirectoryRoot(System.AppDomain.CurrentDomain.BaseDirectory)}\\logs\\task\\{taskDto.Number}.txt"))
                  .CreateLogger();
                var proxyIp = "";
                if (WPFModule.AppConfiguration.IsProxy)
                {
                    proxyIp = WPFModule.AppConfiguration.GetProxy();
                }

                Log($"Use proxyIp:{proxyIp}");
                //4.处理任务
                for (int i = 0; i < guruTasks.Tasks.Count; i++)
                {
                    try
                    {

                        //1.获取用户信息
                        var task = guruTasks.Tasks[i];
                        //3.登陆
                        Log("Get Token .......");
                        var token = await Login(task, proxyIp);

                        if (token == null)
                        {
                            if (task.Listings.Listings != null)
                            {
                                foreach (var listing in task.Listings.Listings)
                                {
                                    await ResultUpload(task, listing, listing.TaskItemId, "", "Failed", "Login Faile ,Please check password or account info");
                                    await End(task, listing.TaskItemId);
                                }
                            }
                            await XwebEnd(task, status: "Failed", note: "Login Faile ,Please check password or account info");
                            continue;
                        }
                        posterRunInfo.Account = task.Account;
                        posterRunInfo.AgentId = token.User.AgentId.ToString();
                        posterRunInfo.TaskType = task.TaskType;
                        posterRunInfo.ListingCount = ListingInfos.Count.ToString();
                        Log("Token success");

                        Log($"{task.TaskType.ToLower()}");

                        Api _api;
                        ProjectsApi _projectsApi;
                        AdsProduct _adsProject;
                        Mobile _mobile;
                        Agent _agent;
                        WrapperListingSg _wrapperListingSg;
                        if (WPFModule.AppConfiguration.IsProxy)
                        {
                            _api = new Api(token, proxyIp) { Log = Log1 };
                            _projectsApi = new ProjectsApi(token, proxyIp) { Log = Log1 };
                            _adsProject = new AdsProduct(token, proxyIp) { Log = Log1 };
                            _mobile = new Mobile(token, proxyIp) { Log = Log1 };
                            _agent = new Agent(token, proxyIp) { Log = Log1 };
                            _wrapperListingSg = new WrapperListingSg(token, proxyIp) { Log = Log1 }
                            ;
                        }
                        else
                        {
                            _api = new Api(token) { Log = Log1 };
                            _projectsApi = new ProjectsApi(token) { Log = Log1 };
                            _adsProject = new AdsProduct(token) { Log = Log1 };
                            _mobile = new Mobile(token) { Log = Log1 };
                            _agent = new Agent(token) { Log = Log1 };
                            _wrapperListingSg = new WrapperListingSg(token) { Log = Log1 };
                        }

                        //await _mobile.Dashboard(token.User.AgentId.ToString());
                        //4.执行操作
                        if (task.TaskType.ToLower() == "post only")
                        {
                            foreach (var listing in task.Listings.Listings)
                            {
                                posterRunInfo.TaskItemId = listing.TaskItemId.ToString();

                                if (await CreateListingAndPublishAsync(task, listing, _api, _agent, _wrapperListingSg))
                                {
                                    continue;
                                }
                                await End(task, listing.TaskItemId);
                            }
                            await XwebEnd(task);
                        }

                        if (task.TaskType.ToLower() == "repost")
                        {
                            foreach (var listing in task.Listings.Listings)
                            {
                                posterRunInfo.TaskItemId = listing.TaskItemId.ToString();

                                var listInfo = IsExtis(task, listing);
                                if (listInfo != null)
                                {
                                    //match task
                                    var taskListing = await _api.GetListing(listing.Listing.Id.Value);

                                    if (listing.FastRepost == "0")
                                    {
                                        if (!await UpdateExistingListingMediaAsync(task, listing, _api, _mobile, taskListing.Data))
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Log("FastRepost");
                                    }
                                    //Repost
                                    await _adsProject.Repost(taskListing.Data.id.Value, listInfo.RepostCharge);
                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());

                                }
                                else
                                {
                                    //Post Only：找不到已有 listing 时，与"post only"任务类型共用同一套 v3 创建+上架流程
                                    if (await CreateListingAndPublishAsync(task, listing, _api, _agent, _wrapperListingSg))
                                    {
                                        continue;
                                    }
                                }

                                await End(task, listing.TaskItemId);
                            }
                            await XwebEnd(task);
                        }

                        if (task.TaskType.ToLower() == "update")
                        {
                            foreach (var listing in task.Listings.Listings)
                            {
                                posterRunInfo.TaskItemId = listing.TaskItemId.ToString();

                                if (IsExtis(task, listing) != null)
                                {
                                    //更新任务 UpdateTask

                                    // get listing detial
                                    var taskListing = await _api.GetListing(listing.Listing.Id.Value);

                                    if (!await UpdateExistingListingMediaAsync(task, listing, _api, _mobile, taskListing.Data))
                                    {
                                        continue;
                                    }

                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                                }
                                else
                                {
                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "Update listing,but Not match listing");
                                }

                                await End(task, listing.TaskItemId);
                            }
                            await XwebEnd(task);
                        }

                        //remove from portals
                        if (task.TaskType.ToLower() == "remove from portals")
                        {
                            foreach (var listing in task.Listings.Listings)
                            {
                                posterRunInfo.TaskItemId = listing.TaskItemId.ToString();
                                var listingInfo = IsExtis(task, listing);
                                if (listingInfo != null)
                                {
                                    var ids = new List<int>();
                                    ids.Add(listingInfo.Id);
                                    await _mobile.DeleteListing(ids);
                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                                }
                                else
                                {
                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "Not find listing ");
                                }
                                await End(task, listing.TaskItemId);
                            }
                            await XwebEnd(task);
                        }

                        if (task.TaskType.ToLower().IndexOf("retrieve") > -1)
                        {
                            foreach (var listing in listings)
                            {
                                try
                                {
                                    var guruListing = await _api.GetListing(listing.id.Value);
                                    var postActionResult = new PosterActionResult()
                                    {
                                        Status = PosterActionResultStatus.Success
                                    };
                                    try
                                    {
                                        _logger.Information("Retrieve");
                                        var url = "http://3.0.87.74/propnex/index.php/";
                                        //var guruListing = await this.getListing(task.Id.ToString());
                                        var retrieveListing = await RetrieveListing.Converter(guruListing.Data, task.Account, task.TargetPortal, task.Id);
                                        retrieveListing.Account = task.Account;
                                        var result = RetrieveListing.GetData(retrieveListing, task.Account, task.Password, task.Id);
                                        if (result.Item2)
                                        {
                                            var data = result.Item1;

                                            FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(data);

                                            HttpClient httpClient = new HttpClient();
                                            var ok = await httpClient.PostAsync($"{url}listings/post", formUrlEncodedContent);
                                            var httpResult = await ok.Content.ReadAsStringAsync();
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
                                                        }
                                                        ;
                                                        if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[1].Trim()))
                                                        {
                                                            data[formName + "[title]"] = vnames[1].Trim();
                                                        }
                                                    }
                                                    await PostXpressorAttachmentAsync(httpClient, url, xpid, formName, data, p);
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
                                                            }
                                                            ;
                                                            if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[1].Trim()))
                                                            {
                                                                data[formName + "[title]"] = vnames[1].Trim();
                                                            }
                                                            await PostXpressorAttachmentAsync(httpClient, url, xpid, formName, data, p);
                                                        }
                                                    }
                                                }

                                                string[] tours = retrieveListing.Tours.Split(new string[] { "\n" }, StringSplitOptions.None);
                                                string[] toursThumbnail = retrieveListing.TourThumbnails.Split(new string[] { "\n" }, StringSplitOptions.None);
                                                //i = 0;
                                                for (int j = 0; j < tours.Length; j++)
                                                {
                                                    string tour = tours[j].Trim();
                                                    if (string.IsNullOrEmpty(tour)) continue;
                                                    string tourThumbnail = toursThumbnail[j].Trim();
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
                                                    await PostXpressorAttachmentAsync(httpClient, url, xpid, formName, data, p);
                                                }

                                                string[] videos = retrieveListing.Videos.Split(new string[] { "\n" }, StringSplitOptions.None);
                                                string[] videosThumbnail = retrieveListing.VideoThumbnails.Split(new string[] { "\n" }, StringSplitOptions.None);
                                                //i = 0;
                                                for (int j = 0; j < videos.Length; j++)
                                                {
                                                    string video = videos[j];
                                                    if (string.IsNullOrEmpty(video)) continue;
                                                    string videoThumbnail = videosThumbnail.Length > j ? videosThumbnail[j] : "";
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
                                                        }
                                                        ;
                                                    }
                                                    else
                                                    {
                                                        data[formName + "[embed_code]"] = p;
                                                    }
                                                    ;
                                                    data["xpressor"] = "";

                                                    if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[vnames.Length - 1].Trim()))
                                                    {
                                                        data[formName + "[title]"] = vnames[vnames.Length - 1].Trim();
                                                    }
                                                    ;

                                                    await PostXpressorAttachmentAsync(httpClient, url, xpid, formName, data, p);
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
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error("Retrieve Error {0},{1}", listing.id, ex);
                                }
                            }
                            await XwebEnd(task);
                        }

                        _api.Dispose();
                        _projectsApi.Dispose();
                        _adsProject.Dispose();
                        _mobile.Dispose();
                        _agent.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("{0},{1}", ex.Message, ex.StackTrace);
                        if (taskDto?.Id is Guid innerTaskId && innerTaskId != Guid.Empty)
                        {
                            await WebServer.LogErrorAsync(innerTaskId, $"{ex.Message}\n{ex.StackTrace}");
                        }
                    }
                    finally
                    {

                    }
                }

                await (_logger as Logger).DisposeAsync();
                //5.
            }
            catch (Exception ex)
            {
                globleLogger.LogError(ex.Message, ex);
                if (taskDto?.Id is Guid taskId && taskId != Guid.Empty)
                {
                    await WebServer.ResetTask(taskId, ex.Message);
                    await WebServer.LogErrorAsync(taskId, $"{ex.Message}\n{ex.StackTrace}");
                }
            }
            finally
            {
                this.globleLogger = null;
                this.MessageEvent = null;
            }

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

        // ─────────────────────────────────────────────────────────────────────
        // 发布任务类型 (TaskType) 一览：
        //   post only            走 v3 新建流程：校验项目信息(AgnetProject) → Agent.CreateListingAsync 建 listing → 上传媒体 → WrapperListingSg.Offerings/Publish 上架
        //   repost                已存在匹配 listing：（非 FastRepost 时）先用 v3/v2 更新并重传媒体，再 AdsProduct.Repost；不存在则按 post only 的 v3 流程新建
        //   update                已存在匹配 listing：更新字段与媒体（走 _api 的 v2 GetListing/UpdateAsync）
        //   remove from portals   已存在匹配 listing：调用 DeleteListing 下架
        //   retrieve*             拉取 listing 详情并转发给 Xpressor（第三方系统），与上面几类走完全不同的上传通道
        // 新建/更新流程都共用同一套媒体上传（图片/视频/全景/平面图），post only 与 repost 的"找不到已有 listing"分支共用同一套 v3 创建+上架逻辑，见下方 UploadAllMediaAsync / CreateListingAndPublishAsync。
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>依次上传图片、视频、全景看房、平面图；任一步失败即上报失败结果并通知 Slack，返回 false。</summary>
        private async Task<bool> UploadAllMediaAsync(GuruTask task, GuruTaskListing listing, Api _api)
        {
            return await uploadStep(uploadPhotosAsync, "upload photo error")
                && await uploadStep(uploadVideos, "upload video error")
                && await uploadStep(uploadVirtualTours, "upload vt error")
                && await uploadStep(uploadFloorPlanAsync, "upload floor plan error");

            async Task<bool> uploadStep(Func<GuruTaskListing, Api, Task<HttpResult<string>>> upload, string errorMessage)
            {
                if ((await upload(listing, _api)).HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return true;
                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", errorMessage);
                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} {errorMessage}  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                return false;
            }
        }

        /// <summary>
        /// 新建 listing 并走 v3 发布流程（校验项目信息 → 创建 → 上传媒体 → Offerings/Publish 上架）。
        /// post only、以及 repost 在找不到已有匹配 listing 时，都共用这一套流程。
        /// 返回 true 仅代表"媒体上传失败"，调用方应据此 continue 以跳过本次 End 上报（与原逻辑保持一致）；其余失败分支已自行上报结果，返回 false 即可继续走 End。
        /// </summary>
        private async Task<bool> CreateListingAndPublishAsync(GuruTask task, GuruTaskListing listing, Api _api, Agent _agent, WrapperListingSg _wrapperListingSg)
        {
            if (listing.ListingV3.Project.MetaByType.Verified != null)
            {
                //get project information to confirm the location id, otherwise it will cause the error of "The location is invalid" when posting.
                var projectResult = await _agent.AgnetProject(int.Parse(listing.ListingV3.Project.MetaByType.Verified.Id));
                if (projectResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var project = projectResult.Data;
                    //update project id to NanoId
                    listing.ListingV3.Project.MetaByType.Verified.Id = project.NanoId;
                    //update location id to location id, match by externalId and replace with the ID.
                    var address = project.Addresses.Where(q => q.ExternalId == listing.ListingV3.Project.MetaByType.Verified.LocationId.ToString()).FirstOrDefault();
                    listing.ListingV3.Project.MetaByType.Verified.LocationId = address != null ? address.Id : project.Addresses[0].Id;
                }
            }

            var result = await _agent.CreateListingAsync(listing.ListingV3);
            if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                if (!result.Message.Contains("Postal code is already being used"))
                {
                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", result.Message);
                    await SlackBotMessage.SendAsync($"{task.Id}-{listing.TaskItemId}-{listing.Listing.Id} {result.Message}");
                }
                return false;
            }

            listing.Listing.Id = result.Data.Id;
            if (result.Data.Id == 0)
                return false;

            if (!await UploadAllMediaAsync(task, listing, _api))
                return true;

            var taskListing = await _wrapperListingSg.Listings(listing.Listing.Id.Value);
            if (taskListing.Data == null)
            {
                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "not find listing in draf");
                return false;
            }

            var offerings = await _wrapperListingSg.Offerings(listing.Listing.Id.ToString());
            if (offerings.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", offerings.Message);
                return false;
            }

            var publish = await _wrapperListingSg.Publish(new CreditKey() { Key = offerings.Data.Products[0].Key, Brand = "pg" }, listing.Listing.Id.Value.ToString());
            if (publish.HttpStatusCode == System.Net.HttpStatusCode.OK)
                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
            else
                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", publish.Message);
            return false;
        }

        /// <summary>
        /// 更新一个已存在 listing 的字段并重传媒体（repost 命中已有 listing、update 任务类型共用），
        /// 按 listing 的 version 分流到 v2/v3 各自的更新实现。
        /// v3 的更新逻辑还没做，暂时先复用 UpdateExistingListingMediaV2Async 占位，后面单独实现 v3 时
        /// 只需要把 v3 分支换成真正的 UpdateExistingListingMediaV3Async。
        /// </summary>
        private async Task<bool> UpdateExistingListingMediaAsync(GuruTask task, GuruTaskListing listing, Api _api, Mobile _mobile, CreateOrUpdateListing taskListingData)
        {
            if (taskListingData.version == "v3")
            {
                // TODO: v3 更新逻辑还没实现，暂时复用 v2 的逻辑
                return await UpdateExistingListingMediaV2Async(task, listing, _api, _mobile, taskListingData);
            }
            else
            {
                return await UpdateExistingListingMediaV2Async(task, listing, _api, _mobile, taskListingData);
            }
        }

        /// <summary>
        /// v2 版本的"更新已有 listing 并重传媒体"：替换字段 → UpdateAsync → 删光旧媒体 →
        /// 依次重传图片/视频/全景/平面图 → 再 GetListing 一次让 PG 刷新数据。
        /// 返回 false 代表媒体上传失败（UploadAllMediaAsync 已自行上报结果并通知 Slack），调用方应据此 continue 跳过后续步骤。
        /// </summary>
        private async Task<bool> UpdateExistingListingMediaV2Async(GuruTask task, GuruTaskListing listing, Api _api, Mobile _mobile, CreateOrUpdateListing taskListingData)
        {
            //replace listing
            taskListingData.Update(listing.Listing);
            taskListingData.isLiveTourAvailable = true;

            //update listing
            await _api.UpdateAsync(taskListingData);

            await _mobile.DeleteMediaAll(taskListingData);
            if (!await UploadAllMediaAsync(task, listing, _api))
                return false;

            await _api.GetListing(listing.Listing.Id.Value);
            return true;
        }

        /// <summary>把一个附件（图片/平面图/视频/全景）以 multipart 表单的形式上传给 Xpressor（retrieve 任务专用，与上面的 PropertyGuru 上传通道完全独立）。</summary>
        private async Task PostXpressorAttachmentAsync(HttpClient httpClient, string url, string xpid, string formName, Dictionary<string, string> data, string filePath)
        {
            try
            {
                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    foreach (var content in data)
                    {
                        multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                    }
                    using (var stream = new StreamContent(new System.IO.FileStream(filePath, System.IO.FileMode.Open)))
                    {
                        multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(filePath));
                        var ok = await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                        await ok.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                //DoProgress("Exception in PostWebPageMultipart", -1, "");
                //throw;
            }
        }

        private async Task<HttpResult<string>> uploadPhotosAsync(GuruTaskListing guruTaskListing, Api _api)
        {
            HttpResult<string> result = new HttpResult<string>() { HttpStatusCode = HttpStatusCode.BadRequest };
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            if (guruTaskListing.Photos.Count == 0)
                return new HttpResult<string>() { HttpStatusCode = System.Net.HttpStatusCode.OK };
            for (int i = 0; i < guruTaskListing.Photos.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;
                var title = "";
                if (guruTaskListing.Photos[i].Split("#").Length > 1)
                    title = guruTaskListing.Photos[i].Split("#")[1];
                var filePath = $"{path}{i}_image{GetExtensionFromUrl(guruTaskListing.Photos[i])}";
                try
                {
                    Log($"download photo {filePath}");
                    if (File.Exists(filePath))
                    {
                        Log($"exists");
                        File.Delete(filePath);
                        Log("delete");
                        if (File.Exists(filePath) == false)
                        {
                            Log("delete success");
                        }
                        else
                        {
                            Log("delete error");
                        }
                    }
                    //DownClient webClient = new DownClient();
                    //webClient.DownloadFile(guruTaskListing.Photos[i], filePath);
                    if (await _downLoadFile(guruTaskListing.Photos[i], filePath) == false)
                    {
                        break;
                    }
                    Log("download photo complete");
                    result = await _api.UploadPhotoAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);
                    if (result.HttpStatusCode != System.Net.HttpStatusCode.OK && result.HttpStatusCode != HttpStatusCode.BadRequest)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }
            }
            return result;
        }

        public string GetExtensionFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            try
            {
                var uri = new Uri(url);
                // AbsolutePath 会去掉 query/fragment，再用 Path.GetExtension 取得扩展名
                return Path.GetExtension(uri.AbsolutePath) ?? string.Empty;
            }
            catch
            {
                // 如果不是合法 URI，则退回到简单字符串处理（去掉 query/fragment 后取最后一个点）
                var clean = url.Split(new[] { '?', '#' })[0];
                var idx = clean.LastIndexOf('.');
                return (idx >= 0) ? clean.Substring(idx) : string.Empty;
            }
        }

        private async Task<HttpResult<string>> uploadVideos(GuruTaskListing guruTaskListing, Api _api)
        {
            HttpResult<string> result = new HttpResult<string>() { HttpStatusCode = HttpStatusCode.BadRequest };
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            if (guruTaskListing.Videos.Count == 0)
                return new HttpResult<string>() { HttpStatusCode = System.Net.HttpStatusCode.OK };
            for (int i = 0; i < guruTaskListing.Videos.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;

                var url = guruTaskListing.Videos[i].ToLower();
                var title = "";
                if (guruTaskListing.Videos[i].Split("#").Length > 1)
                    title = guruTaskListing.Videos[i].Split("#")[1];
                var filePath = $"{path}{i}_movie{GetExtensionFromUrl(url)}";
                if (url.Contains("youtube") ||
                    url.Contains("youtu.be") ||
                    url.Contains("youtube.com") ||
                    url.Contains("vimeo") ||
                    url.Contains("dailymotion") ||
                    url.Contains("<iframe") ||
                    url.Contains("havelock2") ||
                    url.Contains("8prop.com") ||
                    url.Contains("matterport.com") ||
                    url.Contains("tubear") ||
                    url.Contains("beyond.3dnest.cn") ||
                    url.Contains("mixgo.com") ||
                    url.Contains("tiktok.com") ||
                    url.Contains("kuula.co") ||
                    url.Contains("virtualtours") ||
                    url.Contains("singaporeluxuryhouse")
                    )
                {
                    filePath = guruTaskListing.Videos[i];
                    filePath = System.Web.HttpUtility.UrlDecode(filePath);
                }
                else
                {
                    int reTry = 0;
                    try
                    {

                        Log($"download move {filePath}");
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        //DownClient webClient = new DownClient();
                        //webClient.DownloadFile(guruTaskListing.Videos[i], filePath);
                        if (await _downLoadFile(guruTaskListing.Videos[i], filePath) == false)
                        {
                            break;
                        }
                        Log("download move complete");
                        //result = await _api.UploadVideosAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message);
                    }
                    if (System.IO.File.Exists(filePath) == false)
                    {
                        continue;
                    }
                }
                try
                {
                    result = await _api.UploadVideosAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);

                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }

                if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    break;
                }
                Log("upload mov success");
            }
            return result;
        }

        private async Task<HttpResult<string>> uploadVirtualTours(GuruTaskListing guruTaskListing, Api _api)
        {
            HttpResult<string> result = new HttpResult<string>() { HttpStatusCode = HttpStatusCode.BadRequest };
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            if (guruTaskListing.Tours.Count == 0)
                return new HttpResult<string>() { HttpStatusCode = System.Net.HttpStatusCode.OK };
            for (int i = 0; i < guruTaskListing.Tours.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;

                var url = guruTaskListing.Tours[i].ToLower();
                var title = "";
                if (guruTaskListing.Tours[i].Split("#").Length > 1)
                    title = guruTaskListing.Tours[i].Split("#")[1];
                var filePath = $"{path}{i}_vt{GetExtensionFromUrl(url)}";
                if (url.Contains("youtube") ||
                    url.Contains("youtu.be") ||
                    url.Contains("vimeo") ||
                    url.Contains("dailymotion") ||
                    url.Contains("<iframe") ||
                    url.Contains("havelock2") ||
                    url.Contains("8prop.com") ||
                    url.Contains("matterport.com") ||
                    url.Contains("tubear") ||
                    url.Contains("beyond.3dnest.cn") ||
                    url.Contains("mixgo.com") ||
                    url.Contains("tiktok.com") ||
                    url.Contains("kuula.co") ||
                    url.Contains("virtualtours") ||
                    url.Contains("singaporeluxuryhouse")
                    )
                {
                    filePath = guruTaskListing.Tours[i];
                    filePath = System.Web.HttpUtility.UrlDecode(filePath);
                }
                else
                {
                    try
                    {
                        Log($"download tour {filePath}");
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        //DownClient webClient = new DownClient();
                        //webClient.DownloadFile(guruTaskListing.Tours[i], filePath);
                        if (await _downLoadFile(guruTaskListing.Tours[i], filePath) == false)
                        {
                            break;
                        }
                        Log("download tour complete");
                        //result = await _api.UplaodVirtualTours($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message); filePath = "";
                    }
                }
                try
                {
                    result = await _api.UploadVideosAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);

                }
                catch { }
                if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    break;
                }
                Log("upload tour succes");
            }

            return result;
        }

        private async Task<HttpResult<string>> uploadFloorPlanAsync(GuruTaskListing guruTaskListing, Api _api)
        {
            HttpResult<string> result = new HttpResult<string>() { HttpStatusCode = HttpStatusCode.BadRequest };
            var taskId = guruTaskListing.Id.ToString();
            var path = checkFileDirectory(taskId);
            if (guruTaskListing.FloorPlan.Count == 0)
                return new HttpResult<string>() { HttpStatusCode = System.Net.HttpStatusCode.OK };
            for (int i = 0; i < guruTaskListing.FloorPlan.Count; i++)
            {
                // max upload photos
                if (i == 20)
                    break;
                var title = "";
                if (guruTaskListing.FloorPlan[i].Split("#").Length > 1)
                    title = guruTaskListing.FloorPlan[i].Split("#")[1];
                var filePath = $"{path}{i}_fp{GetExtensionFromUrl(guruTaskListing.FloorPlan[i])}";
                try
                {
                    //await guruTaskListing.FloorPlan[i].DownloadFileAsync(path, $"{i}_fp.jpg");

                    Log("download FloorPlan");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    //DownClient webClient = new DownClient();
                    //webClient.DownloadFile(guruTaskListing.FloorPlan[i], filePath);
                    if (await _downLoadFile(guruTaskListing.FloorPlan[i], filePath) == false)
                    {
                        break;
                    }
                    Log("download FloorPlan complete");
                    result = await _api.UploadFlooplan($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);
                    if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }
            }
            return result;
        }

        private async Task<bool> _downLoadFile(string url, string filePath)
        {
            //Start:
            //    int reTry = 0;
            //    try
            //    {
            //        DownClient webClient = new DownClient();
            //        webClient.DownloadFile(url, filePath);
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        Log(ex.Message);
            //        if (reTry < 3)
            //            goto Start;
            //    }
            //    return false;

            return await FileDownloader.DownloadFileAsync(url, filePath, maxAttempts: 3, new Progress<double>(p =>
            {
                if (p < 0) Log("Downloading... size unknown");
                else Log($"FilePath {filePath} Progress: {p:P1}", true, false);
            }), TimeSpan.FromSeconds(5), null);
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

        private async Task<Propnex.Poster.PropertyGuru.Mobile.Dto.Token> Login(GuruTask guruTask, string proxyIp = "")
        {
            ClientBase.PhoneModel = PhoneModelList.GetPhoneModel();
            var pnUser = await getUser();
            var _Token = string.IsNullOrEmpty(pnUser.TokenJson) ?
                await auth() :
                await checkToken();
            if (_Token == null)
                return null;
            ClientBase.PhoneModel = pnUser.PhoneModel == "" ? PhoneModelList.GetPhoneModel() : ClientBase.PhoneModel;
            if (ClientBase.PhoneModel.Length < 20)
            {
                ClientBase.PhoneModel += $";{Guid.NewGuid()}";
            }
            await getListing();


            async Task<PnUserDto> getUser()
            {
                Log("get user ....");
                var pnUser = await WebServer.GetUser(guruTask.Account);

                //2.验证用户信息
                if (pnUser.Id == Guid.Empty)
                {
                    Log("not find user ");
                    pnUser = new PnUserDto();
                    pnUser.Account = guruTask.Account;
                    pnUser.Password = guruTask.Password;
                    Log("insert user ....");
                    pnUser.PhoneModel = ClientBase.PhoneModel;
                    if (pnUser.PhoneModel.Length < 20)
                    {
                        pnUser.PhoneModel += $";{Guid.NewGuid()}";
                    }
                    await WebServer.PnUser(pnUser);
                    pnUser = await WebServer.GetUser(guruTask.Account);

                }
                Log("user success .");
                return pnUser;
            }

            async Task<Propnex.Poster.PropertyGuru.Mobile.Dto.Token> auth()
            {
                Auth _auth;
                if (string.IsNullOrEmpty(proxyIp) == false)
                {
                    _auth = new Auth(proxyIp) { Log = Log1 };
                }
                else
                {
                    _auth = new Auth() { Log = Log1 };
                }
                Log("Login ....");
                var loginResult = await _auth.LoginAsync(new AuthLogin()
                {
                    UserName = guruTask.Account,
                    Password = guruTask.Password
                });
                if (loginResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    Log("Login success .");
                    _Token = loginResult.Data;
                    Log("Token :" + _Token.accessToken);
                    pnUser.TokenJson = Newtonsoft.Json.JsonConvert.SerializeObject(_Token);
                    pnUser.PhoneModel = string.IsNullOrEmpty(pnUser.PhoneModel) ? ClientBase.PhoneModel : pnUser.PhoneModel;
                    Log("UpdatePnUserToken");
                    if (pnUser.PhoneModel.Length < 20)
                    {
                        pnUser.PhoneModel += $";{Guid.NewGuid()}";
                    }
                    await WebServer.UpdatePnUserToken(pnUser);
                    return loginResult.Data;
                }
                else
                {
                    Log("Login Error" + loginResult.Message);
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
                return token;
            }
            async Task getListing()
            {
                //listings = new List<ListingsListing>();
                ListingInfos = new List<ListingInfo>();
                Log("Get Listings ....");
                var token = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(pnUser.TokenJson);
                Mobile mobile;
                if (string.IsNullOrEmpty(proxyIp) == false)
                {
                    mobile = new Mobile(token, proxyIp) { Token = token, Log = Log1 };
                }
                else
                {
                    mobile = new Mobile() { Token = token, Log = Log1 };
                }
                //await mobile.Dashboard(token.User.AgentId.ToString());
                var result = await mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString()));
                try
                {
                    if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        addListing(result.Data.listings);
                        while (result.Data.page < result.Data.totalPages)
                        {
                            result = await mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString())
                            {
                                Page = (result.Data.page + 1).ToString()
                            });
                            if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                            {
                                addListing(result.Data.listings);
                            }
                        }
                    }
                    else
                    {
                        listings = null;
                    }
                }
                catch
                {

                }
            }

            void addListing(List<ListingsListing> lists)
            {
                if (listings == null)
                    listings = new List<ListingsListing>();
                foreach (var item in lists)
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
                    listings.Add(item);
                }
            }
            return _Token;
        }
        private List<ListingsListing> listings = null;

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
            await Polly.Policy.Handle<Exception>()
                  .WaitAndRetryAsync(10, retryNumber => TimeSpan.FromSeconds(30), (ex, retry) =>
                  {
                      _logger?.Error($"Retry count {retry},{ex.Message},{sbUrl.ToString()}", ex);
                      Log(sbUrl.ToString());
                  }).ExecuteAsync(async () =>
                  {
                      var res = await sbUrl.ToString().GetStringAsync();
                      Log("chopeItem success" + res);
                  });
        }

        private async Task xwebItem(GuruTask guruTask, GuruTaskListing taskListing, int time_cost = 0, string status = "Done", string note = "")
        {
            StringBuilder formData = new StringBuilder();
            Dictionary<string, string> data = new Dictionary<string, string>();
            formData.Append($"account_name={guruTask.Account}&");
            formData.Append($"account_password={guruTask.Password}&");
            formData.Append($"task_id={guruTask.Id}&");
            formData.Append($"taskitem_id={taskListing.TaskItemId}&");
            formData.Append($"status={status}&");
            formData.Append($"time_cost={time_cost}&");
            formData.Append($"taskitem_note={note}&");
            if (taskListing.Listing.Id.HasValue && status == "Done")
            {
                formData.Append($"portal_link=https://www.propertyguru.com.sg/listing/{taskListing.Listing.Id}&");
            }
            else
            {
                formData.Append($"portal_link=&");
            }
            formData.Append($"listing_version={taskListing.UpdateTime}&");
            formData.Append("poster=mobileApi");
            System.Net.Http.StringContent stringContent = new System.Net.Http.StringContent(formData.ToString());
            await Polly.Policy.Handle<Exception>()
                .WaitAndRetryAsync(5, retryNumber => TimeSpan.FromSeconds(30), (ex, retry) =>
                {
                    _logger?.Error($"Retry count {retry},{ex.Message},{formData.ToString()}", ex);
                    Log(formData.ToString());
                }).ExecuteAsync(async () =>
                {
                    await PosterResultUpload.XWebItem(new XWebItemDto()
                    {
                        account_name = guruTask.Account,
                        account_password = guruTask.Password,
                        task_id = guruTask.Id,
                        taskitem_id = taskListing.TaskItemId,
                        status = status,
                        time_cost = time_cost.ToString(),
                        taskitem_note = note,
                        portal_link = (taskListing.Listing.Id.HasValue && status == "Done") ? $"https://www.propertyguru.com.sg/listing/{taskListing.Listing.Id}" : "",
                        listing_version = taskListing.UpdateTime,
                        poster = "mobile_api"
                    });
                    Log("xwebItem success");
                });
        }

        private async Task XwebEnd(GuruTask guruTask, string status = "Done", string note = "")
        {
            if (guruTask.Source.ToLower() == "chope")
                return;

            StringBuilder formData = new StringBuilder();
            formData.Append($"account_name={guruTask.Account}&");
            formData.Append($"account_password={guruTask.Password}&");
            formData.Append($"task_id={guruTask.Id}&");
            formData.Append($"status={status}&");
            formData.Append($"time_cost=&");
            formData.Append($"note={note}&");
            formData.Append("poster=mobileApi");


            await Polly.Policy.Handle<Exception>()
                      .WaitAndRetryAsync(5, retryNumber => TimeSpan.FromSeconds(30), (ex, retry) =>
                      {
                          Log($"Retry count {retry},{ex.Message},{formData.ToString()}");
                      }).ExecuteAsync(async () =>
                      {
                          var result = await PosterResultUpload.XWebEnd(new XWebEndDto()
                          {
                              account_name = guruTask.Account,
                              account_password = guruTask.Password,
                              task_id = guruTask.Id,
                              status = status,
                              time_cost = "",
                              note = note,
                              poster = "mobile_api"
                          });
                          Log("xwebItem success " + result);
                          return result;
                      });
        }

        private async Task End(GuruTask guruTask, string queue_id)
        {
            if (guruTask.Source.ToLower() == "chope")
            {
                string url = "https://pa-production.propnex.net/index.php/pnapi/updateChopeTask?" +
    $"super=1&queue_id={queue_id}&portal=GURU&&memo=&tm={unix_timestamp(DateTime.Now)}";

                await Polly.Policy.Handle<Exception>()
                      .WaitAndRetryAsync(10, retryNumber => TimeSpan.FromSeconds(30), (ex, retry) =>
                      {
                          Log($"Retry count {retry},{ex.Message}");
                      }).ExecuteAsync(async () =>
                      {
                          var result = await url.GetStringAsync();
                          Log($"chope end success" + result);
                      });
            }
        }

        public long unix_timestamp(DateTime dt)
        {
            TimeSpan unix_time = (dt.Date - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)unix_time.TotalSeconds;
        }

        private void Log(string message, bool isRef = false, bool isSave = true)
        {
            MessageEvent?.Invoke(message, isRef);
            if (isSave)
                _logger?.Information(message);
        }

        private void Log1(string message, bool isRef = false)
        {
            MessageEvent?.Invoke(message, isRef);
        }
    }
}
