using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>楼盘信息（V3 API）</summary>
    public class ProjectV3
    {
        /// <summary>楼盘类型，目前固定为 verified（已认证楼盘）</summary>
        [JsonProperty("type")]
        public string Type { get; set; } = "verified";

        /// <summary>各类型楼盘的具体元数据</summary>
        [JsonProperty("metaByType")]
        public MetaByTypeV3 MetaByType { get; set; } = new MetaByTypeV3();
    }
}
