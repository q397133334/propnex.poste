using Newtonsoft.Json;
using Propnex.Poster.PropertyGuru.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>
    /// V3 API 创建/更新房源的顶层请求体
    /// POST /v1/listings  或  PUT /v1/listings/{id}
    /// </summary>
    public class CreateListingV3
    {
        /// <summary>房源 ID，仅用于 UpdateV3Async URL 拼接，不序列化到 JSON 请求体</summary>
        [JsonIgnore]
        public int? Id { get; set; }

        public string alternativeEmail { get; set; } = null;
        public string alternativeMobile { get; set; } = null;
        public string alternativePhone { get; set; } = null;

        public string cobroke { get; set;}  =null;

        /// <summary>日期信息（出租时填 available 可入住日期，出售可为 null）</summary>
        [JsonProperty("dates", NullValueHandling = NullValueHandling.Ignore)]
        public DatesV3 Dates { get; set; } = new DatesV3();

        /// <summary>房源描述列表（多语言，目前仅 en）</summary>
        [JsonProperty("descriptions")]
        public List<LocalizedTextV3> Descriptions { get; set; } = new List<LocalizedTextV3>();

        /// <summary>房源标题列表（多语言，目前仅 en）</summary>
        [JsonProperty("headlines")]
        public List<LocalizedTextV3> Headlines { get; set; } = new List<LocalizedTextV3>();

        /// <summary>租约期限（如 1YR / 2YR / MTH），出售类型可为 null</summary>
        [JsonProperty("lease", NullValueHandling = NullValueHandling.Ignore)]
        public LeaseV3 Lease { get; set; }=new LeaseV3();

        /// <summary>挂牌类型（出售 SALE / 出租 RENT）</summary>
        [JsonProperty("listingType")]
        public ListingTypeV3 ListingType { get; set; } = new ListingTypeV3();

        /// <summary>地址/位置信息（邮编、楼层、单元号）</summary>
        [JsonProperty("location")]
        public LocationV3 Location { get; set; } = new LocationV3();

        public string parkingFee { get; set;}   =null;

        /// <summary>价格信息（售价或月租 + 管理费）</summary>
        [JsonProperty("price")]
        public PriceV3 Price { get; set; } = new PriceV3();


        /// <summary>所属楼盘信息（verified 类型 + 楼盘 ID + 物业类型）</summary>
        [JsonProperty("project")]
        public ProjectV3 Project { get; set; } = new ProjectV3();


        public string referenceNumber { get; set;}   =null;


        /// <summary>单元详情（面积、楼层、家具、设施等）</summary>
        [JsonProperty("unitDetails")]
        public UnitDetailsV3 UnitDetails { get; set; } = new UnitDetailsV3();

        // ─────────────────────────────────────────────────────────────────────
        // 工厂方法：从解析好的 GuruTaskListing（V2）转换成 V3 请求体
        // 注意：此方法不含新增字段（租赁状态、电力、升降机等），
        //       新增字段由 GuruTaskListings.Init() 直接构建 ListingV3 时填入。
        // ─────────────────────────────────────────────────────────────────────
        public static CreateListingV3 From(GuruTaskListing taskListing)
        {
            var m = taskListing.Listing;

            var headline = !string.IsNullOrEmpty(m.LocalizedHeadline)
                ? m.LocalizedHeadline
                : DefaultTitles.GetTitle();

            var description = m.LocalizedDescription ?? "";

            // V2 合并为 "12-05"，V3 拆开为 floor + unit
            var floor = "";
            var unit  = "";
            if (!string.IsNullOrEmpty(m.Location?.unit))
            {
                var parts = m.Location.unit.Split('-');
                floor = parts.Length > 0 ? parts[0] : "";
                unit  = parts.Length > 1 ? parts[1] : "";
            }

            // V2 是 [{code:"AIRC"}]，V3 是 ["AIRC"]
            var features = m.PropertyUnit?.features?
                .Where(f => !string.IsNullOrEmpty(f.code))
                .Select(f => f.code)
                .ToList();

            bool isAvailableNow = true;
            if (m.TypeCode?.ToUpper() == "RENT" && m.Dates?.available?.date != null)
            {
                isAvailableNow = Convert.ToDateTime(m.Dates.available.date) <= DateTime.Now;
            }

            int? floorAreaValue = null;
            if (m.Sizes?.floorArea != null && m.Sizes.floorArea.Count > 0)
            {
                var raw = m.Sizes.floorArea[0].value;
                if (raw.HasValue)
                    floorAreaValue = (int)raw.Value;
            }

            return new CreateListingV3
            {
                Id          = m.Id,
                ListingType = new ListingTypeV3 { Code = m.TypeCode ?? "SALE" },
                Price = new PriceV3
                {
                    Value = m.Price?.value ?? 0
                    // MaintenanceFee 由调用方按需赋值
                },
                Location = new LocationV3
                {
                    Address = new AddressV3
                    {
                        PostalCode     = m.Location?.postalCode ?? "",
                        Floor          = string.IsNullOrEmpty(floor) ? null : floor,
                        Unit           = string.IsNullOrEmpty(unit)  ? null : unit,
                        MaskUnitNumber = m.Location?.maskLocation ?? false
                    }
                },
                Headlines = new List<LocalizedTextV3>
                {
                    new LocalizedTextV3 { Text = headline, Locale = "en", Brand = "pg" }
                },
                Descriptions = new List<LocalizedTextV3>
                {
                    new LocalizedTextV3 { Text = description, Locale = "en", Brand = "pg" }
                },
                UnitDetails = new UnitDetailsV3
                {
                    Configuration = new ConfigurationV3
                    {
                        Bedrooms  = m.Sizes?.bedrooms?.value,
                        Bathrooms = m.Sizes?.bathrooms?.value
                    },
                    Dimensions = floorAreaValue.HasValue
                        ? new DimensionsV3
                        {
                            Floor = new FloorDimensionV3
                            {
                                Size = new SizeV3 { Value = floorAreaValue, Uom = "sqft" }
                            }
                        }
                        : null,
                    TenantEligibility = false,
                    IsAvailableNow    = isAvailableNow,
                    FloorLevel  = string.IsNullOrEmpty(m.PropertyUnit?.floorLevelCode) ? null : m.PropertyUnit.floorLevelCode,
                    Furnishing  = string.IsNullOrEmpty(m.PropertyUnit?.furnishingCode)  ? null : m.PropertyUnit.furnishingCode,
                    Features    = features != null && features.Count > 0 ? features : null,
                    IsBumiLot   = null
                },
                Project = new ProjectV3
                {
                    Type = "verified",
                    MetaByType = new MetaByTypeV3
                    {
                        Verified = new VerifiedMetaV3
                        {
                            LocationId = m.Location?.id,
                            Property   = new VerifiedPropertyV3 { SubType = m.Property?.typeCode ?? "" }
                        }
                    }
                },
            };
        }
    }
}
