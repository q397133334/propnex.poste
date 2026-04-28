using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class LocationV3
    {
        [JsonProperty("address")]
        public AddressV3 Address { get; set; }
    }
}