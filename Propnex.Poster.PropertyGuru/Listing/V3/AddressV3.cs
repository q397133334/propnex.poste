using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>详细地址（V3 API）</summary>
    public class AddressV3
    {
        /// <summary>新加坡邮政编码（6位数字）</summary>
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        /// <summary>楼层号，如 "12"，整栋/地面层可为 null</summary>
        [JsonProperty("floor", NullValueHandling = NullValueHandling.Ignore)]
        public string Floor { get; set; }

        /// <summary>单元号，如 "05"，整栋可为 null</summary>
        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        /// <summary>是否隐藏单元号（有地房 typeGroup="L" 时设为 true）</summary>
        [JsonProperty("maskUnitNumber")]
        public bool MaskUnitNumber { get; set; }
    }
}
