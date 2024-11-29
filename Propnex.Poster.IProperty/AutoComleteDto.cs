using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class PropertyDto
    {
        public Level? level1 { get; set; }
        public Level? level2 { get; set; }
        public Level? level3 { get; set; }
        public Level? level4 { get; set; }
        public Level? level5 { get; set; }

        public string PostCode { get; set; } = "";

        public V1.ListingMultiLangText Address { get; set; }

        public ItemType propertyType { get; set; }

        public ItemType propertyGroupType { get; set; }

        public bool hasTownship { get; set; }


        public double? latitude { get; set; }

        public double? longitude { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ItemType>? buildingFacilities { get; set; } = null;

        public string __typename { get; set; } = "Place";
    }

    public class AutoCompleteDto
    {
        public List<PropertyDto> Data { get; set; }

        public string __typename { get; set; } = "AutocompleteResult";
    }



    public class AutocompleteResult
    {
        public AutoCompleteDto Data { get; set; }
    }

}