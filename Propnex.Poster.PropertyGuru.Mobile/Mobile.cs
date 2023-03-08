using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class Mobile : ClientBase
    {
        public Token Token { get; set; }

        public Mobile(Token token) : base("https://bff-mobile.propertyguru.com")
        {
            Token = token;
        }

        public Mobile() : base("https://bff-mobile.propertyguru.com")
        {

        }

        public async Task<HttpResult<ListingsResult>> ListingManagementAsync(QueryListingManagement queryListingManagement)
        {

            var request = GetRequest();
            request.Method = Method.Get;
            request.Resource = "/v1/listingManagement";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Host", "bff-mobile.propertyguru.com");
            request.AddHeader("Connection", "keep-alive");
            request.AddQueryParameter("locale", queryListingManagement.Locale);
            request.AddQueryParameter("region", queryListingManagement.Region);
            request.AddQueryParameter("agent", queryListingManagement.Agent);
            request.AddQueryParameter("status_code", queryListingManagement.StatusCode);
            request.AddQueryParameter("order", queryListingManagement.Order);
            request.AddQueryParameter("limit", queryListingManagement.Limit);
            request.AddQueryParameter("page", queryListingManagement.Page);
            request.AddQueryParameter("sort", queryListingManagement.Sort);

            var response = await client.ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var listingResult = Newtonsoft.Json.JsonConvert.DeserializeObject<ListingsResult>(response.Content);
                return new HttpResult<ListingsResult>()
                {
                    Data = listingResult,
                    HttpStatusCode = response.StatusCode
                };
            }

            return GetHttpResult<ListingsResult>(response);
        }
    

        public async Task DeleteMediaAll(CreateOrUpdateListing listing)
        {
            foreach(var item in listing.media.listing)
            {
                await DeleteMedia(item.id.Value);
            }
            if (listing.media != null && listing.media.listingVideos != null)
            {
                for (var i = 0; i < listing.media.listingVideos.Count; i++)
                {
                    var item = listing.media.listingVideos[i];
                    await DeleteMedia(item.id.Value);
                }
            }
            if (listing.media != null && listing.media.listingVirtualTours != null)
            {
                for (var i = 0; i < listing.media.listingVirtualTours.Count; i++)
                {
                    var item = listing.media.listingVirtualTours[i];
                    await DeleteMedia(item.id.Value);
                }
            }

        }
        public async Task DeleteMedia(int mediaId)
        {
            var request = GetRequest();
            request.Method=Method.Delete;
            request.Resource = $"/v1/media?region=sg&mediaId={mediaId}";
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            var response = await client.ExecuteAsync(request);
        }
    }

    public class QueryListingManagement
    {

        public QueryListingManagement(string agent)
        {
            Agent=agent;
        }

        public string Locale { get; set; } = "en";

        public string Region { get; set; } = "sg";

        public string Agent { get; set; } = "153282";

        public string StatusCode { get; set; } = "ACT";

        public string Order { get; set; } = "desc";

        public string Limit { get; set; } = "100";

        public string Page { get; set; } = "1";

        public string Sort { get; set; } = "start_date";
    }

}
