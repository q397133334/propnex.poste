using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Listing.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Tasks
{
    /// <summary>
    /// app 接口使用
    /// </summary>
    public class TaskListing
    {
        public string FastRepost { get; set; }

        public CreateOrUpdateListing Listing { get; set; }

        public List<string> Photos { get; set; }

        public List<string> Videos { get; set; }

        public List<string> Tours { get; set; }

        public List<string> FloorPlan { get; set; }
    }


    public class Retrieve
    {
        public string Account { get; set; }

        public string TargetPortal { get; set; }

        public string Id { get; set; }

        public string Password { get; set; }

        public List<CreateOrUpdateListing> Listings { get; set; }
    }

    /// <summary>ProjectData 子模型（&lt;ProjectData&gt; 节点下的 Field 字段）</summary>
    public class GuruProjectData
    {
        /// <summary>楼盘名称（Field Name="name"）</summary>
        public string ProjectName { get; set; }
        /// <summary>区域代码（Field Name="regionCode"）</summary>
        public string RegionCode { get; set; }
        /// <summary>楼盘总楼层（Field Name="floors"）</summary>
        public int ProjectFloors { get; set; }
    }

    /// <summary>Details 子模型（&lt;Details&gt; 节点下的 Field 字段全集）</summary>
    public class GuruDetailsData
    {
        // ── 物业基本信息 ─────────────────────────────────────────────
        /// <summary>物业名称（property_name）</summary>
        public string PropertyName { get; set; }
        /// <summary>物业 ID（property_id）</summary>
        public int? PropertyId { get; set; }
        /// <summary>位置 ID（location_id）</summary>
        public int? LocationId { get; set; }
        /// <summary>PG Verified 楼盘 ID，如 "456z7h"（pg_verified_id）</summary>
        public string PgVerifiedId { get; set; }
        /// <summary>物业类型组别：N 住宅 / L 有地 / C 商业（property_type_group）</summary>
        public string PropertyTypeGroup { get; set; }
        /// <summary>物业类型代码，如 CONDO / HDB / FAC（property_type_code）</summary>
        public string PropertyTypeCode { get; set; }
        /// <summary>组屋类型代码，如 3RM / 4RM / EA（hdb_type）</summary>
        public string HdbType { get; set; }
        /// <summary>组屋市镇代码（hdb_estate）</summary>
        public string HdbEstate { get; set; }
        /// <summary>地区代码，如 D15（district）</summary>
        public string District { get; set; }
        /// <summary>地契类型代码，如 F（永久）/ 99Y（99年）（tenure）</summary>
        public string Tenure { get; set; }

        // ── 挂牌信息 ─────────────────────────────────────────────────
        /// <summary>挂牌类型（listing_type）：SALE / RENT</summary>
        public string ListingType { get; set; }
        /// <summary>挂牌标题（listing_title）</summary>
        public string ListingTitle { get; set; }
        /// <summary>挂牌描述（listing_description，超 2000 字自动截断）</summary>
        public string ListingDescription { get; set; }
        /// <summary>租约期代码，如 1YR / 2YR / MTH（lease_term）</summary>
        public string LeaseTerm { get; set; }
        /// <summary>可入住日期（available_date）</summary>
        public string AvailableDate { get; set; }

        // ── 价格 ─────────────────────────────────────────────────────
        /// <summary>价格（price），出售为总价，出租为月租金</summary>
        public int Price { get; set; }
        /// <summary>价格类型代码，如 VTO / NEG / POA（price_type）</summary>
        public string PriceType { get; set; }
        /// <summary>管理费/维护费（tep_maintenance_fee）</summary>
        public int MaintenanceFee { get; set; }

        // ── 面积 ─────────────────────────────────────────────────────
        /// <summary>建筑面积 sqft（floorarea）</summary>
        public int? FloorArea { get; set; }
        /// <summary>土地面积 sqft（landarea），有地住宅专用</summary>
        public int? LandArea { get; set; }

        // ── 房间配置 ─────────────────────────────────────────────────
        /// <summary>卧室数量（bedrooms）</summary>
        public int? Bedrooms { get; set; }
        /// <summary>浴室数量（bathrooms）</summary>
        public int? Bathrooms { get; set; }
        /// <summary>房间类型，如 COMMON / MASTER（room_type）</summary>
        public string RoomType { get; set; }

        // ── 地址 ─────────────────────────────────────────────────────
        /// <summary>邮政编码（postcode）</summary>
        public string PostalCode { get; set; }
        /// <summary>街道名（streetname）</summary>
        public string StreetName { get; set; }
        /// <summary>门牌号（streetnumber）</summary>
        public string StreetNumber { get; set; }
        /// <summary>经度（longitude）</summary>
        public double Longitude { get; set; }
        /// <summary>纬度（latitude）</summary>
        public double Latitude { get; set; }
        /// <summary>楼层号（property_level_number）</summary>
        public string FloorNumber { get; set; }
        /// <summary>单元号（property_unit_number）</summary>
        public string UnitNumber { get; set; }

        // ── 单元属性 ─────────────────────────────────────────────────
        /// <summary>楼层代码，如 GND / MID / HIGH（floor_level）</summary>
        public string FloorLevel { get; set; }
        /// <summary>家具配置代码，如 FULL / PART / UNFURN（furnishing）</summary>
        public string Furnishing { get; set; }
        /// <summary>天花板高度（ceiling_height），工业专用</summary>
        public string CeilingHeight { get; set; }
        /// <summary>是否有租客：Yes / No（srx_tenanted）</summary>
        public string SrxTenanted { get; set; }
        /// <summary>现有租约到期日（tenanted_until）</summary>
        public string TenantedUntil { get; set; }
        /// <summary>卖家/房东种族，如 C / M / I / O（sellerEthnic）</summary>
        public string SellerEthnic { get; set; }
        /// <summary>卖家/房东居民身份，如 SC / PR（sellerResidency）</summary>
        public string SellerResidency { get; set; }
        /// <summary>设施/配套代码列表，如 AIRC / INET（unit_features[]）</summary>
        public List<string> UnitFeatures { get; set; } = new List<string>();

        // ── 工业 / 商业专用 ──────────────────────────────────────────
        /// <summary>供电量安培数（electricity_supply）</summary>
        public int? ElectricitySupply { get; set; }
        /// <summary>电力相数代码：1（单相）/ 3（三相）（electricity_phase）</summary>
        public string ElectricityPhase { get; set; }
        /// <summary>地板承重描述（floor_loading）</summary>
        public string FloorLoading { get; set; }
        /// <summary>地板承重等级代码，如 LIGHT / HEAVY（floor_loading_category）</summary>
        public string FloorLoadingCategory { get; set; }
        /// <summary>是否高天花板（is_high_ceiling）</summary>
        public string IsHighCeiling { get; set; }
        /// <summary>货梯数量（lift_cargo）</summary>
        public int? LiftCargo { get; set; }
        /// <summary>客梯数量（lift_passenger）</summary>
        public int? LiftPassenger { get; set; }
        /// <summary>是否有坡道（ramp）</summary>
        public string Ramp { get; set; }
        /// <summary>装修/交付条件代码，如 BARE / RENO（condition）</summary>
        public string Condition { get; set; }
        /// <summary>物业用途代码，如 ASMB / WRHSE（property_use）</summary>
        public string PropertyUse { get; set; }
        /// <summary>烹饪类型，如 GAS / ELEC / BOTH（cooking_type）</summary>
        public string CookingType { get; set; }

        // ── 经纪人信息 ───────────────────────────────────────────────
        /// <summary>联合代理 ID（alternative_agent）</summary>
        public string AlternativeAgent { get; set; }
        /// <summary>联合代理手机（alternative_mobile）</summary>
        public string AlternativeMobile { get; set; }
        /// <summary>联合代理电话（alternative_phone）</summary>
        public string AlternativePhone { get; set; }
        /// <summary>联合代理邮箱（alternative_email）</summary>
        public string AlternativeEmail { get; set; }

        // ── 任务信息 ─────────────────────────────────────────────────
        /// <summary>PG 已有 Listing ID（hidden_listing_id），用于更新操作</summary>
        public int? HiddenListingId { get; set; }
        /// <summary>任务项 ID（taskitem_id）</summary>
        public string TaskItemId { get; set; }
        /// <summary>最后更新时间（UpdateTime）</summary>
        public string UpdateTime { get; set; }
        /// <summary>快速重发标记：0 正常 / 1 快速（FastRepost）</summary>
        public string FastRepost { get; set; }
    }

    /// <summary>从 XML Listing 文件解析出的原始字段模型（顶层字段 + ProjectData 子模型 + Details 子模型）</summary>
    public class GuruListingData
    {
        // ── 顶层 Listing 字段 ────────────────────────────────────────
        /// <summary>Listing 内部 ID（&lt;ID&gt;）</summary>
        public int ListingId { get; set; }
        /// <summary>Listing XID（&lt;XID&gt;）</summary>
        public string XID { get; set; }
        /// <summary>楼盘名称（&lt;ListingName&gt;）</summary>
        public string ListingName { get; set; }
        /// <summary>挂牌类型（&lt;ListingType&gt;，SALE/RENT）</summary>
        public string ListingTypeRaw { get; set; }
        /// <summary>物业类型文字（&lt;PropertyType&gt;）</summary>
        public string PropertyType { get; set; }

        // ── 子模型 ───────────────────────────────────────────────────
        /// <summary>ProjectData 节点字段</summary>
        public GuruProjectData ProjectData { get; set; }
        /// <summary>Details 节点字段</summary>
        public GuruDetailsData Details { get; set; }
    }

    public class GuruTaskListing
    {
        public GuruTaskListing()
        {
            Listing = new ListingModel();
            Photos = new List<string>();
            Videos = new List<string>();
        }

        /// <summary>V2 格式数据（原有）</summary>
        public ListingModel Listing { get; set; }

        /// <summary>V3 格式数据，与 Listing 同步解析，方便后续直接调用 v3 API</summary>
        public CreateListingV3 ListingV3 { get; set; }

        /// <summary>XML 原始字段数据（ProjectData + Details 全量解析）</summary>
        public GuruListingData Data { get; set; }

        public List<string> Photos { get; set; }

        public List<string> Videos { get; set; }

        public List<string> PhotosTime { get; set; }

        public List<string> Tours { get; set; }

        public List<string> FloorPlan { get; set; }

        public bool NoGuruPhotos { get; set; }

        public bool NoiPropertyPhotos { get; set; }

        public bool NostPropertyPhotos { get; set; }

        public string iPropertyStatus { get; set; }

        public string stPropertyStatus { get; set; }

        public string RefencesNotes { get; set; }

        public bool UseFileName { get; set; }

        public int Id { get; set; }

        public string LastPost { get; set; }

        public int PostCount { get; set; }

        public string XID { get; set; }

        public string FastRepost { get; set; }

        public string TaskItemId { get; set; }

        public string UpdateTime { get; set; }
    }
}
