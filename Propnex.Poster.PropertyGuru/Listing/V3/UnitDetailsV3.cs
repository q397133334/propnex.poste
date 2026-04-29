using System.Collections.Generic;
using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    /// <summary>房源单元详情（V3 API）</summary>
    public class UnitDetailsV3
    {
        /// <summary>天花板高度，如 "3.5m"（ceilingHeight）</summary>
        [JsonProperty("ceilingHeight", NullValueHandling = NullValueHandling.Ignore)]
        public string CeilingHeight { get; set; } = null;

        /// <summary>是否有中央空调</summary>
        [JsonProperty("centralAircon", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CentralAircon { get; set; } = null;

        /// <summary>中央空调开放时长/说明（centralAirconHours）</summary>
        [JsonProperty("centralAirconHours", NullValueHandling = NullValueHandling.Ignore)]
        public string CentralAirconHours { get; set; } = null;

        /// <summary>装修/交付条件代码，如 BARE（毛坯）、RENO（已装修）；工商业房源常用</summary>
        [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
        public string Condition { get; set; } = null;

        /// <summary>房间配置（卧室/浴室数量）</summary>
        [JsonProperty("configuration")]
        public ConfigurationV3 Configuration { get; set; }


        /// <summary>烹饪类型，如 BOTH / GAS / ELEC；合租房间常用</summary>
        [JsonProperty("cookingType")]
        public string CookingType { get; set; } = null;


        /// <summary>面积尺寸（建筑面积/土地面积）</summary>
        [JsonProperty("dimensions")]
        public DimensionsV3 Dimensions { get; set; } = new DimensionsV3();
        [JsonProperty("directionCode")]
        public string directionCode { get; set; } = null;


        /// <summary>电力信息（相数 + 供电量）；工业房源专用</summary>
        [JsonProperty("electricity", NullValueHandling = NullValueHandling.Ignore)]
        public ElectricityV3 Electricity { get; set; } = null;

        /// <summary>设施/配套代码列表，如 AIRC（空调）、INET（网络）等</summary>
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Features { get; set; } = new List<string>();

        /// <summary>楼层代码，如 GND（地面层）、HIGH（高层）等</summary>
        [JsonProperty("floorLevel", NullValueHandling = NullValueHandling.Ignore)]
        public string FloorLevel { get; set; } = null;

        /// <summary>地板承重量（floorLoadingCapacity），工业房源专用</summary>
        [JsonProperty("floorLoadingCapacity", NullValueHandling = NullValueHandling.Ignore)]
        public string FloorLoadingCapacity { get; set; } = null;

        /// <summary>地板承重等级代码，如 LIGHT（轻载）、HEAVY（重载）；工业房源专用</summary>
        [JsonProperty("floorLoadingCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string FloorLoadingCategory { get; set; } = null;

        /// <summary>家具配置代码，如 FULL（全家具）、PART（部分）、UNFURN（无家具）</summary>
        [JsonProperty("furnishing", NullValueHandling = NullValueHandling.Ignore)]
        public string Furnishing { get; set; } = null;
        /// <summary>家具详情列表（具体家具项目代码）</summary>
        [JsonProperty("furnishingDetails", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FurnishingDetails { get; set; }

        /// <summary>组屋类型代码，如 3RM / 4RM / 5RM / EA 等</summary>
        [JsonProperty("hdbType", NullValueHandling = NullValueHandling.Ignore)]
        public string HdbTypeCode { get; set; } = null;

        /// <summary>是否即刻可入住（出租日期 <= 今天则为 true）</summary>
        [JsonProperty("isAvailableNow")]
        public bool IsAvailableNow { get; set; } = false;

        /// <summary>是否为马来人地段（马来西亚专用）</summary>
        [JsonProperty("isBumiLot", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsBumiLot { get; set; } = null;


        /// <summary>是否高天花板</summary>
        [JsonProperty("isHighCeiling", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsHighCeiling { get; set; } = null;

        /// <summary>土地产权类型代码（landTitleType），有地住宅专用</summary>
        [JsonProperty("landTitleType", NullValueHandling = NullValueHandling.Ignore)]
        public string LandTitleType { get; set; } = null;


        /// <summary>升降机信息（货梯/客梯数量）；工业房源专用</summary>
        [JsonProperty("lift")]
        public LiftV3 Lift { get; set; } = null;



        /// <summary>最多租客人数（合租房间用）</summary>
        [JsonProperty("maxTenants", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxTenants { get; set; } = null;



        /// <summary>房东是否同住</summary>
        [JsonProperty("ownerStays", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OwnerStays { get; set; } = null;

        /// <summary>停车位数量（parkingSpots）</summary>
        [JsonProperty("parkingSpots", NullValueHandling = NullValueHandling.Ignore)]
        public int? ParkingSpots { get; set; } = null;

        /// <summary>是否允许养宠物</summary>
        [JsonProperty("petFriendly", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PetFriendly { get; set; } = null;

        /// <summary>物业用途代码列表，如 ASMB（装配）、WRHSE（仓储）等；工业房源专用</summary>
        [JsonProperty("propertyUses", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyUses { get; set; } = null;

        /// <summary>种族配额剩余（quotaEthnic），如 C / M / I / O</summary>
        [JsonProperty("quotaEthnic", NullValueHandling = NullValueHandling.Ignore)]
        public string QuotaEthnic { get; set; } = null;

        /// <summary>永久居民配额剩余（quotaSpr）</summary>
        [JsonProperty("quotaSpr", NullValueHandling = NullValueHandling.Ignore)]
        public string QuotaSpr { get; set; } = null;


        /// <summary>是否有车辆坡道（工业/仓储房源）</summary>
        [JsonProperty("ramp", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ramp { get; set; }

        /// <summary>出租类型，如 ENT（整套）、ROOM（房间）</summary>
        [JsonProperty("rentalType", NullValueHandling = NullValueHandling.Ignore)]
        public string RentalType { get; set; } = null;

        /// <summary>房间类型，如 COMMON（普通房）、MASTER（主卧）</summary>
        [JsonProperty("roomType", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomType { get; set; } = null;

        /// <summary>卖家/房东种族（用于种族配额申报），如 C / M / I / O</summary>
        [JsonProperty("sellerEthnic", NullValueHandling = NullValueHandling.Ignore)]
        public string SellerEthnic { get; set; }

        /// <summary>卖家/房东居民身份，如 SC（公民）、PR（永久居民）</summary>
        [JsonProperty("sellerResidency", NullValueHandling = NullValueHandling.Ignore)]
        public string SellerResidency { get; set; }

        /// <summary>是否符合租客资格限制（如种族配额）</summary>
        [JsonProperty("tenantEligibility")]
        public bool? TenantEligibility { get; set; } = null;

        /// <summary>租客性别要求，如 ANY / MALE / FEMALE</summary>
        [JsonProperty("tenantGender", NullValueHandling = NullValueHandling.Ignore)]
        public string TenantGender { get; set; }

        /// <summary>产权类型代码（titleType）</summary>
        [JsonProperty("titleType", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleType { get; set; } = null;

        [JsonProperty("utilitiesIncluded", NullValueHandling = NullValueHandling.Ignore)]
        public string utilitiesIncluded { get; set; } = null;

        [JsonProperty("visitorsAllowed", NullValueHandling = NullValueHandling.Ignore)]
        public string visitorsAllowed { get; set; } = null;

        [JsonProperty("wifiIncluded", NullValueHandling = NullValueHandling.Ignore)]
        public string wifiIncluded { get; set; } = null;


        /// <summary>朝向，如 NORTH / SOUTH 等（direction）</summary>
        [JsonProperty("direction", NullValueHandling = NullValueHandling.Ignore)]
        public string Direction { get; set; } = null;

        /// <summary>占用状态（occupancy）</summary>
        [JsonProperty("occupancy", NullValueHandling = NullValueHandling.Ignore)]
        public string Occupancy { get; set; } = null;

        /// <summary>业主类型（ownerType），如 INDIVIDUAL / COMPANY</summary>
        [JsonProperty("ownerType", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerType { get; set; } = null;

        /// <summary>停车费（parkingFee）</summary>
        [JsonProperty("parkingFee", NullValueHandling = NullValueHandling.Ignore)]
        public string ParkingFee { get; set; } = null;


        /// <summary>租赁状态（TENANTED 有租客 / UNTENANTED 无租客）及到期日</summary>
        [JsonProperty("tenancy", NullValueHandling = NullValueHandling.Ignore)]
        public TenancyV3 Tenancy { get; set; }








    }
}
