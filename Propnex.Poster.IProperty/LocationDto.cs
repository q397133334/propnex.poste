using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class LocationDto
    {
        public PlaceDto location { get; set; }

        public string id { get; set; }

        public NullClass extension { get; set; } = new NullClass();

        public int saleableAreaMeasurementCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int?>? buildingFacilityCodes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? propertyGroupTypeCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? propertyTypeCode { get; set; }
    }
}
