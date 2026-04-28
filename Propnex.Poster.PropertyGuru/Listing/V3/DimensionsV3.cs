using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class DimensionsV3
    {
        [JsonProperty("floor", NullValueHandling = NullValueHandling.Ignore)]
        public FloorDimensionV3 Floor { get; set; }
    }
}