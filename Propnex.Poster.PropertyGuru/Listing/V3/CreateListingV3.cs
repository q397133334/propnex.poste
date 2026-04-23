using Newtonsoft.Json;
using Propnex.Poster.PropertyGuru.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    // ─────────────────────────────────────────────────────────────────────────
    // 顶层请求体
    // ─────────────────────────────────────────────────────────────────────────
    public class CreateListingV3
    {
        /// <summary>仅用于 UpdateV3Async URL，不序列化到 JSON</summary>
        [JsonIgnore]
        public int? Id { get; set; }

        [JsonProperty("listingType")]
        public ListingTypeV3 ListingType { get; set; }

        [JsonProperty("price")]
        public PriceV3 Price { get; set; }

        [JsonProperty("location")]
        public LocationV3 Location { get; set; }

        [JsonProperty("headlines")]
        public List<LocalizedTextV3> Headlines { get; set; }

        [JsonProperty("descriptions")]
        public List<LocalizedTextV3> Descriptions { get; set; }

        [JsonProperty("unitDetails")]
        public UnitDetailsV3 UnitDetails { get; set; }

        [JsonProperty("project")]
        public ProjectV3 Project { get; set; }

        // ─────────────────────────────────────────────────────────────────────
        // 工厂方法：从解析好的 GuruTaskListing（V2）转换成 V3 请求体
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
                }
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 子模型
    // ─────────────────────────────────────────────────────────────────────────

    public class ListingTypeV3
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class PriceV3
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("maintenanceFee", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaintenanceFee { get; set; }
    }

    public class LocationV3
    {
        [JsonProperty("address")]
        public AddressV3 Address { get; set; }
    }

    public class AddressV3
    {
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("floor", NullValueHandling = NullValueHandling.Ignore)]
        public string Floor { get; set; }

        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [JsonProperty("maskUnitNumber")]
        public bool MaskUnitNumber { get; set; }
    }

    public class LocalizedTextV3
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; } = "en";

        [JsonProperty("brand")]
        public string Brand { get; set; } = "pg";
    }

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
    }

    public class ConfigurationV3
    {
        [JsonProperty("bedrooms", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bedrooms { get; set; }

        [JsonProperty("bathrooms", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bathrooms { get; set; }
    }

    public class DimensionsV3
    {
        [JsonProperty("floor", NullValueHandling = NullValueHandling.Ignore)]
        public FloorDimensionV3 Floor { get; set; }
    }

    public class FloorDimensionV3
    {
        [JsonProperty("size")]
        public SizeV3 Size { get; set; }
    }

    public class SizeV3
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public int? Value { get; set; }

        [JsonProperty("uom")]
        public string Uom { get; set; } = "sqft";
    }

    public class ProjectV3
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "verified";

        [JsonProperty("metaByType")]
        public MetaByTypeV3 MetaByType { get; set; }
    }

    public class MetaByTypeV3
    {
        [JsonProperty("verified")]
        public VerifiedMetaV3 Verified { get; set; }
    }

    public class VerifiedMetaV3
    {
        /// <summary>PropertyGuru 楼盘 verified string ID，如 "vseifm"</summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("locationId", NullValueHandling = NullValueHandling.Ignore)]
        public int? LocationId { get; set; }

        [JsonProperty("property", NullValueHandling = NullValueHandling.Ignore)]
        public VerifiedPropertyV3 Property { get; set; }
    }

    public class VerifiedPropertyV3
    {
        [JsonProperty("subType")]
        public string SubType { get; set; }
    }
}
