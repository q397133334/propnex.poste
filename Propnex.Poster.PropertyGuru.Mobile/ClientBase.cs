using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public AsyncRetryPolicy<RestResponse> clientRetryPolicy;


        public ClientBase(string baseUrl)
        {

            client = new RestSharp.RestClient(new RestClientOptions()
            {
                BaseUrl = new Uri(baseUrl),
                UseDefaultCredentials = true,
                UserAgent = ""
            });
            client.AddDefaultHeader(KnownHeaders.UserAgent, "sg;agentnet;android;2025.8.20;M973Q;null;");
            Log?.Invoke($"client {baseUrl}");
            clientRetryPolicy = Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted) //||
                                                                    //response.ResponseStatus == ResponseStatus.Error)
                .WaitAndRetryAsync(10, retryNumber => TimeSpan.FromSeconds(60), (ex, retry) =>
                {
                    LogHttpResponseMessage?.Invoke($"{ex.Result.Request.Resource} {ex.Result.ResponseStatus}", ex.Result);
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
                Proxy = new System.Net.WebProxy(ip, int.Parse(port)),

            })
            {

            };
            client.AddDefaultHeader(KnownHeaders.UserAgent, "sg;agentnet;android;2025.8.21;M973Q;null;LEGACY");
            Log?.Invoke($"client {baseUrl}");
            clientRetryPolicy = Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted)//||
                                                                   //response.ResponseStatus == ResponseStatus.Error)

                .WaitAndRetryAsync(10, retryNumber => TimeSpan.FromSeconds(60), (ex, retry) =>
                {
                    LogHttpResponseMessage?.Invoke($"{ex.Result.Request.Resource} {ex.Result.ResponseStatus}", ex.Result);
                });
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest restRequest, RestClient _client = null)
        {
           
            _client = _client == null ? client : _client;

            return await clientRetryPolicy.ExecuteAsync(async () => { return await _client.ExecuteAsync(restRequest); });
        }

        public RestRequest GetRequest()
        {
            RestRequest request = new RestRequest();
            //request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
            //request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
            request.AddOrUpdateHeader(KnownHeaders.UserAgent, "sg;agentnet;android;2025.8.21;M973Q;null;");
            request.Timeout = new TimeSpan(0, 5, 0);// 1000 * 60 * 10;
            request.Version = new Version(2, 0);
            return request;
        }

        public RestRequest GetRequest(Method method, string resource)
        {
            RestRequest request = new RestRequest();
            //request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
            //request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
            request.AddOrUpdateHeader(KnownHeaders.UserAgent, "sg;agentnet;android;2025.8.21;M973Q;null;");
            request.Method = method;
            request.Timeout = new TimeSpan(0, 5, 0);// 1000 * 60 * 10;
            request.Resource = resource;
            request.Version = new Version(2, 0);
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
