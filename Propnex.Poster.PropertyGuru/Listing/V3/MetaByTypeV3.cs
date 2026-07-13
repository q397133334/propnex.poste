using Newtonsoft.Json;
using System.Collections.Generic;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>楼盘类型元数据容器（V3 API）</summary>
    public class MetaByTypeV3
    {
        /// <summary>已认证楼盘的详细信息</summary>
        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public VerifiedMetaV3 Verified { get; set; } = null;

        [JsonProperty("unverified", NullValueHandling = NullValueHandling.Ignore)]
        public UnverifiedV3 unverified { get; set; } = null;
    }

    public class UnverifiedV3
    {
        public List<string> facilities { get; set; } = null;

        public locationPoint locationPoint { get; set; } = null;

        public string name { get; set; } = "";

        public VerifiedPropertyV3 property { get; set; } = new VerifiedPropertyV3();

        [JsonProperty("locationLevels", NullValueHandling = NullValueHandling.Ignore)]
        public LocationLevels locationLevels { get; set; } = null;

        [JsonProperty("tenureCode", NullValueHandling = NullValueHandling.Ignore)]
        public string tenureCode { get; set; } = null;
    }

    public class locationPoint
    {
        public double lat { get; set; } = 0;
        public double lon { get; set; } = 0;
    }

    public class LocationLevels
    {
        public string level200Id { get; set; } = "C";
        public string level500Id { get; set;}
    }
}
