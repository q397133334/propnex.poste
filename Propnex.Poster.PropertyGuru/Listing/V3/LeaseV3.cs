using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>租约期限（V3 API）</summary>
    public class LeaseV3
    {
        /// <summary>租约期代码，如 1YR（一年）/ 2YR（两年）/ MTH（按月）</summary>
        [JsonProperty("code")]
        public string Code { get; set; } = null;

        [JsonProperty("remaining")]
        public string Remaining { get; set; }=null;
    }
}
