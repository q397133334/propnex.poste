using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>升降机信息（工业/商业房源专用）</summary>
    public class LiftV3
    {
        /// <summary>货梯数量</summary>
        [JsonProperty("cargo", NullValueHandling = NullValueHandling.Ignore)]
        public int? Cargo { get; set; }

        /// <summary>客梯总数</summary>
        [JsonProperty("totalPassenger", NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalPassenger { get; set; }

        /// <summary>货梯承载重量（吨），不适用时为 null</summary>
        [JsonProperty("capacity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Capacity { get; set; }
    }
}
