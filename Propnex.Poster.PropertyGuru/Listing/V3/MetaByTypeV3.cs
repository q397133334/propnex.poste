using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>楼盘类型元数据容器（V3 API）</summary>
    public class MetaByTypeV3
    {
        /// <summary>已认证楼盘的详细信息</summary>
        [JsonProperty("verified")]
        public VerifiedMetaV3 Verified { get; set; }
    }
}
