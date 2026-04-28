using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class AddressV3
    {
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("floor", NullValueHandling = NullValueHandling.Ignore)]
        public string Floor { get; set; }

        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [JsonProperty("maskUnitNumber")]
        public bool MaskUnitNumber { get; set; }
    }
}