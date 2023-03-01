using Castle.Core.Configuration;
using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru.Tasks;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PropnexPoster.WPF
{
    public class PosterRun
    {

        public Action<string>? MessageEvent { get; set; }

        public Action<string, string, string> TaskInfoEvent { get; set; }

        private ILogger? _logger;

        private PnTaskDto taskDto;

        public PosterRun()
        {

        }


        public async Task Run()
        {
            Log("Get Task .....");
            //1.获取任务信息
            var task = await getGuruTasks();
            if (task == null)
            {
                Log("Not find task ,delay 1 min");
                await Task.Delay(1000 * 60); return;
            }
            TaskInfoEvent?.Invoke(taskDto.Number, "", "");
            Log($"Get Tas success,{taskDto.Number}");
            //2.生成日志
            _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File($"{Directory.GetDirectoryRoot(System.AppDomain.CurrentDomain.BaseDirectory)}\\logs\\task\\{taskDto.Number}.txt", rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
            //4.处理任务
            for (int i = 0; i < task.Tasks.Count; i++)
            {
                //1.获取用户信息
                //2.验证用户信息
                //3.登陆
                //4.执行操作
            }
            //5.
        }

        private async Task<GuruTasks> getGuruTasks()
        {
            string context = "";
            //taskDto = await Api.WebServer.GetTask();
            var taskDto = new PnTaskDto()
            {
                Id = Guid.Parse("3a096f11-6583-7283-5eea-693372dab84c"),
                Number = "881997.guru.tsk"
            };

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

        private void Log(string message)
        {
            MessageEvent?.Invoke(message);
            _logger?.Information(message);
        }
    }
}
