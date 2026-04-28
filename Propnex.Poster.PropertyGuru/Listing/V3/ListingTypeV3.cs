using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>挂牌类型（V3 API）</summary>
    public class ListingTypeV3
    {
        /// <summary>类型代码：SALE（出售）/ RENT（出租）</summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
