using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class PriceV3
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("maintenanceFee", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaintenanceFee { get; set; }
    }
}