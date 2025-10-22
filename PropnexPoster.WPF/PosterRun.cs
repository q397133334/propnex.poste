using Flurl.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Propnex;
using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using Propnex.Poster.PropertyGuru.Tasks;
using Propnex.Poster.Share;
using Serilog;
using Serilog.Core;
using SlackBotMessages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ILogger = Serilog.ILogger;

namespace PropnexPoster.WPF
{

    public class PosterRunInfo
    {
        private string taskNumber;
        private string account;
        private string agentId;
        private string taskType;
        private int listingCount = 0;
        private string taskItemId;

        public string TaskNumber { get => taskNumber; set => taskNumber = value; }

        public string Account
        {
            get
            {
                if (string.IsNullOrEmpty(account))
                {
                    return "";
                }
                else
                {
                    return $"Account: {account}";
                }
            }
            set => account = value;
        }

        public string AgentId
        {
            get => string.IsNullOrEmpty(agentId) ? "" : $"AgentId: {agentId}";

            set => agentId = value;
        }

        public string TaskType { get => string.IsNullOrEmpty(agentId) ? "" : $"TaskType: {taskType}"; set => taskType = value; }

        public string TaskItemId { get => string.IsNullOrEmpty(taskItemId) ? "" : $"ListingNumber: {taskItemId}"; set => taskItemId = value; }

        public string ListingCount { get => listingCount == 0 ? "" : $"ListingCount: {listingCount}"; set => listingCount = int.Parse(value); }
    }

    public class PosterRun : Volo.Abp.DependencyInjection.ITransientDependency
    {

        public Action<string> MessageEvent { get; set; }

        public Action<PosterRunInfo> TaskInfoEvent { get; set; }

        private ILogger? _logger;

        public ILogger<PosterRun> globleLogger { get; set; }

        private PnTaskDto taskDto;

        private PosterRunInfo posterRunInfo;

        public PosterRun()
        {
            posterRunInfo = new PosterRunInfo()
            {
                TaskNumber = "Get Task ...."
            };

            TaskInfoEvent?.Invoke(posterRunInfo);
        }


