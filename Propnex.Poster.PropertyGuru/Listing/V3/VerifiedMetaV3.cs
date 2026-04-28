using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>已认证楼盘的元数据（V3 API）</summary>
    public class VerifiedMetaV3
    {
        /// <summary>PropertyGuru 楼盘的 verified 字符串 ID，如 "456z7h"（可从楼盘搜索接口获取）</summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>楼盘位置 ID（location_id），用于定位具体地址</summary>
        [JsonProperty("locationId", NullValueHandling = NullValueHandling.Ignore)]
        public int? LocationId { get; set; }

        /// <summary>物业类型信息（子类型代码）</summary>
        [JsonProperty("property", NullValueHandling = NullValueHandling.Ignore)]
        public VerifiedPropertyV3 Property { get; set; }
    }
}
