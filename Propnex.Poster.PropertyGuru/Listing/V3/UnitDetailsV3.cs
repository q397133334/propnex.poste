using System.Collections.Generic;
using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class UnitDetailsV3
    {
        [JsonProperty("configuration")]
        public ConfigurationV3 Configuration { get; set; }

        [JsonProperty("dimensions", NullValueHandling = NullValueHandling.Ignore)]
        public DimensionsV3 Dimensions { get; set; }

        [JsonProperty("tenantEligibility")]
        public bool TenantEligibility { get; set; }

        [JsonProperty("isAvailableNow")]
        public bool IsAvailableNow { get; set; }

        [JsonProperty("floorLevel", NullValueHandling = NullValueHandling.Ignore)]
        public string FloorLevel { get; set; }

        [JsonProperty("furnishing", NullValueHandling = NullValueHandling.Ignore)]
        public string Furnishing { get; set; }

        [JsonProperty("furnishingDetails", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FurnishingDetails { get; set; }

        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Features { get; set; }

        [JsonProperty("isBumiLot")]
        public bool? IsBumiLot { get; set; }


        [JsonProperty("maxTenants", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxTenants { get; set; }

        [JsonProperty("tenantGender", NullValueHandling = NullValueHandling.Ignore)]
        public string TenantGender { get; set; }

        [JsonProperty("ownerStays")]
        public bool OwnerStays { get; set; }

        [JsonProperty("petFriendly")]
        public bool PetFriendly { get; set; }

        [JsonProperty("wifiIncluded")]
        public bool WifiIncluded { get; set; }

        [JsonProperty("utilitiesIncluded")]
        public bool UtilitiesIncluded { get; set; }

        [JsonProperty("visitorsAllowed")]
        public bool VisitorsAllowed { get; set; }


        [JsonProperty("rentalType", NullValueHandling = NullValueHandling.Ignore)]
        public string RentalType { get; set; }

        [JsonProperty("roomType", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomType { get; set; }

        [JsonProperty("hdbTypeCode", NullValueHandling = NullValueHandling.Ignore)]
        public string HdbTypeCode { get; set; }
    }
}