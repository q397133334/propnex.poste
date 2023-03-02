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

        public ILogger<Auth> _logger { get; set; }

        public Auth(ILogger<Auth> logger) : base("https://auth.propertyguru.com")
        {
            _logger = logger;
        }

        public Auth() : base("https://auth.propertyguru.com")
        {

        }

        public async Task<HttpResult<Token>> LoginAsync(AuthLogin authLogin)
        {
            var request = GetRequest();
            request.Method = Method.Post;
            request.Resource = "/v1/jwt/login";
            request.AddParameter("username", authLogin.UserName);
            request.AddParameter("password", authLogin.Password);
            request.AddParameter("grant_type", "password");
            request.AddParameter("scope", "singapore");

            var response = await client.PostAsync(request);
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
