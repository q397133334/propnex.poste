using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class VerifiedMetaV3
    {
        /// <summary>PropertyGuru 楼盘 verified string ID，如 "vseifm"</summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("locationId", NullValueHandling = NullValueHandling.Ignore)]
        public int? LocationId { get; set; }

        [JsonProperty("property", NullValueHandling = NullValueHandling.Ignore)]
        public VerifiedPropertyV3 Property { get; set; }
    }
}