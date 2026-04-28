using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class ConfigurationV3
    {
        [JsonProperty("bedrooms", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bedrooms { get; set; }

        [JsonProperty("bathrooms", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bathrooms { get; set; }
    }
}