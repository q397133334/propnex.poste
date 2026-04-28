using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class SizeV3
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public int? Value { get; set; }

        [JsonProperty("uom")]
        public string Uom { get; set; } = "sqft";
    }
}