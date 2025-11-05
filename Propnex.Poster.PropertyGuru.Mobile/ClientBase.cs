using Polly;
using Polly.Retry;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;

namespace Propnex.Poster.PropertyGuru.Mobile
{


    public class ClientBase : IDisposable
    {
        public static string PhoneModel = "23127PN0CC";
        protected readonly RestClient client;

        public static CookieContainer cookieContainer = new CookieContainer();

        public Action<string, RestResponse> LogHttpResponseMessage;

        public Action<string, bool> Log;

        public AsyncRetryPolicy<RestResponse> clientRetryPolicy;


        public ClientBase(string baseUrl)
        {

            client = new RestClient(new RestClientOptions()
            {
                BaseUrl = new Uri(baseUrl),
                UseDefaultCredentials = true,
                UserAgent = ""
            });

            Log?.Invoke($"client {baseUrl}", false);
            clientRetryPolicy = Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 response.StatusCode == 0 ||
                 response.StatusCode == HttpStatusCode.GatewayTimeout ||
                 response.StatusCode == HttpStatusCode.InternalServerError ||
                 response.StatusCode == HttpStatusCode.Forbidden ||
                 response.StatusCode == HttpStatusCode.Unauthorized ||
                 response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted)
                .WaitAndRetryAsync(3, retryNumber => TimeSpan.FromSeconds(60), (ex, retry) =>
                {
                    Log?.Invoke($"{ex.Result.Request.Resource} {ex.Result.ResponseStatus} {ex.Result?.Content} {ex.Exception?.Message}", false);
                    LogHttpResponseMessage?.Invoke($"{ex.Result.Request.Resource} {ex.Result.ResponseStatus}", ex.Result);
                });
        }

        public ClientBase(string baseUrl, string proxyIp)
        {
            var ip = proxyIp.Split(':')[0];
            var port = proxyIp.Split(':')[1];
            client = new RestClient(new RestClientOptions()
            {
                BaseUrl = new Uri(baseUrl),
                UseDefaultCredentials = true,
                UserAgent = "",
                Proxy = new WebProxy(ip, int.Parse(port))
            });

            Log?.Invoke($"client {baseUrl}", false);
            clientRetryPolicy = Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                  response.StatusCode == 0 ||
                 response.StatusCode == HttpStatusCode.GatewayTimeout ||
                 response.StatusCode == HttpStatusCode.InternalServerError ||
                 response.StatusCode == HttpStatusCode.Forbidden ||
                 response.StatusCode == HttpStatusCode.Unauthorized ||
                 response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted)

                .WaitAndRetryAsync(3, retryNumber => TimeSpan.FromSeconds(10), (ex, retry) =>
                {
                    Log?.Invoke($"{ex.Result.Request.Resource} {ex.Result.ResponseStatus} {ex.Result?.Content} {ex.Exception?.Message}", false);
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
            //request.AddOrUpdateHeader(KnownHeaders.UserAgent, $"AgentNet Android/LEGACY");
            request.AddOrUpdateHeader(KnownHeaders.UserAgent, $"sg;agentnet;android;2025.10.24;{PhoneModel}");
            request.Timeout = TimeSpan.FromMinutes(5);//  1000 * 60 * 5;
            request.Version = new Version(2, 0);
            request.CookieContainer = cookieContainer;
            return request;
        }


        static Random Random = new Random();

        public RestRequest GetRequest(Method method, string resource)
        {
            System.Threading.Thread.Sleep(Random.Next(1000, 5000));

            RestRequest request = new RestRequest();
            //request.AddOrUpdateHeader(KnownHeaders.UserAgent, $"AgentNet Android/LEGACY");
            request.AddOrUpdateHeader(KnownHeaders.UserAgent, $"sg;agentnet;android;2025.10.24;{PhoneModel}");
            request.Method = method;
            request.Timeout = TimeSpan.FromMinutes(5);// 1000 * 60 * 5;
            request.Resource = resource;
            request.Version = new Version(2, 0);
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

        public static RestClient Create(RestClientOptions restClientOptions)
        {
            var handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            // Allow TLS 1.0/1.1/1.2/1.3 (TLS 1.0/1.1 are deprecated; prefer TLS1.2+ in production)
            handler.SslOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols =  SslProtocols.Tls12 | SslProtocols.Tls13
            };

            // Try to set per-connection cipher suites where supported (may throw on some platforms)
            try
            {
                handler.SslOptions.CipherSuitesPolicy = new CipherSuitesPolicy(new[]
                {
                // AES/GCM / ChaCha20
                TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256,
                TlsCipherSuite.TLS_AES_128_GCM_SHA256,

                // ECDHE
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,

                // DHE / CBC fallbacks
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA
            });
            }
            catch (PlatformNotSupportedException)
            {
                // OS/.NET runtime doesn't support per-connection cipher suite policy.
                // Fall back to OS-managed cipher suites.
            }
            if (restClientOptions.Proxy != null)
                handler.Proxy = restClientOptions.Proxy;
            var httpClient = new HttpClient(handler, disposeHandler: true);
            return new RestClient(httpClient, restClientOptions);
        }

        public static HttpClient GetCipherHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            // Allow TLS 1.0 / 1.1 / 1.2 / 1.3 (note: TLS 1.0/1.1 are deprecated on many platforms)
            handler.SslOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            };

            // Try to set per-connection cipher suites where supported (.NET 5+ on supported OSes).
            try
            {
                handler.SslOptions.CipherSuitesPolicy = new CipherSuitesPolicy(new[]
                {
                // Preferred AEAD / modern suites
                TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256,
                TlsCipherSuite.TLS_AES_128_GCM_SHA256,

                // ECDHE suites
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,

                // DHE / CBC fallbacks (as in your Java list)
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA
            });
            }
            catch (PlatformNotSupportedException)
            {
                // Platform/OS doesn't support per-connection cipher suite policies.
                // The OS-wide cipher suite configuration will be used instead.
            }

            return new HttpClient(handler, disposeHandler: true);
        }
    }
}