        public async Task Run()
        {
            await WebServer.PosterPing();
            Log("Get Task .....");
            //1.获取任务信息
#if DEBUG
            taskDto = new PnTaskDto()
            {
                Number = "1264056.guru.tsk"
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
            TaskInfoEvent?.Invoke(posterRunInfo);
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
                        TaskInfoEvent?.Invoke(posterRunInfo);
                        Log("Token success");

                        Log($"{task.TaskType.ToLower()}");

                        Api _api;
                        ProjectsApi _projectsApi;
                        AdsProduct _adsProject;
                        Mobile _mobile;
                        if (WPFModule.AppConfiguration.IsProxy)
                        {
                            _api = new Api(token, proxyIp) { Log = Log };
                            _projectsApi = new ProjectsApi(token, proxyIp) { Log = Log };
                            _adsProject = new AdsProduct(token, proxyIp) { Log = Log };
                            _mobile = new Mobile(token, proxyIp) { Log = Log };
                        }
                        else
                        {
                            _api = new Api(token) { Log = Log };
                            _projectsApi = new ProjectsApi(token) { Log = Log };
                            _adsProject = new AdsProduct(token) { Log = Log };
                            _mobile = new Mobile(token) { Log = Log };
                        }

                        //await _mobile.Dashboard(token.User.AgentId.ToString());
                        //4.执行操作
                        if (task.TaskType.ToLower() == "post only")
                        {
                            foreach (var listing in task.Listings.Listings)
                            {
                                posterRunInfo.TaskItemId = listing.TaskItemId.ToString();
                                TaskInfoEvent?.Invoke(posterRunInfo);
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
                                createOrUpdateListing.isLiveTourAvailable = true;
                                var result = await _api.CreateAsync(createOrUpdateListing);
                                //result = new HttpResult<CreateOrUpdateListingResult>() { Data = new CreateOrUpdateListingResult { Id = 24371139 } };
                                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    listing.Listing.Id = result.Data.Id;
                                    if (result.Data.Id != 0)
                                    {
                                        if ((await uploadPhotosAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload photo error");
                                            await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload photo error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                            continue;
                                        }
                                        if ((await uploadVideos(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload video error");
                                            await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload video error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                            continue;
                                        }
                                        if ((await uploadVirtualTours(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload vt error");
                                            await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload vt error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                            continue;
                                        }
                                        if ((await uploadFloorPlanAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload floor plan error");
                                            await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload floor plan error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                            continue;
                                        }
                                        var taskListing = await _api.GetListing(listing.Listing.Id.Value, "DRAFT");
                                        if (taskListing.HttpStatusCode == HttpStatusCode.OK)
                                        {
                                            await _api.UpdateAsync(taskListing.Data);
                                        }
                                        var mobile = new Mobile() { Token = token };
                                        var draflistings =
                                        (await _mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString())
                                        {
                                            StatusCode = "DRAFT"
                                        })).Data.listings;
                                        var draflisting = draflistings.Where(q => q.id == listing.Listing.Id).FirstOrDefault();
                                        if (draflisting != null)
                                        {
                                            var activateResult = await _adsProject.Activate(result.Data.Id, draflisting.charges.activate);
                                            if (activateResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                            {
                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                                            }
                                            else
                                            {

                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", activateResult.Message);
                                            }
                                        }
                                        else
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "not find listing in draf");
                                        }
                                    }
                                }
                                else
                                {
                                    if (result.Message.Contains("Postal code is already being used"))
                                    {
                                        //var listings = _mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString()));
                                        //1.获取邮政编号
                                        var locales = await _api.AutocompleteAsync(new QueryAutocomplete(listing.Listing.Location.postalCode));
                                        if (locales.Data != null)
                                        {
                                            var locale = locales.Data.Where(q => q.DisplayText == listing.Listing.Title).FirstOrDefault();
                                            if (locale == null)
                                                locale = locales.Data.FirstOrDefault();
                                            if (locale != null)
                                            {
                                                //2. 获取loca 信息
                                                var project = (await _projectsApi.GetProjectAsync(int.Parse(locale.ObjectId))).Data;
                                                if (project != null && project.addresses != null && project.addresses.Count > 0)
                                                {
                                                    project.addresses = project.addresses.Where(q => q.external_id != null).ToList();
                                                    createOrUpdateListing.location.id = int.Parse(project.addresses[1].external_id);
                                                    result = await _api.CreateAsync(createOrUpdateListing);
                                                    if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                                    {
                                                        listing.Listing.Id = result.Data.Id;
                                                        if (result.Data.Id != 0)
                                                        {
                                                            if ((await uploadPhotosAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                                            {
                                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload photo error");
                                                                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload photo error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                                                continue;
                                                            }
                                                            if ((await uploadVideos(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                                            {
                                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload video error");
                                                                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload video error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                                                continue;
                                                            }
                                                            if ((await uploadVirtualTours(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                                            {
                                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload vt error");
                                                                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload vt error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                                                continue;
                                                            }
                                                            if ((await uploadFloorPlanAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                                            {
                                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload floor plan error");
                                                                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload floor plan error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                                                continue;
                                                            }
                                                            var taskListing = await _api.GetListing(listing.Listing.Id.Value, "DRAFT");
                                                            await _api.UpdateAsync(taskListing.Data);
                                                            var draflistings =
                                                                (await _mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString())
                                                                {
                                                                    StatusCode = "DRAFT"
                                                                })).Data.listings;
                                                            var draflisting = draflistings.Where(q => q.id == listing.Listing.Id).FirstOrDefault();
                                                            if (draflisting != null)
                                                            {
                                                                var activateResult = await _adsProject.Activate(result.Data.Id, draflisting.charges.activate);
                                                                if (activateResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                                                {
                                                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                                                                }
                                                                else
                                                                {

                                                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", activateResult.Message);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "not find listing in draf");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", result.Message);
                                            }
                                        }
                                        else
                                        {
                                            await SlackBotMessage.SendAsync($"{task.Id}-{listing.TaskItemId}-{listing.Listing.Id} {result.Message}  {WPFModule.AppConfiguration.MachineNumber}  <@U01DQLBLWNL>");
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "not find :" + result.Message);
                                        }
                                    }
                                    else
                                    {
                                        await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", result.Message);
                                        await SlackBotMessage.SendAsync($"{task.Id}-{listing.TaskItemId}-{listing.Listing.Id} {result.Message}");
                                    }
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
                                TaskInfoEvent?.Invoke(posterRunInfo);
                                if (IsExtis(task, listing) != null)
                                {
                                    //match task 
                                    var listInfo = IsExtis(task, listing);
                                    var taskListing = await _api.GetListing(listing.Listing.Id.Value);

                                    if (listing.FastRepost == "0")
                                    {
                                        //更新任务 update task 
                                        taskListing.Data.Update(listing.Listing);
                                        taskListing.Data.isLiveTourAvailable = true;
                                        await _api.UpdateAsync(taskListing.Data);
                                        await _mobile.DeleteMediaAll(taskListing.Data);
                                        if ((await uploadPhotosAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload photo error");
                                            await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload photo error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                            continue;
                                        }
                                        if ((await uploadVideos(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload video error");
                                            await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload video error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                            continue;
                                        }
                                        if ((await uploadVirtualTours(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload vt error");
                                            await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload vt error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                            continue;
                                        }
                                        if ((await uploadFloorPlanAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload floor plan error");
                                            await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload floor plan error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
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
                                    //Post Only
                                    var createOrUpdateListing = new CreateOrUpdateListing();
                                    listing.Listing.Agent.id = token.User.AgentId;
                                    createOrUpdateListing.Create(listing.Listing);

                                    createOrUpdateListing.isLiveTourAvailable = true;
                                    var result = await _api.CreateAsync(createOrUpdateListing);
                                    //result = new HttpResult<CreateOrUpdateListingResult>() { Data = new CreateOrUpdateListingResult { Id = 24371139 } };
                                    if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        listing.Listing.Id = result.Data.Id;
                                        if (result.Data.Id != 0)
                                        {
                                            if ((await uploadPhotosAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                            {
                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload photo error");
                                                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload photo error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                                continue;
                                            }
                                            if ((await uploadVideos(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                            {
                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload video error");
                                                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload video error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                                continue;
                                            }
                                            if ((await uploadVirtualTours(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                            {
                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload vt error");
                                                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload vt error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                                continue;
                                            }
                                            if ((await uploadFloorPlanAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                            {
                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload floor plan error");
                                                await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload floor plan error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                                continue;
                                            }
                                            var taskListing = await _api.GetListing(listing.Listing.Id.Value, "DRAFT");
                                            await _api.UpdateAsync(taskListing.Data);
                                            await _api.UpdateAsync(taskListing.Data); var mobile = new Mobile() { Token = token };
                                            var draflistings =
                                            (await _mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString())
                                            {
                                                StatusCode = "DRAFT"
                                            })).Data.listings;
                                            var draflisting = draflistings.Where(q => q.id == listing.Listing.Id).FirstOrDefault();
                                            if (draflisting != null)
                                            {
                                                var activateResult = await _adsProject.Activate(result.Data.Id, draflisting.charges.activate);
                                                if (activateResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                                {
                                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                                                }
                                                else
                                                {

                                                    await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", activateResult.Message);
                                                }
                                            }
                                            else
                                            {
                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "not find listing in draf");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (result.Message.Contains("Postal code is already being used"))
                                        {
                                            var listings = _mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString()));
                                            //1.获取邮政编号
                                            var locales = await _api.AutocompleteAsync(new QueryAutocomplete(listing.Listing.Location.postalCode));
                                            var locale = locales.Data.FirstOrDefault();
                                            //2. 获取loca 信息
                                            var project = (await _projectsApi.GetProjectAsync(int.Parse(locale.ObjectId))).Data;
                                            if (project != null && project.addresses != null && project.addresses.Count > 0)
                                            {
                                                createOrUpdateListing.location.id = int.Parse(project.addresses[0].external_id);
                                                result = await _api.CreateAsync(createOrUpdateListing);
                                                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                                {
                                                    listing.Listing.Id = result.Data.Id;
                                                    if (result.Data.Id != 0)
                                                    {
                                                        await uploadPhotosAsync(listing, _api);
                                                        await uploadVideos(listing, _api);
                                                        await uploadVirtualTours(listing, _api);
                                                        await uploadFloorPlanAsync(listing, _api);
                                                        var taskListing = await _api.GetListing(listing.Listing.Id.Value, "DRAFT");
                                                        await _api.UpdateAsync(taskListing.Data);
                                                        await _api.UpdateAsync(taskListing.Data); var mobile = new Mobile() { Token = token };
                                                        var draflistings =
                                                        (await _mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString())
                                                        {
                                                            StatusCode = "DRAFT"
                                                        })).Data.listings;
                                                        var draflisting = draflistings.Where(q => q.id == listing.Listing.Id).FirstOrDefault();
                                                        if (draflisting != null)
                                                        {
                                                            var activateResult = await _adsProject.Activate(result.Data.Id, draflisting.charges.activate);
                                                            if (activateResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                                            {
                                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString());
                                                            }
                                                            else
                                                            {

                                                                await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", activateResult.Message);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "not find listing in draf");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", result.Message);
                                        }
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
                                TaskInfoEvent?.Invoke(posterRunInfo);
                                if (IsExtis(task, listing) != null)
                                {
                                    //更新任务 UpdateTask 

                                    // get listing detial 
                                    var taskListing = await _api.GetListing(listing.Listing.Id.Value);
                                   
                                    //replace listing 
                                    taskListing.Data.Update(listing.Listing);
                                    taskListing.Data.isLiveTourAvailable = true;

                                    //update listing
                                    await _api.UpdateAsync(taskListing.Data);

                                    await _mobile.DeleteMediaAll(taskListing.Data);
                                    if ((await uploadPhotosAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                    {
                                        await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload photo error");
                                        await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload photo error {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                        continue;
                                    }
                                    if ((await uploadVideos(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                    {
                                        await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload video error");
                                        await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload video error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                        continue;
                                    }
                                    if ((await uploadVirtualTours(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                    {
                                        await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload vt error");
                                        await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload vt error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                        continue;
                                    }
                                    if ((await uploadFloorPlanAsync(listing, _api)).HttpStatusCode != System.Net.HttpStatusCode.OK)
                                    {
                                        await ResultUpload(task, listing, listing.TaskItemId, listing.Listing.Id.ToString(), "Failed", "upload floor plan error");
                                        await SlackBotMessage.SendAsync($"TaskId:{task.Id}-TaskItemid:{listing.TaskItemId}-ListingId{listing.Listing.Id} upload floor plan error  {WPFModule.AppConfiguration.MachineNumber} <@U01DQLBLWNL>");
                                        continue;
                                    }
                                    await _api.GetListing(listing.Listing.Id.Value);
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
                                TaskInfoEvent?.Invoke(posterRunInfo);
                                if (IsExtis(task, listing) != null)
                                {
                                    var listingInfo = IsExtis(task, listing);
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
                                                                    ok = await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                                                    httpResult = await ok.Content.ReadAsStringAsync();
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
                                                                            httpResult = await ok.Content.ReadAsStringAsync();
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
                                                                    httpResult = await ok.Content.ReadAsStringAsync();
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
                                                                    ok = await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                                                    httpResult = await ok.Content.ReadAsStringAsync();
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
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("{0},{1}", ex.Message, ex.StackTrace);
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
            }
            finally
            {
                this.globleLogger = null;
                this.MessageEvent = null;
                this.TaskInfoEvent = null;
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

        private async Task<HttpResult<string>> uploadPhotosAsync(GuruTaskListing guruTaskListing, Api _api)
        {
            HttpResult<string> result = new HttpResult<string>() { HttpStatusCode = HttpStatusCode.OK };
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
                var filePath = $"{path}{i}_image.jpg";
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
                    DownClient webClient = new DownClient();
                    webClient.DownloadFile(guruTaskListing.Photos[i], filePath);
                    Log("download photo complete");
                    result = await _api.UploadPhotoAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);
                    if (result.HttpStatusCode != System.Net.HttpStatusCode.OK && result.HttpStatusCode != HttpStatusCode.BadRequest)
                    {
                        break;
                    }
                }
                catch
                {

                }
            }
            return result;
        }

        private async Task<HttpResult<string>> uploadVideos(GuruTaskListing guruTaskListing, Api _api)
        {
            HttpResult<string> result = new HttpResult<string>() { HttpStatusCode = HttpStatusCode.OK };
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
                var filePath = $"{path}{i}_movie.mp4";
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
                    filePath = guruTaskListing.Videos[i];
                    filePath = System.Web.HttpUtility.UrlDecode(filePath);
                }
                else
                {
                    try
                    {
                        Log($"download move {filePath}");
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        DownClient webClient = new DownClient();
                        webClient.DownloadFile(guruTaskListing.Videos[i], filePath);
                        Log("download move complete");
                    }
                    catch { }
                    if (System.IO.File.Exists(filePath) == false)
                    {
                        continue;
                    }
                }
                result = await _api.UploadVideosAsync($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);
                if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    break;
                }
            }
            return result;
        }

        private async Task<HttpResult<string>> uploadVirtualTours(GuruTaskListing guruTaskListing, Api _api)
        {
            HttpResult<string> result = new HttpResult<string>() { HttpStatusCode = HttpStatusCode.OK };
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
                var filePath = $"{path}{i}_vt.mp4";
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
                        DownClient webClient = new DownClient();
                        webClient.DownloadFile(guruTaskListing.Tours[i], filePath);
                        Log("download tour complete");
                    }
                    catch
                    {
                        filePath = "";
                    }
                }
                result = await _api.UplaodVirtualTours($"{guruTaskListing.Listing.Id}", $"{i + 1}", filePath, title);
                if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    break;
                }
            }

            return result;
        }

        private async Task<HttpResult<string>> uploadFloorPlanAsync(GuruTaskListing guruTaskListing, Api _api)
        {
            HttpResult<string> result = new HttpResult<string>() { HttpStatusCode = HttpStatusCode.OK };
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
                var filePath = $"{path}{i}_fp.jpg";
                try
                {
                    //await guruTaskListing.FloorPlan[i].DownloadFileAsync(path, $"{i}_fp.jpg");

                    Log("download FloorPlan");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    DownClient webClient = new DownClient();
                    webClient.DownloadFile(guruTaskListing.FloorPlan[i], filePath);
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
            var pnUser = await getUser();
            var _Token = await auth();// string.IsNullOrEmpty(pnUser.TokenJson) ? await auth() : await checkToken();
            if (_Token == null)
                return null;
            await getListing();

            async Task<PnUserDto> getUser()
            {
                Log("get user ....");
                var pnUser = new PnUserDto() { Id = Guid.Empty }; //await WebServer.GetUser(guruTask.Account);

                //2.验证用户信息
                if (pnUser.Id == Guid.Empty)
                {
                    Log("not find user ");
                    pnUser = new PnUserDto();
                    pnUser.Account = guruTask.Account;
                    pnUser.Password = guruTask.Password;
                    Log("insert user ....");
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
                    _auth = new Auth(proxyIp) { Log = Log };
                }
                else
                {
                    _auth = new Auth() { Log = Log };
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
                    Log("UpdatePnUserToken");
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
                await getListing();
                if (listings == null)
                    return await auth();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(pnUser.TokenJson);
            }
            async Task getListing()
            {
                listings = new List<ListingsListing>();
                ListingInfos = new List<ListingInfo>();
                Log("Get Listings ....");
                var token = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(pnUser.TokenJson);
                Mobile mobile;
                if (string.IsNullOrEmpty(proxyIp) == false)
                {
                    mobile = new Mobile(token, proxyIp) { Token = token, Log = Log };
                }
                else
                {
                    mobile = new Mobile() { Token = token, Log = Log };
                }
                await mobile.Dashboard(token.User.AgentId.ToString());
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
                }
                catch
                {

                }
            }

            void addListing(List<ListingsListing> lists)
            {
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
        private List<ListingsListing> listings = new List<ListingsListing>();

        private async Task<GuruTasks> getGuruTasks()
        {
            string context = "";
            taskDto = await WebServer.GetTask();
            //taskDto = new PnTaskDto()
            //{
            //    Id = Guid.Parse("3a0ceff3-f520-f889-5e83-327a219f7445"),
            //    Number = "954852.guru.tsk"
            //};

            if (taskDto != null)
            {
                try
                {
                    context = await WebServer.GetTaskContent(taskDto);
                    var lenght = context.IndexOf("Xpressor-Listing-File===");
                    var taskContext = context.Substring(0, lenght == -1 ? context.Length : lenght);
                    return new GuruTasks(context, taskContext);
                }
                catch
                {
                    return null;
                }
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

        private void Log(string message)
        {
            MessageEvent?.Invoke(message);
            _logger?.Information(message);
        }
    }
}
