using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Propnex.Poster.PropertyGuru.Mobile.Dto;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class Auth : ClientBase
    {
        private const string baseUrl = "https://auth.propertyguru.com";

        public ILogger<Auth> _logger { get; set; }

        public Auth(ILogger<Auth> logger) : base(baseUrl)
        {
            _logger = logger;
        }

        public Auth() : base(baseUrl)
        {

        }

        public Auth(string proxyIp):base(baseUrl,proxyIp)
        {

        }

        public async Task<HttpResult<Token>> LoginAsync(AuthLogin authLogin)
        {
            var request = GetRequest();
            request.Method = Method.Post;
            request.Resource = "/v1/jwt/login";
            request.AddHeader("x-clientid", "766f966f-e33c-4a51-ae2c-1c2ccb989eeb");
            request.AddHeader("x-clientsecret", "hJUmEWsszf3Oz3CR5TZgVlPyvbwJ0wwoQzPTD4Yp14enfYqlrUOd0uw2MFSKACfaYAxvDcK8Xq3Tafm");
            request.AddParameter("username", authLogin.UserName);
            request.AddParameter("password", authLogin.Password);
            request.AddParameter("grant_type", "password");
            request.AddParameter("scope", "singapore");

            var response = await ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new HttpResult<Token>()
                {
                    Data = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(response.Content),
                    HttpStatusCode = response.StatusCode
                };
            }
            return GetHttpResult<Token>(response);


        }
    }

    public class AuthLogin
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
