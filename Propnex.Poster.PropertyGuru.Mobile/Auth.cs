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

        private ILogger Logger;

        public Auth(ILogger logger) : base("https://auth.propertyguru.com")
        {
            Logger = logger;
        }

        public async Task<Token> Login(string username, string password)
        {
            var request = GetRequest();
            request.Method = Method.Post;
            request.AddParameter("username", username);
            request.AddParameter("password", password);
            request.AddParameter("grant_type", "password");
            request.AddParameter("scope", "singapore");
            var token = await client.PostAsync<Token>(request);
            return token;
        }
    }
}
