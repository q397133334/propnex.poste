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
    public class AdsProduct : ClientBase
    {
        public Token Token { get; set; }

        public AdsProduct() : base("https://ads-products.propertyguru.com") { }

        public AdsProduct(Token token) : this()
        {
            Token = token;
        }

        public async Task<HttpResult<string>> Activate(int listingId, int credit = 1)
        {
            var request = GetRequest();
            request.Resource = "/api/v1/listing/activate?region=sg";
            request.Method = RestSharp.Method.Post;
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            request.AddJsonBody(new
            {
                listingId = listingId,
                expectedCredit = credit,
                userId = Token.User.UserId,
                origin = "listing-creation-mobile-android"
            });

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return new HttpResult<string>()
                {
                    HttpStatusCode = response.StatusCode,
                    Data = ""
                };
            }
            return GetHttpResult<string>(response);
        }

        public async Task<HttpResult<string>> Repost(int listingId, int credit = 1)
        {
            var request = GetRequest();
            request.Resource = "/api/v1/listing/repost?region=sg";
            request.Method = RestSharp.Method.Post;
            request.Authenticator = new JwtAuthenticator(Token.accessToken);
            request.AddJsonBody(new
            {
                listingId = listingId,
                expectedCredit = credit,
                userId = Token.User.UserId,
                origin = "listing-creation-mobile-android"
            });

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return new HttpResult<string>()
                {
                    HttpStatusCode = response.StatusCode,
                    Data = ""
                };
            }
            return GetHttpResult<string>(response);
        }
    }
}
