using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class ClientBase : IDisposable
    {
        protected readonly RestClient client;

        public Action<string, RestResponse> LogHttpResponseMessage;
        public Action<string> Log;

        private AsyncRetryPolicy<RestResponse> clientRetryPolicy;


        public ClientBase(string baseUrl)
        { 
            client = new RestSharp.RestClient(new RestClientOptions()
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 1000 * 60 * 10
            });
            client.AddDefaultHeader("User-Agent", "sg;agentnet;android;23.2.10;HD1910;null");
            Log?.Invoke($"client {baseUrl}");
            clientRetryPolicy = Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted)
                .WaitAndRetryAsync(10, retryNumber => TimeSpan.FromSeconds(60), (ex, retry) =>
                {
                    LogHttpResponseMessage?.Invoke($"{ex.Result.Request.Resource}", ex.Result);
                });
        }

        public ClientBase(string baseUrl, string proxyIp)
        {
            var ip = proxyIp.Split(':')[0];
            var port = proxyIp.Split(':')[1];
            client = new RestSharp.RestClient(new RestClientOptions()
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 1000 * 60 * 10,
                Proxy = new System.Net.WebProxy(ip, int.Parse(port))
            });
            client.AddDefaultHeader("User-Agent", "sg;agentnet;android;23.2.10;HD1910;null");
            Log?.Invoke($"client {baseUrl}");
            clientRetryPolicy = Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted)
                .WaitAndRetryAsync(10, retryNumber => TimeSpan.FromSeconds(60), (ex, retry) =>
                {
                    LogHttpResponseMessage?.Invoke($"{ex.Result.Request.Resource}", ex.Result);
                });
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest restRequest)
        {
            return await clientRetryPolicy.ExecuteAsync(async () => { return await client.ExecuteAsync(restRequest); });
        }

        public RestRequest GetRequest()
        {
            RestRequest request = new RestRequest();
            request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
            request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
            request.Timeout = 1000 * 60 * 10;
            return request;
        }

        public RestRequest GetRequest(Method method, string resource)
        {
            RestRequest request = new RestRequest();
            request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
            request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
            request.Method = method;
            request.Timeout = 1000 * 60 * 10;
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
