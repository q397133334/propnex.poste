using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using System.Threading.Tasks;

namespace Propnex
{
    public static class PosterResultUpload
    {
        private static IRestClient propnexClient = new RestClient(new RestSharp.RestClientOptions()
        {
            BaseUrl = new Uri("https://pa-production.propnex.net")
        });

        public static async Task<string> XWebItem(XWebItemDto xWebItem)
        {
            var request = new RestRequest("/index.php/tasks/updateStatus", Method.Post);
            // 设置 Content-Type
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            // 将 DTO 属性添加为表单参数
            request.AddParameter("account_name", xWebItem.account_name);
            request.AddParameter("account_password", xWebItem.account_password);
            request.AddParameter("task_id", xWebItem.task_id);
            request.AddParameter("taskitem_id", xWebItem.taskitem_id);
            request.AddParameter("status", xWebItem.status);
            request.AddParameter("time_cost", xWebItem.time_cost);
            request.AddParameter("taskitem_note", xWebItem.taskitem_note);
            request.AddParameter("portal_link", xWebItem.portal_link);
            request.AddParameter("listing_version", xWebItem.listing_version);
            // 执行请求
            var response = await propnexClient.ExecuteAsync(request);

            // 处理响应
            if (response.IsSuccessful)
            {
                Console.WriteLine("请求成功!");
                return response?.Content;
            }
            else
            {
                return response.StatusCode.ToString();
            }
        }

        public static async Task<string> XWebEnd(XWebEndDto dto)
        {
            var request = new RestRequest("/index.php/tasks/updateStatus", Method.Post);
            // 设置 Content-Type
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            // 将 DTO 属性添加为表单参数
            request.AddParameter("account_name", dto.account_name);
            request.AddParameter("account_password", dto.account_password);
            request.AddParameter("task_id", dto.task_id);
            request.AddParameter("status", dto.status);
            request.AddParameter("time_cost", dto.time_cost);
            request.AddParameter("note", dto.note);
            request.AddParameter("poster", dto.poster); 
            // 执行请求
            var response = await propnexClient.ExecuteAsync(request);

            // 处理响应
            if (response.IsSuccessful)
            {
                Console.WriteLine("请求成功!");
                return response?.Content;
            }
            else
            {
                return response.StatusCode.ToString();
            }
        }
    }


    public class XWebItemDto
    {
        public string account_name { get; set; }

        public string account_password { get; set; }

        public string task_id { get; set; }

        public string taskitem_id { get; set; }

        public string status { get; set; }

        public string time_cost { get; set; }

        public string taskitem_note { get; set; }

        public string portal_link { get; set; }

        public string listing_version { get; set; }

        public string poster { get; set; } = "mobile_api";
    }

    public class XWebEndDto
    {
        public string account_name { get; set; }

        public string account_password { get; set; }

        public string task_id { get; set; }

        public string status { get; set; }

        public string time_cost { get; set; }

        public string note { get; set; }

        public string poster { get; set; } = "mobile_api";
    }
}
