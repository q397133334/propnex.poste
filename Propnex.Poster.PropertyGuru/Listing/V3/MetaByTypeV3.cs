using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class MetaByTypeV3
    {
        [JsonProperty("verified")]
        public VerifiedMetaV3 Verified { get; set; }
    }
}