using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class ListingSalePrice
    {
        public int? Fixed { get; set; }

        public string CurrencyCode { get; set; } = "MYR";

        [JsonIgnore]
        public string __typename { get; set; } = "Price";
    }
}
