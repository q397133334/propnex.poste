using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>多语言文本（标题或描述）</summary>
    public class LocalizedTextV3
    {
        /// <summary>文本内容</summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>语言代码，目前固定为 en（英文）</summary>
        [JsonProperty("locale")]
        public string Locale { get; set; } = "en";

        /// <summary>平台品牌，目前固定为 pg（PropertyGuru）</summary>
        [JsonProperty("brand")]
        public string Brand { get; set; } = "pg";
    }
}
