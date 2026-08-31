using System.Collections.Generic;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    // ─────────────────────────────────────────────────────────────────────
    // PATCH /v1/listings/{listingId} 的请求体（局部更新一个已存在的 listing）。
    // 这套结构是从 WrapperListingSg 实际发出的 Patch body 样例生成的，跟 CreateListingV3
    // （POST /v1/listings 创建时用，走 Agent 的 agentnet 接口）是两套完全不同的 schema，
    // 字段名/形状都对不上（比如这里的 unitDetails.dimensions.floor 是 {width,length,size:{value,uom}}，
    // configuration 是 {bedrooms,bathrooms,extrarooms} 这种扁平数字，跟 ListingModel/CreateListingV3
    // 里同名的类型都不一样），不要互相混用或复用。
    // ─────────────────────────────────────────────────────────────────────

    public class PatchDatesV3
    {
        public object available { get; set; }

        public object auction { get; set; }
    }

    public class PatchLeaseV3
    {
        public string code { get; set; }

        public object remaining { get; set; }
    }

    public class PatchPriceV3
    {
        public decimal? value { get; set; }

        public string type { get; set; }

        public object maintenanceFee { get; set; }
    }

    public class PatchHeadlineV3
    {
        public string locale { get; set; }

        public string text { get; set; }

        public string brand { get; set; }
    }

    public class PatchDescriptionV3
    {
        public string locale { get; set; }

        public string text { get; set; }

        public string brand { get; set; }
    }

    public class PatchSizeV3
    {
        public double? value { get; set; }

        public string uom { get; set; }
    }

    public class PatchFloorDimensionV3
    {
        public object width { get; set; }

        public object length { get; set; }

        public PatchSizeV3 size { get; set; }
    }

    public class PatchLandDimensionV3
    {
        public PatchSizeV3 size { get; set; }

        public object width { get; set; }

        public object length { get; set; }
    }

    public class PatchRoomDimensionV3
    {
        public PatchSizeV3 size { get; set; }
    }

    public class PatchDimensionsV3
    {
        public PatchFloorDimensionV3 floor { get; set; }

        public PatchLandDimensionV3 land { get; set; }

        public PatchRoomDimensionV3 room { get; set; }
    }

    public class PatchConfigurationV3
    {
        public int? bedrooms { get; set; }

        public int? bathrooms { get; set; }

        public int? extrarooms { get; set; }
    }

    public class PatchUnitDetailsV3
    {
        public PatchConfigurationV3 configuration { get; set; }

        public PatchDimensionsV3 dimensions { get; set; }

        public object electricity { get; set; }

        public object lift { get; set; }

        public object maxTenants { get; set; }

        public object tenantGender { get; set; }

        public object ownerStays { get; set; }

        public object cookingType { get; set; }

        public object petFriendly { get; set; }

        public object wifiIncluded { get; set; }

        public object utilitiesIncluded { get; set; }

        public object visitorsAllowed { get; set; }

        public bool? tenantEligibility { get; set; }

        public object titleType { get; set; }

        public object landTitleType { get; set; }

        public bool? isAvailableNow { get; set; }

        public string floorLevel { get; set; }

        public string furnishing { get; set; }

        public List<string> furnishingDetails { get; set; }

        public List<string> features { get; set; }

        public object isBumiLot { get; set; }

        public object directionCode { get; set; }

        public object roomType { get; set; }

        public object condition { get; set; }

        public object parkingSpots { get; set; }

        public object hdbTypeCode { get; set; }

        public object sellerResidency { get; set; }

        public object sellerEthnic { get; set; }

        public object quotaEthnic { get; set; }

        public object quotaSpr { get; set; }

        public object ramp { get; set; }

        public object isHighCeiling { get; set; }

        public object ceilingHeight { get; set; }

        public object floorLoadingCategory { get; set; }

        public object floorLoadingCapacity { get; set; }

        public object centralAirconHours { get; set; }

        public object centralAircon { get; set; }

        public object propertyUse { get; set; }
    }

    /// <summary>PATCH /v1/listings/{listingId} 请求体的根对象。</summary>
    public class PatchListingV3
    {
        public PatchDatesV3 dates { get; set; }

        public PatchLeaseV3 lease { get; set; }

        public bool? cobroke { get; set; }

        public object referenceNumber { get; set; }

        public PatchPriceV3 price { get; set; }

        public List<PatchHeadlineV3> headlines { get; set; }

        public List<PatchDescriptionV3> descriptions { get; set; }

        public PatchUnitDetailsV3 unitDetails { get; set; }

        public object alternativePhone { get; set; }

        public object alternativeMobile { get; set; }

        public object alternativeEmail { get; set; }

        public object parkingFee { get; set; }
    }
}
