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

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class Api : ClientBase
    {
        public Token Token { get; set; }

        public ILogger<Api> Logger { get; set; }

        public Api() : base("https://api.propertyguru.com")
        {
        }

        public Api(Token token) : base("https://api.propertyguru.com")
        {
            Token = token;
        }

        public async Task<HttpResult<Listing.CreateOrUpdateListing>> ListingsAsync(int id, QueryListing queryListing)
        {
            var request = GetRequest();
            request.Method = Method.Get;
            request.Resource = $"/v1/listings/{id}";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddParameter("locale", queryListing.Locale);
            request.AddParameter("region", queryListing.Region);
            request.AddParameter("include_suspended_photos", queryListing.include_suspended_photos);
            request.AddParameter("status_code", queryListing.status_code);

            var response = await client.ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var listing = Newtonsoft.Json.JsonConvert.DeserializeObject<Listing.CreateOrUpdateListing>(response.Content);
                return new HttpResult<Listing.CreateOrUpdateListing> { Data = listing, HttpStatusCode = System.Net.HttpStatusCode.OK };
            }
            return GetHttpResult<Listing.CreateOrUpdateListing>(response);
        }

        public async Task<HttpResult<List<QueryLocale>>> AutocompleteAsync(QueryAutocomplete queryAutocomplete)
        {
            var request = GetRequest();
            request.Method = Method.Get;
            request.Resource = $"/v1/autocomplete";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddParameter("locale", queryAutocomplete.Locale);
            request.AddParameter("region", queryAutocomplete.Region);
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

        public async Task<HttpResult<CreateOrUpdateListingResult>> CreateAsync(CreateOrUpdateListing createListing)
        {
            var request = GetRequest();
            request.Resource = "/v1/listings";
            request.Method = Method.Post;
            //request.AddHeader(KnownHeaders.Authorization, $"Bearer {Token.accessToken}");
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            //request.AddParameter("region", "sg");
            //request.AddJsonBody(createListing);
            request.AddStringBody(Newtonsoft.Json.JsonConvert.SerializeObject(createListing), DataFormat.Json);
            var response = await client.ExecuteAsync(request);
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

        public async Task UploadPhotoAsync(string ownerId, string sortOrder, string filePath)
        {
            await UploadMediaAsync(ownerId, "UPHO", "IMAGE", sortOrder, filePath);
        }
        public async Task UploadVideosAsync(string ownerId, string sortOrder, string filePath)
        {
            await UploadMediaAsync(ownerId, "UMOV", "MOVIE", sortOrder, filePath);
        }

        public async Task UplaodVirtualTours(string ownerId, string sortOrder, string filePath)
        {
            await UploadMediaAsync(ownerId, "UTOUR", "VTOUR", sortOrder, filePath);
        }

        public async Task UploadFlooplan(string ownerId, string sortOrder, string filePath)
        {
            await UploadMediaAsync(ownerId, "UFLOO", "IMAGE", sortOrder, filePath);
        }

        private async Task<HttpResult<string>> UploadMediaAsync(
            string ownerid,
            string mediaClass,
            string mediaType,
            string sortOrder,
            string filePath
            )
        {
            var request = GetRequest(Method.Post, "/v0/media");
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            request.AddParameter("locale", "en");
            request.AddParameter("ownerId", ownerid);
            request.AddParameter("region", "sg");
            request.AddParameter("mediaClass", mediaClass);
            request.AddParameter("mediaType", mediaType);
            request.AddParameter("userId", $"{Token.User.AgentId}");
            request.AddParameter("source", "AgentNet-android");
            request.AddParameter("sortOrder", sortOrder);
            request.AddParameter("statusCode", "ACT");
            if (filePath.Contains("youtube") ||
            filePath.Contains("vimeo") ||
                    filePath.Contains("dailymotion") ||
                    filePath.Contains("<iframe")
                    )
            {
                request.AddParameter("videoEmbedHtml", filePath);
                request.AlwaysMultipartFormData = true;
            }
            else
            {
                var files = File.ReadAllBytes(filePath);
                var fileName = Path.GetExtension(filePath);
                request.AddFile("mediaFile", files, $"{Guid.NewGuid()}{filePath}");
            }
            try
            {
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new HttpResult<string>()
                    {
                        Data = response.Content,
                        HttpStatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                return GetHttpResult<string>(response);
            }
            catch(Exception ex)
            {

            }
            return null;
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
}
