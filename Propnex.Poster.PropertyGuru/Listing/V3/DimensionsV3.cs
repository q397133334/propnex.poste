using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>面积尺寸（建筑面积 + 土地面积）</summary>
    public class DimensionsV3
    {
        /// <summary>建筑面积（室内面积）</summary>
        [JsonProperty("floor", NullValueHandling = NullValueHandling.Ignore)]
        public FloorDimensionV3 Floor { get; set; }
    }
}
