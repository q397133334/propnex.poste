using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
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

        public async Task<HttpResult<bool>> Activate(int listingId, int credit = 1)
        {
            var request = GetRequest();
            request.Resource = "/api/v1/listing/activate";
            request.Method = RestSharp.Method.Post;
            request.AddJsonBody(new
            {
                listingId = 0,
                expectedCredit = 0,
                userId = 0,
                origin = "sg"
            });

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return new HttpResult<bool>()
                {
                    HttpStatusCode = response.StatusCode,
                    Data = true
                };
            }
            return GetHttpResult<bool>(response);
        }
    }
}
