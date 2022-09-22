using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;

namespace Propnex.Poster.Guru.Api
{
    public class WebServer
    {

        public static string Id = System.Configuration.ConfigurationManager.AppSettings["Id"];

        public static string BaseUrl = System.Configuration.ConfigurationManager.AppSettings["BaseUrl"];

        public static async Task<Dtos.PnTaskDto> GetTask()
        {
            string url = $"{BaseUrl}/api/app/pn-task/pn-task";

            var result = await url.GetJsonAsync<Dtos.PnTaskDto>();

            if (Guid.Empty == result.Id)
            {
                return null;
            }
            return result;
        }

        public static async Task<string> GetTaskContent(Dtos.PnTaskDto pnTaskDto)
        {
            string url = $"{BaseUrl}/api/downloadtask?taskId={pnTaskDto.Id}&fileName={pnTaskDto.Number}";

            return  await url.GetStringAsync();
        }
    }
}
