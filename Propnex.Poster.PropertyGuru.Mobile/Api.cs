using Microsoft.Extensions.Logging;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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

            }
            return GetHttpResult<Listing.CreateOrUpdateListing>(response);
        }
    }

    public class QueryListing
    {
        public string Locale { get; set; } = "en";

        public string Region { get; set; } = "sg";

        public string include_suspended_photos { get; set; } = "true";

        public string status_code { get; set; } = "ACT";
    }
}
