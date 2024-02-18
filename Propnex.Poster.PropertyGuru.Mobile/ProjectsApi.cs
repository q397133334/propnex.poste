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

        private const string baseUrl = "https://projects-api-projectnet.propertyguru.com";

        public Token Token { get; set; }

        public ProjectsApi() : base("https://projects-api-projectnet.propertyguru.com")
        {
        }

        public ProjectsApi(Token token) : base(baseUrl)
        {
            Token = token;
        }

        public ProjectsApi(Token token, string proxyIp) : base(baseUrl, proxyIp)
        {
            Token = token;
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

            var respones = await ExecuteAsync(request);
            if (respones.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new HttpResult<Model.Project>()
                {
                    Data = Model.Project.FromJson(respones.Content),
                    HttpStatusCode = respones.StatusCode,
                    Message = respones.Content
                };
            }
            return GetHttpResult<Model.Project>(respones);

        }
    }
}
