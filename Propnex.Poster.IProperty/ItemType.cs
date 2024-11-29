using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class ItemType
    {
        public int Code { get; set; }

        public string _description { get; set; } = "";

        [JsonIgnore]
        public string __typename { get; set; } = "Reference";
    }
}
