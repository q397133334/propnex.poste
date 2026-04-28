using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>租赁状态（V3 API）</summary>
    public class TenancyV3
    {
        /// <summary>租赁状态：TENANTED（有租客）/ UNTENANTED（无租客）</summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>现有租约到期日期，无租客时为 null</summary>
        [JsonProperty("tenantedUntilDate", NullValueHandling = NullValueHandling.Ignore)]
        public object TenantedUntilDate { get; set; }
    }
}
