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

        public async Task<HttpResult<Offerings>> AdsProducts(string listingId)
        {
            var request = GetRequest();
            request.Method = Method.Get;
            request.Resource = $"/v1/ads-products/offerings?listingId={listingId}&t={DateTime.Now.ToLongTimeString()}&groupTypeCodes=activate";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
            var response = await ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var offerings = Newtonsoft.Json.JsonConvert.DeserializeObject<Offerings>(response.Content);
                return new HttpResult<Offerings> { Data = offerings, HttpStatusCode = System.Net.HttpStatusCode.OK };
            }
            return GetHttpResult<Offerings>(response);
        }

        public async Task<HttpResult<ResponsePublisheResult>> Publish(Publishe publishe)
        {
            var request = GetRequest();
            request.Method = Method.Post;
            request.Resource = $"/v1/listings";
            request.AddHeader("Authorization", $"Bearer {Token.accessToken}");
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
    }
}
