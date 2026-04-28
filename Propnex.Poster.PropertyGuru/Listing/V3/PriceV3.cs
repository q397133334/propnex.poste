using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>价格信息（V3 API）</summary>
    public class PriceV3
    {
        /// <summary>价格金额（新加坡元），出售为总价，出租为月租金</summary>
        [JsonProperty("value")]
        public int Value { get; set; }

        /// <summary>管理费/维护费（新加坡元/月），无则为 null</summary>
        [JsonProperty("maintenanceFee", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaintenanceFee { get; set; }
    }
}
