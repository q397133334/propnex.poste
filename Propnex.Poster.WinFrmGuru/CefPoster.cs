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

        IPosterAction action;

        Serilog.ILogger Logger;

        private void CefPoster_Load(object sender, EventArgs e)
        {
            cwb.LoadUrl("http://www.baidu.com");
        }

        public async void PosterStart()
        {
            try
            {
                await getGuruTasks();
                Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"logs\\task\\{taskDto.Number}.txt", rollingInterval: RollingInterval.Infinite)
                .CreateLogger();
                IPosterAction posterAction = new CefPosterGuruAction(cwb, Logger);
                if (guruTasks != null)
                {
                    for (int i = 0; i < guruTasks.Tasks.Count; i++)
                    {
                        guruTask = guruTasks.Tasks[i];

                        var loginResult = await posterAction.Login(guruTask.Account, guruTask.Password);
                        if (loginResult.Status != PosterActionResultStatus.Success)
                        {
                            if ((loginResult.Message != "Invalid captcha value." || !loginResult.Message.Contains("attempts")) && loginResult.Message != "Verification Code" && loginResult.Message != "")
                            {
                                if (guruTask.Listings.Listings != null)
                                {
                                    foreach (var item in guruTask.Listings.Listings)
                                    {
                                        ResultUpload(item, item.TaskItemId, "", "Failed", "Email or Password not valid.Please try again");
                                        End(item.TaskItemId);
                                    }
                                }
                            }
                            else
                            {
                                await Task.Delay(1000 * 60 * 5);
                                Logger.Information($"waiting 5 min ,message {loginResult.Message}");
                            }
                        }
                        else
                        {

                        }
                        //await getLisints();
                        //if (task.TaskType.ToLower() == "post only")
                        //{
                        //    await postOnlyAsync(task);
                        //}
                        //if (task.TaskType.ToLower() == "repost")
                        //{
                        //    await repost(task);
                        //}
                        //if (task.TaskType.ToLower() == "update")
                        //{
                        //    await update(task);
                        //}
                        //if (task.TaskType.ToLower() == "remove")
                        //{
                        //    await remove(task);
                        //}
                    }
                }

            }
            catch (Exception ex)
            {

            }

            //Close();
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
