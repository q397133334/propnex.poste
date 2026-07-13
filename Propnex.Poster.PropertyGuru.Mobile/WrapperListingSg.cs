using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Listing.V3;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class WrapperListingSg : ClientBase
    {
        public const string baseUrl = "https://wrapper-listings-sg.propertyguru.com";

        public Token Token { get; set; }
        public WrapperListingSg() : base(baseUrl)
        {
        }

        public WrapperListingSg(Dto.Token token) : base(baseUrl)
        {
            Token = token;
        }

        public WrapperListingSg(Dto.Token token, string proxyIp) : base(baseUrl, proxyIp)
        {
            Token = token;
        }

        public async Task<HttpResult<Offerings>> Offerings(string listingId)
        {
            var request = GetRequest();
            request.Method = Method.Get;
            request.Resource = $"/v1/ads-products/offerings?listingId={listingId}&t={DateTime.Now.ToLongTimeString()}&groupTypeCodes=activate";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddHeader("x-source", "mobile");
            request.AddHeader("x-brand", "pg");
            request.AddHeader("x-market", "pg");
            request.AddHeader("x-region", "sg");
            request.AddHeader("x-requested-with", "com.allproperty.android.agentnet");
            var response = await ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var offerings = Newtonsoft.Json.JsonConvert.DeserializeObject<Offerings>(response.Content);
                return new HttpResult<Offerings> { Data = offerings, HttpStatusCode = System.Net.HttpStatusCode.OK };
            }
            return GetHttpResult<Offerings>(response);
        }

        public async Task<HttpResult<ResponsePublisheResult>> Publish(Publishe publishe, string listingId)
        {
            var request = GetRequest();
            request.Method = Method.Post;
            request.Resource = $"/v1/listings/{listingId}/publish";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddHeader("x-source", "mobile");
            request.AddHeader("x-brand", "pg");
            request.AddHeader("x-market", "pg");
            request.AddHeader("x-region", "sg");
            request.AddHeader("x-requested-with", "com.allproperty.android.agentnet");
            request.AddJsonBody(new PublishRequest()
            {
                Publishes = new List<Publishe>() { publishe }
            });
            var response = await ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responsePublisheResult = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponsePublisheResult>(response.Content);
                return new HttpResult<ResponsePublisheResult> { Data = responsePublisheResult, HttpStatusCode = System.Net.HttpStatusCode.OK };
            }
            return GetHttpResult<ResponsePublisheResult>(response);
        }

        public async Task<HttpResult<Listing.V3.ListingModel>> Listings(int listingId)
        {
            var request = GetRequest();
            request.Method = Method.Get;
            request.Resource = $"/v1/listings/{listingId}?t={DateTime.Now.ToLongTimeString()}";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            request.AddHeader("x-source", "mobile");
            request.AddHeader("x-brand", "pg");
            request.AddHeader("x-market", "pg");
            request.AddHeader("x-region", "sg");
            request.AddHeader("x-requested-with", "com.allproperty.android.agentnet");
            var response = await ExecuteAsync(request);
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responsePublisheResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Listing.V3.ListingModel>(response.Content);
                    return new HttpResult<Listing.V3.ListingModel> { Data = responsePublisheResult, HttpStatusCode = System.Net.HttpStatusCode.OK };
                }
                return GetHttpResult<Listing.V3.ListingModel>(response);
            }
            catch(Exception ex)
            {
                return new HttpResult<Listing.V3.ListingModel>();
            }
        }
    }
}
