using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>日期节点中的单个日期对象</summary>
    public class DateItemV3
    {
        /// <summary>日期字符串，如 "2026-02-13T16:00:00.000Z" 或 "yyyy-MM-dd HH:mm:ss"</summary>
        [JsonProperty("date")]
        public string Date { get; set; }
    }

    /// <summary>挂牌日期信息（V3 API dates 节点）</summary>
    public class DatesV3
    {
        /// <summary>可入住日期，出租房源必填，出售可为 null</summary>
        [JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
        public DateItemV3 Available { get; set; }

        /// <summary>拍卖日期（auction），拍卖类房源专用</summary>
        [JsonProperty("auction", NullValueHandling = NullValueHandling.Ignore)]
        public DateItemV3 Auction { get; set; }

        /// <summary>到期日期（expiry）</summary>
        [JsonProperty("expiry", NullValueHandling = NullValueHandling.Ignore)]
        public DateItemV3 Expiry { get; set; }
    }
}
