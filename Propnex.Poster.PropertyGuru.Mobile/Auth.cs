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

        public async Task<Token> LoginAsync(AuthLogin authLogin)
        {
            var request = GetRequest();
            request.Method = Method.Post;
            request.Resource = "/v1/jwt/login";
            request.AddParameter("username", authLogin.UserName);
            request.AddParameter("password", authLogin.Password);
            request.AddParameter("grant_type", "password");
            request.AddParameter("scope", "singapore");
            var token = await client.PostAsync<Token>(request);
            return token;
        }
    }

    public class AuthLogin
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
