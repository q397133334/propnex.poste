using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty.V1
{
    public class PlaceDto : ListingLocation
    {

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ListingItemType>? buildingFacilities { get; set; } = null;

        public double? latitude { get; set; }

        public double? longitude { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingItemType propertyGroupType { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingItemType propertyType { get; set; }

        public string postalCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? postCode { get; set; }

        public string block { get; set; }

        public string unit { get; set; }

        public string floor { get; set; }

        public bool hideUnitFloor { get; set; }
    }

    public class BuildingRequestData
    {
        public ResponseData<List<PlaceDto>> places { get; set; }
    }
}
