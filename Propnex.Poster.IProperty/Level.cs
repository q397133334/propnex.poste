using Newtonsoft.Json;
using Propnex.Poster.IProperty.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class Level
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? NanoId { get; set; } = "";


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public V1.ListingMultiLangTextV1? Text { get; set; }

        [JsonIgnore]
        public string __typename { get; set; } = "";
    }
}
