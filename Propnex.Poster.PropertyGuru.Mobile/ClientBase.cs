using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
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
            Log?.Invoke($"client {baseUrl}");
            clientRetryPolicy = Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 response.StatusCode==HttpStatusCode.Forbidden||
                 response.StatusCode == HttpStatusCode.Unauthorized ||
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
                UserAgent = ""

            })
            {

            };

            Log?.Invoke($"client {baseUrl}");
            clientRetryPolicy = Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 response.StatusCode == HttpStatusCode.Forbidden ||
                 response.StatusCode == HttpStatusCode.Unauthorized ||
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
            System.Threading.Thread.Sleep(Random.Next(1000, 5000));
            RestRequest request = new RestRequest();
            //request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
            //request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
            request.AddOrUpdateHeader(KnownHeaders.UserAgent, "sg;agentnet;android;2025.8.20;SM-N9760");
            //request.AddHeader("sentry-trace", "b6dda4d0b7894eacadc6c899e8b062f5-b80b761182153526");
            //request.AddHeader("sentry-trace", "5776dbfd7f464c7e8a2a2befeabf67de-0dddf6e780b34bff");
            //request.AddHeader("baggage", "sentry-environment=SG-android,sentry-public_key=f0e2339e8daa4ee5a7051e17d4c4037b," +
            //    "sentry-release=com.allproperty.android.agentnet%402025.8.20%2B301413145," +
            //    "sentry-sample_rand=0.3413986372724464,sentry-trace_id=5776dbfd7f464c7e8a2a2befeabf67de");
            request.Timeout = new TimeSpan(0, 5, 0);// 1000 * 60 * 10;
            request.Version = new Version(2, 0);
            return request;
        }


        static Random Random = new Random();

        public RestRequest GetRequest(Method method, string resource)
        {
            System.Threading.Thread.Sleep(Random.Next(1000, 5000));

            RestRequest request = new RestRequest();
            //request.AddHeader("x-clientid", "L7C9YKV9-ESF3606Q-GHF9H1F5-8LJMKRO5");
            //request.AddHeader("x-clientsecret", "jjiF916yVwfCRQEJtS6loHVDZ16mWPWf");
            request.AddOrUpdateHeader(KnownHeaders.UserAgent, "sg;agentnet;android;2025.8.20;SM-N9760");
            request.Method = method;
            request.Timeout = new TimeSpan(0, 5, 0);// 1000 * 60 * 10;
            request.Resource = resource;
            //request.Version = new Version(2, 0);
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
