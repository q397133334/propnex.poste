using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{



    public class PropertyDetailsDto
    {
        public int? bathroom { get; set; }

        public int? bedroomCode { get; set; }

        public List<int>? buildingFacilityCodes { get; set; }

        public int? carPark { get; set; }

        public int? conditionCodes { get; set; }

        public int? directionCode { get; set; }



        public PropertyDetailsExtension extension { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> floorPlans { get; set; }


        public int? grossArea { get; set; }

        public string id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> images { get; set; }

        public NullClass location { get; set; }

        public ListingSalePrice maintenanceFee { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> photo360s { get; set; }

        public ListingSalePrice rentPrice { get; set; }

        public ListingSalePrice salePrice { get; set; }

        public int? saleableArea { get; set; }

        public int? saleableAreaMeasurementCode { get; set; }

        public int? storeRoom { get; set; }

        public int? tenureCode { get; set; }

        public List<int>? unitFeatureCodes { get; set; }

        public int? unitTypeCode { get; set; }


    }

    public class PropertyDetailsExtension
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AreaDimension? grossAreaDimension { get; set; } = new AreaDimension();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? isBumiLot { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? landTitleTypeCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? occupiedCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AreaDimension? saleableAreaDimension { get; set; } = new AreaDimension();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? titleTypeCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? unexpiredLeaseYear { get; set; }
    }

    public class AreaDimension
    {
        public int? length { get; set; }

        public int? width { get; set; }
    }
}
