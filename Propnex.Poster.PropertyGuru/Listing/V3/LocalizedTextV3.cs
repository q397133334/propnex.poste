using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class LocalizedTextV3
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; } = "en";

        [JsonProperty("brand")]
        public string Brand { get; set; } = "pg";
    }
}