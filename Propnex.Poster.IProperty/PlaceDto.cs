using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class PlaceDto : ListingLocation
    {

        public List<ListingItemType> buildingFacilities { get; set; }=new List<ListingItemType>();

        public double latitude { get; set; }

        public double longitude { get; set; }  

        public ListingItemType propertyGroupTpye { get; set; }

        public ListingItemType propertyType { get; set; }

        public string postCode { get; set; }

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
