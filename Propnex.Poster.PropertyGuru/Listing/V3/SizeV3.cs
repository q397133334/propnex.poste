using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>面积数值（V3 API）</summary>
    public class SizeV3
    {
        /// <summary>面积数值（整数）</summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public int? Value { get; set; }

        /// <summary>面积单位，固定为 sqft（平方英尺）</summary>
        [JsonProperty("uom")]
        public string Uom { get; set; } = "sqft";
    }
}
