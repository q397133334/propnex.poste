using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class PropertyDetailsExtension
    {
        /// <summary>
        /// 
        /// </summary>
        public string? availableDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string isCoAgency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AreaDimension? saleableAreaDimension { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AreaDimension? grossAreaDimension { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string unexpiredLeaseYear { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? titleTypeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? landTitleTypeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? occupiedCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? isBumiLot { get; set; }
    }
}
