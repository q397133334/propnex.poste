using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>建筑面积详情</summary>
    public class FloorDimensionV3
    {
        /// <summary>面积数值及单位</summary>
        [JsonProperty("size")]
        public SizeV3 Size { get; set; }
    }
}
