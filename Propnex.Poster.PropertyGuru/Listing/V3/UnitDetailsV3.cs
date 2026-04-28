using System.Collections.Generic;
using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>房源单元详情（V3 API）</summary>
    public class UnitDetailsV3
    {
        /// <summary>房间配置（卧室/浴室数量）</summary>
        [JsonProperty("configuration")]
        public ConfigurationV3 Configuration { get; set; }

        /// <summary>面积尺寸（建筑面积/土地面积）</summary>
        [JsonProperty("dimensions", NullValueHandling = NullValueHandling.Ignore)]
        public DimensionsV3 Dimensions { get; set; }

        /// <summary>是否符合租客资格限制（如种族配额）</summary>
        [JsonProperty("tenantEligibility")]
        public bool TenantEligibility { get; set; }

        /// <summary>是否即刻可入住（出租日期 <= 今天则为 true）</summary>
        [JsonProperty("isAvailableNow")]
        public bool IsAvailableNow { get; set; }

        /// <summary>楼层代码，如 GND（地面层）、HIGH（高层）等</summary>
        [JsonProperty("floorLevel", NullValueHandling = NullValueHandling.Ignore)]
        public string FloorLevel { get; set; }

        /// <summary>家具配置代码，如 FULL（全家具）、PART（部分）、UNFURN（无家具）</summary>
        [JsonProperty("furnishing", NullValueHandling = NullValueHandling.Ignore)]
        public string Furnishing { get; set; }

        /// <summary>家具详情列表（具体家具项目代码）</summary>
        [JsonProperty("furnishingDetails", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FurnishingDetails { get; set; }

        /// <summary>设施/配套代码列表，如 AIRC（空调）、INET（网络）等</summary>
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Features { get; set; }

        /// <summary>是否为马来人地段（马来西亚专用）</summary>
        [JsonProperty("isBumiLot")]
        public bool? IsBumiLot { get; set; }

        /// <summary>最多租客人数（合租房间用）</summary>
        [JsonProperty("maxTenants", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxTenants { get; set; }

        /// <summary>租客性别要求，如 ANY / MALE / FEMALE</summary>
        [JsonProperty("tenantGender", NullValueHandling = NullValueHandling.Ignore)]
        public string TenantGender { get; set; }

        /// <summary>房东是否同住</summary>
        [JsonProperty("ownerStays")]
        public bool OwnerStays { get; set; }

        /// <summary>是否允许养宠物</summary>
        [JsonProperty("petFriendly")]
        public bool PetFriendly { get; set; }

        /// <summary>是否包含 WiFi</summary>
        [JsonProperty("wifiIncluded")]
        public bool WifiIncluded { get; set; }

        /// <summary>是否包含水电费</summary>
        [JsonProperty("utilitiesIncluded")]
        public bool UtilitiesIncluded { get; set; }

        /// <summary>是否允许访客</summary>
        [JsonProperty("visitorsAllowed")]
        public bool VisitorsAllowed { get; set; }

        /// <summary>出租类型，如 ENT（整套）、ROOM（房间）</summary>
        [JsonProperty("rentalType", NullValueHandling = NullValueHandling.Ignore)]
        public string RentalType { get; set; }

        /// <summary>房间类型，如 COMMON（普通房）、MASTER（主卧）</summary>
        [JsonProperty("roomType", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomType { get; set; }

        /// <summary>组屋类型代码，如 3RM / 4RM / 5RM / EA 等</summary>
        [JsonProperty("hdbTypeCode", NullValueHandling = NullValueHandling.Ignore)]
        public string HdbTypeCode { get; set; }

        /// <summary>租赁状态（TENANTED 有租客 / UNTENANTED 无租客）及到期日</summary>
        [JsonProperty("tenancy", NullValueHandling = NullValueHandling.Ignore)]
        public TenancyV3 Tenancy { get; set; }

        /// <summary>装修/交付条件代码，如 BARE（毛坯）、RENO（已装修）；工商业房源常用</summary>
        [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
        public string Condition { get; set; }

        /// <summary>物业用途代码列表，如 ASMB（装配）、WRHSE（仓储）等；工业房源专用</summary>
        [JsonProperty("propertyUses", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyUses { get; set; }

        /// <summary>电力信息（相数 + 供电量）；工业房源专用</summary>
        [JsonProperty("electricity", NullValueHandling = NullValueHandling.Ignore)]
        public ElectricityV3 Electricity { get; set; }

        /// <summary>地板承重等级代码，如 LIGHT（轻载）、HEAVY（重载）；工业房源专用</summary>
        [JsonProperty("floorLoadingCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string FloorLoadingCategory { get; set; }

        /// <summary>是否高天花板</summary>
        [JsonProperty("isHighCeiling", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsHighCeiling { get; set; }

        /// <summary>升降机信息（货梯/客梯数量）；工业房源专用</summary>
        [JsonProperty("lift", NullValueHandling = NullValueHandling.Ignore)]
        public LiftV3 Lift { get; set; }

        /// <summary>是否有车辆坡道（工业/仓储房源）</summary>
        [JsonProperty("ramp", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ramp { get; set; }

        /// <summary>卖家/房东种族（用于种族配额申报），如 C / M / I / O</summary>
        [JsonProperty("sellerEthnic", NullValueHandling = NullValueHandling.Ignore)]
        public string SellerEthnic { get; set; }

        /// <summary>卖家/房东居民身份，如 SC（公民）、PR（永久居民）</summary>
        [JsonProperty("sellerResidency", NullValueHandling = NullValueHandling.Ignore)]
        public string SellerResidency { get; set; }

        /// <summary>烹饪类型，如 BOTH / GAS / ELEC；合租房间常用</summary>
        [JsonProperty("cookingType", NullValueHandling = NullValueHandling.Ignore)]
        public string CookingType { get; set; }

        /// <summary>是否有中央空调</summary>
        [JsonProperty("centralAircon", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CentralAircon { get; set; }
    }
}
