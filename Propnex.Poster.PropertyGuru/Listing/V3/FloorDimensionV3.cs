using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class FloorDimensionV3
    {
        [JsonProperty("size")]
        public SizeV3 Size { get; set; }
    }
}