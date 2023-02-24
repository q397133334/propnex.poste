using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class ProjectsApi : ClientBase
    {
        public Token Token { get; set; }

        public ILogger<ProjectsApi> Logger { get; set; }

        public ProjectsApi() : base("https://projects-api-projectnet.propertyguru.com")
        {
        }

        public async Task<HttpResult<Model.Project>> GetProjectAsync(int property_id)
        {
            var request = GetRequest();
            request.Resource = "/v1/project";
            request.Method = Method.Get;
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddParameter("property_id", property_id);
            request.AddParameter("country", "singapore");
            request.AddParameter("language", "en");

            var respones = await client.ExecuteAsync(request);
            if (respones.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new HttpResult<Model.Project>()
                {
                    Data = Model.Project.FromJson(respones.Content),
                    HttpStatusCode = respones.StatusCode
                };
            }
            return GetHttpResult<Model.Project>(respones);

        }
    }
}
