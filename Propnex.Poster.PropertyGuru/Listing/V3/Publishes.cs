using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class CreditKey
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }
    }

    public class PublishRequest
    {
        [JsonProperty("publishes")]
        public List<CreditKey> Publishes { get; set; } = new List<CreditKey>();

        public string currencyTypeCode { get; set; } = "ad_credit";
        //listing-management-mobile-android-single
        //listing-management-mobile-ios-single
        //listing-creation-desktop-web
        public string origin { get; set; } = "listing-management-mobile-android-single";
    }

    public class RepostRequest
    {
        [JsonProperty("reposts")]
        public List<CreditKey> Reposts { get; set; } = new List<CreditKey>();

        public string currencyTypeCode { get; set; } = "ad_credit";
        //listing-management-mobile-android-single
        //listing-management-mobile-ios-single
        //listing-creation-desktop-web
        public string origin { get; set; } = "listing-management-mobile-android-single";
    }

    public class ResponsePublisheResult
    {
        public List<Result> Results { get; set; } = new List<Result>();
}

    public class Result
    {
        public int ListingId { get; set; }

        public int listerId { get; set; }

        public string Brand { get; set; }

        public bool IsSuccess { get; set; }
    }
}
