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
        public static string PhoneModel = "23127PN0CC";
        protected readonly RestClient client;

        public static CookieContainer cookieContainer = new CookieContainer();

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
                 response.StatusCode == 0 ||
                 response.StatusCode == HttpStatusCode.GatewayTimeout ||
                 response.StatusCode == HttpStatusCode.InternalServerError ||
                 response.StatusCode == HttpStatusCode.Forbidden ||
                 response.StatusCode == HttpStatusCode.Unauthorized ||
                 response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted )
                .WaitAndRetryAsync(3, retryNumber => TimeSpan.FromSeconds(60), (ex, retry) =>
                {
                    Log?.Invoke($"{ex.Result.Request.Resource} {ex.Result.ResponseStatus} {ex.Exception?.Message}");
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
                  response.StatusCode == 0 ||
                 response.StatusCode == HttpStatusCode.GatewayTimeout ||
                 response.StatusCode == HttpStatusCode.InternalServerError ||
                 response.StatusCode == HttpStatusCode.Forbidden ||
                 response.StatusCode == HttpStatusCode.Unauthorized ||
                 response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted )

                .WaitAndRetryAsync(3, retryNumber => TimeSpan.FromSeconds(10), (ex, retry) =>
                {
                    Log?.Invoke($"{ex.Result.Request.Resource} {ex.Result.ResponseStatus} {ex.Exception?.Message}");
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
            request.AddOrUpdateHeader(KnownHeaders.UserAgent, $"sg;agentnet;android;2025.10.24;{PhoneModel}");
            request.Timeout = new TimeSpan(0, 5, 0);// 1000 * 60 * 10;
            //request.Version = new Version(2, 0);
            request.CookieContainer = cookieContainer;
            return request;
        }


        static Random Random = new Random();

        public RestRequest GetRequest(Method method, string resource)
        {
            System.Threading.Thread.Sleep(Random.Next(1000, 5000));

            RestRequest request = new RestRequest();
            request.AddOrUpdateHeader(KnownHeaders.UserAgent, $"sg;agentnet;android;2025.10.24;{PhoneModel}");
            request.Method = method;
            request.Timeout = new TimeSpan(0, 5, 0);// 1000 * 60 * 10;
            request.Resource = resource;
            //request.Version = new Version(2, 0);
            request.CookieContainer = cookieContainer;
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
