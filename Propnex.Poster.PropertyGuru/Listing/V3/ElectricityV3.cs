using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>电力相数信息</summary>
    public class ElectricityPhaseV3
    {
        /// <summary>相数代码：1（单相）/ 3（三相）</summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    /// <summary>电力供应信息（工业/商业房源专用）</summary>
    public class ElectricityV3
    {
        /// <summary>电力相数</summary>
        [JsonProperty("phase", NullValueHandling = NullValueHandling.Ignore)]
        public ElectricityPhaseV3 Phase { get; set; }

        /// <summary>供电量（安培数），如 60 / 100 / 200</summary>
        [JsonProperty("supply", NullValueHandling = NullValueHandling.Ignore)]
        public int? Supply { get; set; }
    }
}
