using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>物业类型信息（V3 API）</summary>
    public class VerifiedPropertyV3
    {
        /// <summary>
        /// 物业子类型代码，如：
        /// HDB（组屋）、CONDO（公寓）、LANDED（有地住宅）、
        /// FAC（工厂/厂房）、WRHSE（仓库）、SHOP（店铺）等
        /// </summary>
        [JsonProperty("subType")]
        public string SubType { get; set; }
    }
}
