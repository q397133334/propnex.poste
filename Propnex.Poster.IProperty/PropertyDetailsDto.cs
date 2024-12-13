using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{

    public class SaleableAreaDimension
    {
        /// <summary>
        /// 
        /// </summary>
        public string width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string length { get; set; }
    }

    public class GrossAreaDimension
    {
        /// <summary>
        /// 
        /// </summary>
        public string width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string length { get; set; }
    }


    public class Location
    {
    }

    public class SalePrice
    {
        /// <summary>
        /// 
        /// </summary>
        public string currencyCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int @fixed { get; set; }
    }

    public class RentPrice
    {
        /// <summary>
        /// 
        /// </summary>
        public string currencyCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string @fixed { get; set; }
    }

    public class MaintenanceFee
    {
        /// <summary>
        /// 
        /// </summary>
        public string currencyCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string @fixed { get; set; }
    }

    public class Caption
    {
        /// <summary>
        /// 
        /// </summary>
        public string en_GB { get; set; }
    }

    public class ImagesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fullPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Caption caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int sourceCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int height { get; set; }
    }

    public class PropertyDetailsDto
    {

        /// <summary>
        /// 
        /// </summary>
        public DateTime? auctionDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? bathroom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? bedroomCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int?>? buildingFacilityCodes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? carPark { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? channelCode { get; set; }

     
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? conditionCodes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? directionCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PropertyDetailsExtension extension { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? grossArea { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? isAuction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string listingRefNo { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public NullClass location { get; set; } = new NullClass();

        /// <summary>
        /// 
        /// </summary>
        public ListingSalePrice maintenanceFee { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ListingSalePrice salePrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListingSalePrice rentPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? saleableArea { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? saleableAreaMeasurementCode { get; set; }
            
     
     
        /// <summary>
        /// 
        /// </summary>
        public string? storeRoom { get; set; }
       

        /// <summary>
        /// 
        /// </summary>
        public int? tenureCode { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public List<int> unitFeatureCodes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? unitTypeCode { get; set; }


        /// <summary>
        /// 
        /// </summary>

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? images { get; set; }
        /// <summary>
        /// 
        /// </summary>

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? floorPlans { get; set; }
        /// <summary>
        /// 
        /// </summary>

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? photo360s { get; set; }

    }
}
