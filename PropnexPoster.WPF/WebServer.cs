using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Flurl.Http;
using Polly;
using Propnex.Poster.Dtos;

namespace PropnexPoster.WPF
{
    public class WebServer
    {

        //public static string Id = System.Configuration.ConfigurationManager.AppSettings["Id"];
        public static string BaseUrl = "";
        public static string MachindNumber = "";

        public static async Task<Propnex.Poster.Dtos.PnTaskDto> GetTask()
        {
            string url = $"{BaseUrl}/api/app/pn-task/pn-task?machineId={MachindNumber}";

            try
            {
                var result = await url.GetJsonAsync<PnTaskDto>();
                if (result == null || Guid.Empty == result.Id)
                {
                    return null;
                }
                return result;
            }
            catch (FlurlHttpException ex)
            {
                var exType = ex.GetType();
                await PingAsync();
            }
            catch (Exception ex)
            {
                var exType = ex.GetType();
                await PingAsync();
            }

            return null;
        }

        public static async Task<T> CallBack<T>(Func<Task<T>> action)
        {
            return await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.Zero, async (ex, ts) => await PingAsync())
                .ExecuteAsync(action);
        }

        public static async Task CallBack(Func<Task> action)
        {
            await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.Zero, async (ex, ts) => await PingAsync())
                .ExecuteAsync(action);
        }
        public static async Task<string> GetTaskContent(PnTaskDto pnTaskDto)
        {
            string url = $"{BaseUrl}/api/downloadtask?machineId={MachindNumber}&taskId={pnTaskDto.Id}&fileName={pnTaskDto.Number}";
            return await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.Zero, async (ex, ts) => await PingAsync())
                .ExecuteAsync(() => url.GetStringAsync());
        }

        public static async Task ResetTask(Guid pnTaskId, string message)
        {
            try
            {
                Guid.TryParse(MachindNumber, out var machineId);
                await $"{BaseUrl}/api/app/pn-task/reset-pn-task?machineId={machineId}&pnTaskId={pnTaskId}&message={Uri.EscapeDataString(message)}".PostAsync();
            }
            catch { }
        }

        public static async Task LogErrorAsync(Guid pnTaskId, string message)
        {
            try
            {
                Guid.TryParse(MachindNumber, out var machineId);
                await $"{BaseUrl}/api/app/pn-task/log-error?machineId={machineId}&pnTaskId={pnTaskId}&message={Uri.EscapeDataString(message)}".PostAsync();
            }
            catch { }
        }

        public static async Task PostPntaskRetry(Guid pnTaskId, string message)
        {
            string url = $"{BaseUrl}/api/app/pn-task/pn-task-retry?machineId={MachindNumber}&pnTaskId={pnTaskId}&message={message}";
            await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.Zero, async (ex, ts) => await PingAsync())
                .ExecuteAsync(() => url.PostAsync());
        }

        public static void UpdatePnTask(PnTaskDto pnTaskDto)
        {
            string url = $"{BaseUrl}/api/app/pn-task/";
        }

        public static async Task<string> GetMachineIdAsync(string anyDesk)
        {
            string url = $"{BaseUrl}/api/app/machine/id?anyDesk={anyDesk}";

            return await CallBack<string>(async () =>
            {
                try
                {
                    return await url.GetStringAsync();
                }
                catch
                {
                    return "";
                }
            });
        }

        public static async Task UpdateMachineAsync()
        {
            try
            {
                string url = $"{BaseUrl}/api/app/machine/{MachindNumber}/online";
                await url.PutAsync();
            }
            catch
            {

            }
        }

        public static async Task<PnUserDto> GetUser(string account)
        {
            return await CallBack<PnUserDto>(async () =>
            {
                try
                {
                    var userDto = await $"{BaseUrl}/api/app/pn-user/user?account={account}".GetJsonAsync<PnUserDto>();
                    if (userDto.Account != "")
                        return userDto;
                }
                catch(Exception ex)
                {

                }
                
                return new PnUserDto();
            });
        }

        public static async Task PnUser(PnUserDto userDto)
        {
            await CallBack(async () =>
            {
                try
                {
                    await $"{BaseUrl}/api/app/pn-user".PutJsonAsync(userDto);
                }
                catch
                {

                }
                
            });
        }

        public static async Task UpdatePnUserToken(PnUserDto userDto)
        {
            await CallBack(async () =>
            {
                try
                {
                    await $"{BaseUrl}/api/app/pn-user/upate-token".PostJsonAsync(userDto);
                }
                catch
                {

                }
                
            });
        }

        public static async Task PosterPing()
        {
            try
            {
                await $"https://pa-production.propnex.net/index.php/tools/posterPing?name={MachindNumber}".GetAsync();
            }
            catch
            {

            }

        }

        public static async Task PingAsync()
        {
            Ping ping = new Ping();
            var pingRetryPolicy = Policy
                .Handle<Exception>()
                .OrResult<PingReply>(pr => pr.Status != IPStatus.Success)
                .WaitAndRetryAsync(10, retryNumber => TimeSpan.FromSeconds(60), (ex, retry) =>
                {

                });
            await pingRetryPolicy.ExecuteAsync(async () =>
             {
                 return await ping.SendPingAsync("8.8.8.8");
             });
        }
    }
}
