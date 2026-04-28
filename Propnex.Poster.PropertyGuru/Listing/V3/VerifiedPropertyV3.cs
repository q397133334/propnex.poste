using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class VerifiedPropertyV3
    {
        [JsonProperty("subType")]
        public string SubType { get; set; }
    }
}