using Abp.Dependency;
using Castle.MicroKernel;
using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.Dom;
using CefSharp.WinForms;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru.Tasks;
using Propnex.Poster.Share;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Propnex.Poster.Guru
{
    public partial class CefPoster : Form, ITransientDependency
    {

        private readonly IIocManager _iocManager;
        private readonly IIocResolver _iocResolver;

        private PnTaskDto taskDto;

        private GuruTasks guruTasks;

        private GuruTask guruTask;

        public CefPoster(IIocManager iocManager, IIocResolver iocResolver)
        {
            InitializeComponent();
            _iocManager = iocManager;
            _iocResolver = iocResolver;
        }

        Serilog.ILogger Logger;

        private void CefPoster_Load(object sender, EventArgs e)
        {
            //this.FormBorderStyle=FormBorderStyle.None;
            cwb.LoadUrl("http://www.baidu.com");
        }

        public async void PosterStart()
        {
            try
            {
                await getGuruTasks();
                if (taskDto == null)
                {
                    Console.WriteLine("Not find task ,delay 1 min");
                    await Task.Delay(1000 * 60);
                    Close();
                    return;
                }

                Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File($"logs\\task\\{taskDto.Number}.txt", rollingInterval: RollingInterval.Infinite)
                .CreateLogger();
                IPosterAction<GuruTaskListing> posterAction = new CefPosterGuruAction(cwb, Logger);
                if (guruTasks != null)
                {
                    Logger.Information($"start run task {taskDto.Number}");
                    for (int i = 0; i < guruTasks.Tasks.Count; i++)
                    {
                        guruTask = guruTasks.Tasks[i];

                        var loginResult = await posterAction.Login(guruTask.Account, guruTask.Password);
                        if (loginResult.Status != PosterActionResultStatus.Success)
                        {
                            Logger.Information("login error");
                            Logger.Information($"{loginResult.Message}");
                            if ((loginResult.Message != "Invalid captcha value." || !loginResult.Message.Contains("attempts")) && loginResult.Message != "Verification Code" && loginResult.Message != "")
                            {
                                if (guruTask.Listings.Listings != null)
                                {
                                    loginResult.Message = string.IsNullOrEmpty(loginResult.Message) ? "Email or Password not valid.Please try again" : loginResult.Message;
                                    foreach (var item in guruTask.Listings.Listings)
                                    {
                                        ResultUpload(item, item.TaskItemId, "", "Failed", $"{loginResult.Message}");
                                        End(item.TaskItemId);
                                    }
                                }
                            }
                            else
                            {
                                Api.WebServer.PostPntaskRetry(taskDto.Id, loginResult.Message);
                                Logger.Information($"waiting 5 min ,message {loginResult.Message}");
                                await Task.Delay(1000 * 60 * 5);
                            }
                        }
                        else
                        {
                            var result = new PosterActionResult();
                            if (guruTask.TaskType.ToLower() == "post only")
                            {
                                for (var j = 0; j < guruTask.Listings.Listings.Count; j++)
                                {
                                    var item = guruTask.Listings.Listings[j];
                                    result = await posterAction.PostOnly(item);
                                    Logger.Information($"{result.Status}--{result.Message}");
                                    if (result.Status == PosterActionResultStatus.Success)
                                    {
                                        ResultUpload(item, item.TaskItemId, item.Listing.Id.ToString());
                                    }
                                    else
                                    {
                                        ResultUpload(item, item.TaskItemId, "", "Failed", result.Message.ToString());
                                    }
                                    End(item.TaskItemId);
                                }
                            }

                            if (guruTask.TaskType.ToLower() == "repost")
                            {
                                for (var j = 0; j < guruTask.Listings.Listings.Count; j++)
                                {
                                    var item = guruTask.Listings.Listings[j];
                                    result = await posterAction.Repost(item);
                                    Logger.Information($"{result.Status}--{result.Message}");
                                    if (result.Status == PosterActionResultStatus.Success)
                                    {
                                        ResultUpload(item, item.TaskItemId, item.Listing.Id.ToString());
                                    }
                                    else
                                    {
                                        ResultUpload(item, item.TaskItemId, "", "Failed", result.Message.ToString());
                                    }
                                    End(item.TaskItemId);
                                }

                            }

                            if (guruTask.TaskType.ToLower() == "update")
                            {
                                for (var j = 0; j < guruTask.Listings.Listings.Count; j++)
                                {
                                    var item = guruTask.Listings.Listings[j];
                                    result = await posterAction.Update(item);
                                    Logger.Information($"{result.Status}--{result.Message}");
                                    if (result.Status == PosterActionResultStatus.Success)
                                    {
                                        ResultUpload(item, item.TaskItemId, item.Listing.Id.ToString());
                                    }
                                    else
                                    {
                                        ResultUpload(item, item.TaskItemId, "", "Failed", result.Message.ToString());
                                    }
                                    End(item.TaskItemId);
                                }
                            }

                            if (guruTask.TaskType.ToLower() == "remove from portals")
                            {
                                for (var j = 0; j < guruTask.Listings.Listings.Count; j++)
                                {
                                    var item = guruTask.Listings.Listings[j];
                                    result = await posterAction.Remove(item);
                                    Logger.Information($"{result.Status}--{result.Message}");
                                    if (result.Status == PosterActionResultStatus.Success)
                                    {
                                        ResultUpload(item, item.TaskItemId, item.Listing.Id.ToString());
                                    }
                                    else
                                    {
                                        ResultUpload(item, item.TaskItemId, "", "Failed", result.Message.ToString());
                                    }
                                    End(item.TaskItemId);
                                }
                            }
                            if (guruTask.TaskType.ToLower().IndexOf("retrieve") > -1)
                            {
                                for (var j = 0; j < guruTask.Listings.Listings.Count; j++)
                                {
                                    var item = guruTask.Listings.Listings[j];
                                    ResultUpload(item, item.TaskItemId, "", "Failed", "To realize the function, wait a few days");
                                    End(item.TaskItemId);
                                }
                            }
                            XwebEnd();
                            Logger.Information("Success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "PosterStart");
            }

            Close();
        }


        #region 私有方法

        /// <summary>
        /// 获取xml
        /// </summary>
        /// <returns></returns>
        private async Task getGuruTasks()
        {
            string context = "";
            taskDto = await Api.WebServer.GetTask();
            if (taskDto != null)
            {
                context = await Api.WebServer.GetTaskContent(taskDto);
                var lenght = context.IndexOf("Xpressor-Listing-File===");
                var taskContext = context.Substring(0, lenght == -1 ? context.Length : lenght);
                guruTasks = new GuruTasks(context, taskContext);
            }
            else
            {
                guruTasks = null;
            }
        }

        private void ResultUpload(GuruTaskListing taskListing, string queue_id, string listing_id, string status = "Done", string memo = "")
        {


            Logger.Information($"result upload queue_id is {queue_id},listing_id is {listing_id} ,status is {status},memo is {memo}");

            if (guruTask.Source.ToLower() == "chope")
            {
                chopeItem(queue_id, listing_id, status, memo);
            }
            else
            {
                xwebItem(taskListing, 0, status, memo);
            }
        }

        private void chopeItem(string queue_id, string listing_id, string status = "Done", string memo = "")
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
            using (System.Net.WebClient webClient = new System.Net.WebClient())
            {
                try
                {
                    webClient.DownloadString(sbUrl.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"upload result error {ex.Message}");
                }
            }
        }

        private async void xwebItem(GuruTaskListing taskListing, int time_cost = 0, string status = "Done", string note = "")
        {
            StringBuilder formData = new StringBuilder();
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
            formData.Append("poster=selenium");

            try
            {
                var result = await "https://pa-production.propnex.net/index.php/tasks/updateStatus".PostStringAsync(formData.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"upload result error {ex.Message}");
            }
        }

        private async void XwebEnd(string note = "")
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
                    var result = await "https://pa-production.propnex.net/index.php/tasks/updateStatus".PostStringAsync(formData.ToString());
                    Logger.Information($"Xweb end success");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Xweb end upload result error {ex.Message}");
                }
            }
        }

        private void End(string queue_id)
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
                            webClient.DownloadString(url.ToString());
                            Logger.Information($"chope end success");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, $"chope end upload result error {ex.Message}");
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

        #endregion
    }
}
