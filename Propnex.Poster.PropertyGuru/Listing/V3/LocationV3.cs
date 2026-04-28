using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>地址/位置信息（V3 API）</summary>
    public class LocationV3
    {
        /// <summary>详细地址（邮编、楼层、单元号等）</summary>
        [JsonProperty("address")]
        public AddressV3 Address { get; set; }
    }
}
