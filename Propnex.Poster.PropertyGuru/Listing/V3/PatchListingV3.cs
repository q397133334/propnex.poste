using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// 从已经解析好的 CreateListingV3（创建时用的那套数据）填充出一份 PatchListingV3。
        /// 两者字段形状不完全一样（比如 propertyUse 单数/复数、cobroke 类型），逐个按语义对应转换，
        /// 不是简单地整体复用同一个对象。
        /// </summary>
        public static PatchListingV3 From(CreateListingV3 create)
        {
            if (create == null)
                return null;

            var unitDetails = create.UnitDetails;

            return new PatchListingV3
            {
                cobroke = ParseBool(create.cobroke),
                referenceNumber = create.referenceNumber,
                dates = new PatchDatesV3
                {
                    available = create.Dates?.Available,
                    auction = create.Dates?.Auction
                },
                lease = create.Lease == null ? null : new PatchLeaseV3
                {
                    code = create.Lease.Code,
                    remaining = create.Lease.Remaining
                },
                price = create.Price == null ? null : new PatchPriceV3
                {
                    value = create.Price.Value,
                    type = null,
                    maintenanceFee = create.Price.MaintenanceFee
                },
                headlines = create.Headlines?.Select(h => new PatchHeadlineV3
                {
                    locale = h.Locale,
                    text = h.Text,
                    brand = h.Brand
                }).ToList(),
                descriptions = create.Descriptions?.Select(d => new PatchDescriptionV3
                {
                    locale = d.Locale,
                    text = d.Text,
                    brand = d.Brand
                }).ToList(),
                unitDetails = unitDetails == null ? null : new PatchUnitDetailsV3
                {
                    configuration = unitDetails.Configuration == null ? null : new PatchConfigurationV3
                    {
                        bedrooms = unitDetails.Configuration.Bedrooms,
                        bathrooms = unitDetails.Configuration.Bathrooms,
                        extrarooms = unitDetails.Configuration.extrarooms
                    },
                    dimensions = unitDetails.Dimensions == null ? null : new PatchDimensionsV3
                    {
                        floor = new PatchFloorDimensionV3 { size = ToPatchSize(unitDetails.Dimensions.Floor?.Size) },
                        land = new PatchLandDimensionV3 { size = ToPatchSize(unitDetails.Dimensions.land?.Size) },
                        room = new PatchRoomDimensionV3 { size = ToPatchSize(unitDetails.Dimensions.room?.Size) }
                    },
                    electricity = unitDetails.Electricity,
                    lift = unitDetails.Lift,
                    maxTenants = unitDetails.MaxTenants,
                    tenantGender = unitDetails.TenantGender,
                    ownerStays = unitDetails.OwnerStays,
                    cookingType = unitDetails.CookingType,
                    petFriendly = unitDetails.PetFriendly,
                    wifiIncluded = unitDetails.wifiIncluded,
                    utilitiesIncluded = unitDetails.utilitiesIncluded,
                    visitorsAllowed = unitDetails.visitorsAllowed,
                    tenantEligibility = unitDetails.TenantEligibility,
                    titleType = unitDetails.TitleType,
                    landTitleType = unitDetails.LandTitleType,
                    isAvailableNow = unitDetails.IsAvailableNow,
                    floorLevel = unitDetails.FloorLevel,
                    furnishing = unitDetails.Furnishing,
                    furnishingDetails = unitDetails.FurnishingDetails,
                    features = unitDetails.Features,
                    isBumiLot = unitDetails.IsBumiLot,
                    directionCode = unitDetails.directionCode,
                    roomType = unitDetails.RoomType,
                    condition = unitDetails.Condition,
                    parkingSpots = unitDetails.ParkingSpots,
                    hdbTypeCode = unitDetails.HdbTypeCode,
                    sellerResidency = unitDetails.SellerResidency,
                    sellerEthnic = unitDetails.SellerEthnic,
                    quotaEthnic = unitDetails.QuotaEthnic,
                    quotaSpr = unitDetails.QuotaSpr,
                    ramp = unitDetails.Ramp,
                    isHighCeiling = unitDetails.IsHighCeiling,
                    ceilingHeight = unitDetails.CeilingHeight,
                    floorLoadingCategory = unitDetails.FloorLoadingCategory,
                    floorLoadingCapacity = unitDetails.FloorLoadingCapacity,
                    centralAirconHours = unitDetails.CentralAirconHours,
                    centralAircon = unitDetails.CentralAircon,
                    // Create 侧是 List<string> PropertyUses（工厂方法固定包一层单元素列表），
                    // Patch 响应样例里是单个 propertyUse，取第一个即可
                    propertyUse = unitDetails.PropertyUses?.FirstOrDefault()
                },
                alternativePhone = create.alternativePhone,
                alternativeMobile = create.alternativeMobile,
                alternativeEmail = create.alternativeEmail,
                parkingFee = create.parkingFee
            };
        }

        private static PatchSizeV3 ToPatchSize(SizeV3 size)
        {
            if (size == null)
                return null;
            return new PatchSizeV3 { value = size.Value, uom = size.Uom };
        }

        private static bool? ParseBool(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            if (bool.TryParse(value, out var result))
                return result;
            if (value == "1") return true;
            if (value == "0") return false;
            return null;
        }
    }
}
