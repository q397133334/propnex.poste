using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class ClientBase : IDisposable
    {
        protected readonly RestClient client;

        public ClientBase(string baseUrl)
        {
            client = new RestSharp.RestClient(baseUrl);
            client.AddDefaultHeader("User-Agent", "sg;agentnet;android;23.2.10;HD1910;null");
        }

        public RestRequest GetRequest()
        {
            RestRequest request = new RestRequest();
            //request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
            //request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
            return request;
        }

        public RestRequest GetRequest(Method method, string resource)
        {
            RestRequest request = new RestRequest();
            //request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
            //request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
            request.Method = method;
            request.Resource = resource;
            return request;
        }

        public HttpResult<T> GetHttpResult<T>(RestResponse restResponse)
        {
            return new HttpResult<T>()
            {
                HttpStatusCode = restResponse.StatusCode,
                Message = restResponse.Content
            };
        }

        public void Dispose()
        {
            client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
