using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>房间配置（卧室/浴室数量）</summary>
    public class ConfigurationV3
    {
        /// <summary>卧室数量，工商业房源为 null</summary>
        [JsonProperty("bedrooms")]
        public int? Bedrooms { get; set; }

        /// <summary>浴室数量，工商业房源为 null</summary>
        [JsonProperty("bathrooms")]
        public int? Bathrooms { get; set; }

        public int? extrarooms { get; set; }
    }
}
