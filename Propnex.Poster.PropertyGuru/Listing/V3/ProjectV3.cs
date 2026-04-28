using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class ProjectV3
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "verified";

        [JsonProperty("metaByType")]
        public MetaByTypeV3 MetaByType { get; set; }
    }
}