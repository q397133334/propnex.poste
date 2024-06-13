using Microsoft.Extensions.Logging;
using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Util;
using System.Net;
using System.Threading;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class Api : ClientBase
    {

        private const string baseUrl = "https://listing.propertyguru.com";

        public Token Token { get; set; }

        public ILogger<Api> Logger { get; set; }

        public Api() : base(baseUrl)
        {
        }

        public Api(Token token) : base(baseUrl)
        {
            Token = token;
        }

        public Api(Token token, string proxyIp) : base(baseUrl, proxyIp)
        {
            Token = token;
        }

        public async Task<HttpResult<Listing.CreateOrUpdateListing>> ListingsAsync(int id, QueryListing queryListing)
        {
            var request = GetRequest();
            request.Method = Method.Get;
            request.Resource = $"/v1/listings/{id}";
            request.AddParameter("agentId", $"{Token.User.AgentId}");
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddParameter("locale", queryListing.Locale);
            request.AddParameter("region", queryListing.Region);
            request.AddParameter("include_suspended_photos", queryListing.include_suspended_photos);
            request.AddParameter("status_code", queryListing.status_code);

            var response = await ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var listing = Newtonsoft.Json.JsonConvert.DeserializeObject<Listing.CreateOrUpdateListing>(response.Content);
                return new HttpResult<Listing.CreateOrUpdateListing> { Data = listing, HttpStatusCode = System.Net.HttpStatusCode.OK };
            }
            return GetHttpResult<Listing.CreateOrUpdateListing>(response);
        }

        public async Task<HttpResult<List<QueryLocale>>> AutocompleteAsync(QueryAutocomplete queryAutocomplete)
        {
            using (var client = new RestSharp.RestClient("https://prefix-search.propertyguru.com/v1/sg/autocomplete"))
            {
                var request = GetRequest();
                request.Method = Method.Get;
                request.Resource = $"/v1/autocomplete";
                if (Token != null)
                {
                    request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
                }
                request.AddParameter("locale", queryAutocomplete.Locale);
                //request.AddParameter("region", queryAutocomplete.Region);
                request.AddParameter("query", queryAutocomplete.Query);
                request.AddParameter("limit", queryAutocomplete.Limit);
                request.AddParameter("objectType", queryAutocomplete.ObjectType);
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new HttpResult<List<QueryLocale>>()
                    {
                        HttpStatusCode = response.StatusCode,
                        Data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<QueryLocale>>(response.Content)
                    };
                }
                return GetHttpResult<List<QueryLocale>>(response);
            }
        }

        public async Task<HttpResult<CreateOrUpdateListingResult>> CreateAsync(CreateOrUpdateListing createListing)
        {
            //using (var client=new RestClient(new RestClientOptions()
            //{
            //    BaseUrl = new Uri("https://agentnet.propertyguru.com.sg"),
            //    MaxTimeout = 1000 * 60 * 10,
            //    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36"
            //}))
            //{
            var request = GetRequest();
            request.Resource = $"/v1/listings?region=sg&agentId={Token.User.AgentId}";
            //request.Resource = "/v2/create-listing/detail";
            //request.AddParameter("region", "sg");
            //request.AddParameter("agentId", $"{Token.User.AgentId}");
            request.Method = Method.Post;
            request.AddHeader("x-logger-edited-by", $"{Token.User.AgentId}");
            request.AddHeader(KnownHeaders.Authorization, $"Bearer {Token.accessToken}");
            //createListing.agent = new Agent();
            //createListing.media = null;
            var stringBody = Newtonsoft.Json.JsonConvert.SerializeObject(createListing);
            request.AddStringBody(stringBody, DataFormat.Json);
            var response = await ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                return new HttpResult<CreateOrUpdateListingResult>()
                {
                    Data = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateOrUpdateListingResult>(response.Content),
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                };
            }
            return GetHttpResult<CreateOrUpdateListingResult>(response);
            //}
        }

        public async Task<HttpResult<CreateOrUpdateListing>> GetListing(int listingId, string statusCode = "ACT")
        {
            var request = GetRequest();
            request.Resource = $"/v1/listings/{listingId}";
            request.Method = Method.Get;
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            request.AddParameter("locale", "en");
            request.AddParameter("region", "sg");
            request.AddParameter("agentId", $"{Token.User.AgentId}");
            request.AddParameter("status_code[]", "ACT");
            request.AddParameter("status_code[]", "COMP");
            request.AddParameter("status_code[]", "DEL");
            request.AddParameter("status_code[]", "DRAFT");
            request.AddParameter("status_code[]", "EXP");
            request.AddParameter("status_code[]", "SUSP");
            request.AddParameter("status_code[]", "PAUSE");
            request.AddParameter("include_suspended_photos", "1");
            request.AddParameter("forceMasterConnection", "1");

            //request.AddParameter("include_suspended_photos", "true");
            //request.AddParameter("status_code", statusCode);

            var response = await ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new HttpResult<CreateOrUpdateListing>()
                {
                    Data = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateOrUpdateListing>(response.Content),
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                };
            }
            return GetHttpResult<CreateOrUpdateListing>(response);
        }

        public async Task<HttpResult<CreateOrUpdateListingResult>> UpdateAsync(CreateOrUpdateListing createListing)
        {
            var request = GetRequest();
            request.Resource = $"/v1/listings/{createListing.id}?region=sg&agentId={Token.User.AgentId}";
            //request.AddParameter("region", "sg");
            //request.AddParameter("agentId", $"{Token.User.AgentId}");
            request.Method = Method.Put;
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            request.AddStringBody(Newtonsoft.Json.JsonConvert.SerializeObject(createListing), DataFormat.Json);
            var response = await ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new HttpResult<CreateOrUpdateListingResult>()
                {
                    Data = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateOrUpdateListingResult>(response.Content),
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                };
            }
            return GetHttpResult<CreateOrUpdateListingResult>(response);
        }

        public async Task<HttpResult<string>> UploadPhotoAsync(string ownerId, string sortOrder, string filePath)
        {
            return await UploadMediaAsync(ownerId, "UPHO", "IMAGE", sortOrder, filePath);
        }
        public async Task<HttpResult<string>> UploadVideosAsync(string ownerId, string sortOrder, string filePath)
        {
            return await UploadMediaAsync(ownerId, "UMOV", "MOVIE", sortOrder, filePath);
        }

        public async Task<HttpResult<string>> UplaodVirtualTours(string ownerId, string sortOrder, string filePath)
        {
            return await UploadMediaAsync(ownerId, "UTOUR", "VTOUR", sortOrder, filePath);
        }

        public async Task<HttpResult<string>> UploadFlooplan(string ownerId, string sortOrder, string filePath)
        {
            return await UploadMediaAsync(ownerId, "UFLOO", "IMAGE", sortOrder, filePath);
        }

        private async Task<HttpResult<string>> UploadMediaAsync(
            string ownerid,
            string mediaClass,
            string mediaType,
            string sortOrder,
            string filePath
            )
        {
            var request = GetRequest(Method.Post, $"/sf2-agent/ajax/listings/{ownerid}/media");
            //request.Authenticator = new JwtAuthenticator(Token.accessToken);
            request.AddParameter("ownerId", ownerid);
            request.AddParameter("mediaClass", mediaClass);
            request.AddParameter("mediaType", mediaType);
            request.AddParameter("userId", $"{Token.User.UserId}");
            request.AddParameter("source", "AgentNet");
            request.AddParameter("sortOrder", 1);
            request.AddParameter("caption", "");
            request.AddParameter("statusCode", "ACT");
            var filePathLower = filePath.ToLower();
            if (filePathLower.Contains("youtube") ||
                filePathLower.Contains("youtu.be")||
                filePathLower.Contains("vimeo") ||
                    filePathLower.Contains("dailymotion") ||
                    filePathLower.Contains("<iframe") ||
                    filePathLower.Contains("havelock2") ||
                    filePathLower.Contains("new-vr") ||
                    filePathLower.Contains("8prop.com") ||
                    filePathLower.Contains("matterport.com")
                    )
            {
                if (filePath.Contains("#"))
                {
                    filePath = filePath.Split('#')[0];
                }
                filePath=System.Web.HttpUtility.UrlDecode(filePath);
                request.AddParameter("videoEmbedHtml", filePath);
                request.AlwaysMultipartFormData = true;
            }
            else
            {
                if (filePath == "")
                {
                    return new HttpResult<string>() { };
                }
                if (File.Exists(filePath) == false)
                {
                    return new HttpResult<string>() { };
                }
                var files = File.ReadAllBytes(filePath);
                var fileName = Path.GetExtension(filePath);
                request.AddFile("mediaFile", files, $"{Guid.NewGuid()}{filePath}");
            }
            int count = 0;
        Start:
            count++;
            using (var c = new RestSharp.RestClient(new RestClientOptions()
            {
                BaseUrl = new Uri("https://agentnet.propertyguru.com.sg"),
                MaxTimeout = 1000 * 60 * 10,
                Proxy = client.Options.Proxy ?? null,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36"
            }))
            {
                request.AlwaysMultipartFormData = true;
                cookie = cookie == null ? await GetCookie() : cookie;
                if (cookie == null)
                {
                    return new HttpResult<string>()
                    {
                        Data = "get cookie faile",
                        HttpStatusCode = System.Net.HttpStatusCode.Forbidden
                    };
                }
                request.AddHeader("origin", "https://agentnet.propertyguru.com.sg");
                request.AddHeader("referer", $"https://agentnet.propertyguru.com.sg/v2/create-listing/media/{ownerid}");
                request.AddHeader("cookie", cookie);
                var response = await ExecuteAsync(request, c);// c.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Log("media upload success");
                    return new HttpResult<string>()
                    {
                        Data = response.Content,
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    if (count < 2)
                    {
                        Log($"media upload errir {response.StatusCode},{response.ErrorMessage}-{response.StatusDescription}");
                        cookie = null;
                        Log("Waiting 10min");
                        await Task.Delay(1000 * 60 * 10);
                        Log("retry get cookie");
                        goto Start;
                    }
                }
                Log($"media upload errir {response.StatusCode}");
                return GetHttpResult<string>(response);
            }
        }
        string cookie = null;
        public async Task<string> GetCookie()
        {
            using (var c = new RestClient(new RestClientOptions()
            {
                BaseUrl = new Uri("https://api.zenrows.com")
                //Proxy = new System.Net.WebProxy("127.0.0.1", int.Parse("8888"))
            }))
            {
                var request = new RestRequest();
                request.Resource = $"/v1/?apikey=355de58ca556aff2ca943242562717c5178c71cb";
                request.Method = Method.Get;
                //request.AddParameter("apiKey", "8338c842866acc825ac9a7a4ca5cf057c77960f1");
                request.AddParameter("url", $"https://agentnet.propertyguru.com.sg/oauth/callback/pgaccount?state=%2Fex_home&locale=en&access_token={Token.accessToken}&remember=1", true);
                //request.AddParameter("js_render", "true");
                request.AddParameter("premium_proxy", "true");
                request.AddParameter("original_status", "true");
                // request.AddUrlSegment("url", $"https://agentnet.propertyguru.com.sg/oauth/callback/pgaccount?state=%2Fex_home&locale=en&access_token={Token.accessToken}&remember=1&premium_proxy=true&original_status=true",true);
                var result = await clientRetryPolicy.ExecuteAsync(async () => { return await c.ExecuteAsync(request); });
                Log?.Invoke($"get cookie {result.Headers.Count},{result.StatusCode}");
                return result.Headers.Where(q => q.Name == "Zr-Cookies").FirstOrDefault().Value.ToString();
            }
        }

    }
}

public class QueryAutocomplete
{
    public QueryAutocomplete(string query)
    {
        Query = query;
    }

    public string Query { get; set; }

    public string Locale { get; set; } = "en";

    public string Region { get; set; } = "sg";

    public int Limit { get; set; } = 100;

    public string ObjectType { get; set; } = "PROPERTY";

}

public class QueryListing
{
    public string Locale { get; set; } = "en";

    public string Region { get; set; } = "sg";

    public string include_suspended_photos { get; set; } = "true";

    public string status_code { get; set; } = "ACT";
}
