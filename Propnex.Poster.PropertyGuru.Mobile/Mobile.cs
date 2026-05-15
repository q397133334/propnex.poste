using Microsoft.Extensions.Logging;
using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class Mobile : ClientBase
    {

        private const string baseUrl = "https://bff-mobile.propertyguru.com";

        public Token Token { get; set; }

        public Mobile(Token token) : base(baseUrl)
        {
            Token = token;
        }

        public Mobile() : base(baseUrl)
        {

        }

        public Mobile(Token token, string proxyIp) : base(baseUrl, proxyIp)
        {
            Token = token;
        }

        public async Task<HttpResult<string>> Dashboard(string agentId)
        {
            var request = GetRequest();
            request.Resource = $"/v1/dashboard?locale=en&region=sg&agentId={agentId}";
            request.Method = Method.Get;
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            request.AddOrUpdateHeader(KnownHeaders.Accept, "application/json, application/xml, text/xml");
            var reponse = await ExecuteAsync(request);
            if (reponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new HttpResult<string>() { };
            }
            return GetHttpResult<string>(reponse);
        }

        public async Task<HttpResult<ListingsResult>> ListingManagementAsync(QueryListingManagement queryListingManagement)
        {

            var request = GetRequest();
            request.Method = Method.Get;
            request.Resource = "/v1/listingManagement";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddQueryParameter("locale", queryListingManagement.Locale);
            request.AddQueryParameter("region", queryListingManagement.Region);
            request.AddQueryParameter("agent", queryListingManagement.Agent);
            request.AddQueryParameter("status_code", queryListingManagement.StatusCode);
            request.AddQueryParameter("order", queryListingManagement.Order);
            request.AddQueryParameter("limit", queryListingManagement.Limit);
            request.AddQueryParameter("page", queryListingManagement.Page);
            request.AddQueryParameter("sort", queryListingManagement.Sort);

            var response = await ExecuteAsync(request);
            //Log(response.Content,false);
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

        public async Task<HttpResult<string>> DeleteListing(List<int> ids)
        {
            var request = GetRequest();
            request.Method = Method.Post;
            request.Resource = "/v1/listingManagement/delist?region=sg";
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            request.AddJsonBody(new
            {
                listingIds = ids,
                agentId = Token.User.AgentId,
                statusCode = "DEL",
                origin = "mobile-android-bulk"
            });
            var reponse = await ExecuteAsync(request);
            if (reponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //cookieContainer = new CookieContainer();
                //foreach (Cookie item in reponse.Cookies)
                //{
                //    cookieContainer.Add(item);
                //}
                return new HttpResult<string>() { };
            }
            return GetHttpResult<string>(reponse);
        }

        public async Task DeleteMediaAll(CreateOrUpdateListing listing)
        {
            foreach (var item in listing.media.listing)
            {
                await DeleteMedia(item.id.Value);
            }
            if (listing.media != null && listing.media.listingFloorplans != null)
                foreach (var item in listing.media.listingFloorplans)
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
            request.Method = Method.Delete;
            request.Resource = $"/v1/media?region=sg&mediaId={mediaId}";
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            var response = await ExecuteAsync(request);
        }
    }

    public class QueryListingManagement
    {

        public QueryListingManagement(string agent)
        {
            Agent = agent;
        }

        public string Locale { get; set; } = "en";

        public string Region { get; set; } = "sg";

        public string Agent { get; set; } = "153282";

        public string StatusCode { get; set; } = "ACT";

        public string Order { get; set; } = "desc";

        public string Limit { get; set; } = "50";

        public string Page { get; set; } = "1";

        public string Sort { get; set; } = "updated_date";
    }

}
