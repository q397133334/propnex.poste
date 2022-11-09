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
using Flurl.Http;
using Propnex.Poster.Dtos;

namespace Propnex.Poster.Guru.Api
{
    public class WebServer
    {

        public static string Id = System.Configuration.ConfigurationManager.AppSettings["Id"];
        public static string BaseUrl = System.Configuration.ConfigurationManager.AppSettings["BaseUrl"];

        public static async Task<Dtos.PnTaskDto> GetTask()
        {
            string url = $"{BaseUrl}/api/app/pn-task/pn-task?machineId={Id}";

            try
            {
                var result = await url.GetJsonAsync<Dtos.PnTaskDto>();
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
            int count = 0;
            T context = default(T);
            while (count < 10)
            {
                count++;
                try
                {
                    context = await action();
                }
                catch (Exception ex)
                {
                    await PingAsync();
                }
            }
            return context;
        }

        public static async Task CallBack(Action action)
        {
            int count = 0;
            while (count < 10)
            {
                count++;
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    await PingAsync();
                }
            }
        }
        public static async Task<string> GetTaskContent(Dtos.PnTaskDto pnTaskDto)
        {
            int count = 0;
            var context = "";
            while (count < 10)
            {
                count++;
                try
                {
                    string url = $"{BaseUrl}/api/downloadtask?machineId={Id}&taskId={pnTaskDto.Id}&fileName={pnTaskDto.Number}";
                    context = await url.GetStringAsync();
                    count = 20;
                    break;
                }
                catch
                {
                    await PingAsync();
                }
            }
            return context;
        }

        public static async void PostPntaskRetry(Guid pnTaskId, string message)
        {
            string url = $"{BaseUrl}/api/app/pn-task/pn-task-retry?machineId={Id}&pnTaskId={pnTaskId}&message={message}";
            int count = 0;
            while (count < 10)
            {
                count++;
                try
                {
                    await url.PostAsync();
                }
                catch
                {
                    await PingAsync();
                }
            }
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
                return await url.GetStringAsync();
            });
        }


        public static async Task PingAsync()
        {
            Ping ping = new Ping();
            var reply = await ping.SendPingAsync("61.147.37.1");
            while (reply.Status != IPStatus.Success)
            {
                Console.WriteLine("Ping" + reply.Status);
                await Task.Delay(1000 * 10);
                reply = await ping.SendPingAsync("61.147.37.1");
            }
        }

        public static void Ping()
        {
            Ping ping = new Ping();
            var reply = ping.Send("61.147.37.1");
            while (reply.Status != IPStatus.Success)
            {
                Console.WriteLine("Ping" + reply.Status);
                System.Threading.Thread.Sleep(1000 * 10);
                reply = ping.Send("61.147.37.1");
            }
        }
    }
}
