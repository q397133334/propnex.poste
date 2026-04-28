using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class ListingTypeV3
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}