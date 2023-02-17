using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
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


        public async Task ListingManagement(QueryListingManagement queryListingManagement)
        {

            var request = GetRequest();
            request.AddHeader("authorization", $"Bearer {Token.accessToken}");
            request.AddQueryParameter("locale", queryListingManagement.Locale);
            request.AddQueryParameter("region", queryListingManagement.Region);
            request.AddQueryParameter("agent", queryListingManagement.Agent);
            request.AddQueryParameter("status_code", queryListingManagement.StatusCode);
            request.AddQueryParameter("order", queryListingManagement.Order);
            request.AddQueryParameter("limit", queryListingManagement.Limit);
            request.AddQueryParameter("page", queryListingManagement.Page);
            request.AddQueryParameter("sort", queryListingManagement.Sort);

            var response = await client.ExecuteAsync(request);
            switch(response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    break;
                case System.Net.HttpStatusCode.Forbidden: 
                    break;
                case System.Net.HttpStatusCode.NotFound: 
                    break;
            }
        }
    }

    public class QueryListingManagement
    {
        public string Locale { get; set; } = "en";

        public string Region { get; set; } = "sg";

        public string Agent { get; set; }

        public string StatusCode { get; set; } = "ACT";

        public string Order { get; set; } = "desc";

        public string Limit { get; set; }

        public string Page { get; set; }

        public string Sort { get; set; } = "start_date";
    }

}
