using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{

        // ── 根对象 ────────────────────────────────────────────
        public class Project
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("nano_id")]
            public string NanoId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("localised_name")]
            public string LocalisedName { get; set; }

            [JsonProperty("property_id")]
            public int PropertyId { get; set; }

            [JsonProperty("new_project")]
            public bool NewProject { get; set; }

            [JsonProperty("completion_year")]
            public int CompletionYear { get; set; }

            [JsonProperty("launch_year")]
            public int LaunchYear { get; set; }

            [JsonProperty("streetname")]
            public string StreetName { get; set; }

            [JsonProperty("streetnumber")]
            public string StreetNumber { get; set; }

            [JsonProperty("postcode")]
            public string PostCode { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("developer_name")]
            public string DeveloperName { get; set; }

            [JsonProperty("property_type_code")]
            public string PropertyTypeCode { get; set; }

            [JsonProperty("property_type_group")]
            public string PropertyTypeGroup { get; set; }

            [JsonProperty("adm_level1")]
            public string AdmLevel1 { get; set; }

            [JsonProperty("adm_level2")]
            public string AdmLevel2 { get; set; }

            [JsonProperty("adm_level3")]
            public string AdmLevel3 { get; set; }

            [JsonProperty("district_code")]
            public string DistrictCode { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("total_units")]
            public int TotalUnits { get; set; }

            [JsonProperty("remaining_unit")]
            public int? RemainingUnit { get; set; }

            [JsonProperty("updated_at")]
            public DateTime UpdatedAt { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("location")]
            public GeoLocation Location { get; set; }

            [JsonProperty("tenure")]
            public Tenure Tenure { get; set; }

            [JsonProperty("tenure_code")]
            public string TenureCode { get; set; }

            [JsonProperty("project_status")]
            public ProjectStatus ProjectStatus { get; set; }

            [JsonProperty("project_status_code")]
            public string ProjectStatusCode { get; set; }

            [JsonProperty("sales_status")]
            public SalesStatus SalesStatus { get; set; }

            [JsonProperty("timeline")]
            public Timeline Timeline { get; set; }

            [JsonProperty("maintenance_fee")]
            public PriceValue MaintenanceFee { get; set; }

            [JsonProperty("starting_price")]
            public PriceValue StartingPrice { get; set; }

            [JsonProperty("max_price")]
            public PriceValue MaxPrice { get; set; }

            [JsonProperty("project_land_area")]
            public AreaValue ProjectLandArea { get; set; }

            [JsonProperty("price_per_unit_from")]
            public PriceAreaValue PricePerUnitFrom { get; set; }

            [JsonProperty("bed")]
            public RangeValue Bed { get; set; }

            [JsonProperty("bath")]
            public RangeValue Bath { get; set; }

            [JsonProperty("parking")]
            public RangeValue Parking { get; set; }

            [JsonProperty("floor_area")]
            public FloorAreaRange FloorArea { get; set; }

            [JsonProperty("developer")]
            public Developer Developer { get; set; }

            [JsonProperty("buildings")]
            public List<Building> Buildings { get; set; }

            [JsonProperty("facilities")]
            public List<Facility> Facilities { get; set; }

            [JsonProperty("addresses")]
            public List<Address> Addresses { get; set; }

            [JsonProperty("key_points")]
            public List<KeyPoint> KeyPoints { get; set; }

            [JsonProperty("unit_types")]
            public List<UnitType> UnitTypes { get; set; }

            [JsonProperty("media")]
            public ProjectMedia Media { get; set; }

            [JsonProperty("derived_info")]
            public DerivedInfo DerivedInfo { get; set; }

            [JsonProperty("government")]
            public GovernmentInfo Government { get; set; }

            [JsonProperty("showroom")]
            public Showroom Showroom { get; set; }
        }

        // ── 地理位置 ──────────────────────────────────────────
        public class GeoLocation
        {
            [JsonProperty("lat")]
            public double Lat { get; set; }

            [JsonProperty("lon")]
            public double Lon { get; set; }
        }

        // ── 产权 ─────────────────────────────────────────────
        public class Tenure
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        // ── 项目状态 ──────────────────────────────────────────
        public class ProjectStatus
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        // ── 销售状态 ──────────────────────────────────────────
        public class SalesStatus
        {
            [JsonProperty("code")]
            public string Code { get; set; }
        }

        // ── 时间线 ────────────────────────────────────────────
        public class Timeline
        {
            [JsonProperty("completion_date")]
            public DateInfo CompletionDate { get; set; }

            [JsonProperty("launch_date")]
            public DateInfo LaunchDate { get; set; }

            [JsonProperty("en_bloc_date")]
            public DateInfo EnBlocDate { get; set; }

            [JsonProperty("new_project_until")]
            public DateInfo NewProjectUntil { get; set; }

            [JsonProperty("preview_date")]
            public DateInfo PreviewDate { get; set; }

            [JsonProperty("sold_out_date")]
            public DateInfo SoldOutDate { get; set; }
        }

        public class DateInfo
        {
            [JsonProperty("year")]
            public int Year { get; set; }

            [JsonProperty("month")]
            public int Month { get; set; }

            [JsonProperty("day")]
            public int Day { get; set; }
        }

        // ── 价格/面积通用值 ───────────────────────────────────
        public class PriceValue
        {
            [JsonProperty("value")]
            public decimal? Value { get; set; }

            [JsonProperty("unit")]
            public string Unit { get; set; }
        }

        public class PriceAreaValue : PriceValue
        {
            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class AreaValue
        {
            [JsonProperty("value")]
            public decimal? Value { get; set; }

            [JsonProperty("unit")]
            public string Unit { get; set; }
        }

        public class RangeValue
        {
            [JsonProperty("from")]
            public int? From { get; set; }

            [JsonProperty("to")]
            public int? To { get; set; }
        }

        public class FloorAreaRange
        {
            [JsonProperty("from")]
            public AreaValue From { get; set; }

            [JsonProperty("to")]
            public AreaValue To { get; set; }
        }

        // ── 开发商 ────────────────────────────────────────────
        public class Developer
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("main_developer_id")]
            public int MainDeveloperId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("established_year")]
            public int? EstablishedYear { get; set; }

            [JsonProperty("project_count")]
            public int? ProjectCount { get; set; }

            [JsonProperty("property_type")]
            public string PropertyType { get; set; }

            [JsonProperty("website_url")]
            public string WebsiteUrl { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("address")]
            public DeveloperAddress Address { get; set; }

            [JsonProperty("media")]
            public DeveloperMedia Media { get; set; }
        }

        public class DeveloperAddress
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("administrative_levels")]
            public AdministrativeLevels AdministrativeLevels { get; set; }
        }

        public class DeveloperMedia
        {
            [JsonProperty("owner_id")]
            public int OwnerId { get; set; }

            [JsonProperty("logos")]
            public List<MediaItem> Logos { get; set; }
        }

        // ── 楼栋 ─────────────────────────────────────────────
        public class Building
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("total_floors")]
            public int? TotalFloors { get; set; }

            [JsonProperty("total_units")]
            public int TotalUnits { get; set; }

            [JsonProperty("address_id")]
            public int AddressId { get; set; }

            [JsonProperty("completion_status")]
            public CompletionStatus CompletionStatus { get; set; }
        }

        public class CompletionStatus
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        // ── 设施 ─────────────────────────────────────────────
        public class Facility
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        // ── 地址 ─────────────────────────────────────────────
        public class Address
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("street_no")]
            public string StreetNo { get; set; }

            [JsonProperty("street_name")]
            public string StreetName { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("latitude")]
            public string Latitude { get; set; }

            [JsonProperty("longitude")]
            public string Longitude { get; set; }

            [JsonProperty("external_id")]
            public string ExternalId { get; set; }

            [JsonProperty("primary_address")]
            public bool PrimaryAddress { get; set; }

            [JsonProperty("administrative_levels")]
            public AdministrativeLevels AdministrativeLevels { get; set; }

            [JsonProperty("pointOfInterest")]
            public List<PointOfInterest> PointOfInterest { get; set; }
        }

        // ── 行政区划 ──────────────────────────────────────────
        public class AdministrativeLevels
        {
            [JsonProperty("level1")]
            public AdminLevel Level1 { get; set; }

            [JsonProperty("level2")]
            public AdminLevel Level2 { get; set; }

            [JsonProperty("level3")]
            public AdminLevel Level3 { get; set; }
        }

        public class AdminLevel
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        // ── 兴趣点（周边设施）────────────────────────────────
        public class PointOfInterest
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("category")]
            public string Category { get; set; }

            [JsonProperty("subcategory")]
            public string Subcategory { get; set; }

            [JsonProperty("stationId")]
            public string StationId { get; set; }

            [JsonProperty("distanceKm")]
            public string DistanceKm { get; set; }

            [JsonProperty("walkingDistanceKm")]
            public double WalkingDistanceKm { get; set; }

            [JsonProperty("walkingDurationMins")]
            public int WalkingDurationMins { get; set; }

            [JsonProperty("drivingDistanceKm")]
            public double? DrivingDistanceKm { get; set; }

            [JsonProperty("drivingDurationMins")]
            public int? DrivingDurationMins { get; set; }

            [JsonProperty("point")]
            public GeoLocation Point { get; set; }

            [JsonProperty("metadata")]
            public Dictionary<string, string> Metadata { get; set; }
        }

        // ── 关键卖点 ──────────────────────────────────────────
        public class KeyPoint
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("project_id")]
            public int ProjectId { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("descriptions")]
            public Dictionary<string, string> Descriptions { get; set; }
        }

        // ── 户型 ─────────────────────────────────────────────
        public class UnitType
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("bed")]
            public int Bed { get; set; }

            [JsonProperty("bath")]
            public int? Bath { get; set; }

            [JsonProperty("study")]
            public int? Study { get; set; }

            [JsonProperty("total_units")]
            public int? TotalUnits { get; set; }

            [JsonProperty("remaining_unit")]
            public int? RemainingUnit { get; set; }

            [JsonProperty("parking_spaces")]
            public int? ParkingSpaces { get; set; }

            [JsonProperty("number_of_floors")]
            public int? NumberOfFloors { get; set; }

            [JsonProperty("building_ids")]
            public List<int> BuildingIds { get; set; }

            [JsonProperty("floor_area")]
            public FloorAreaRange FloorArea { get; set; }

            [JsonProperty("land_area")]
            public FloorAreaRange LandArea { get; set; }

            [JsonProperty("price")]
            public PriceRange Price { get; set; }

            [JsonProperty("property_type")]
            public PropertyTypeInfo PropertyType { get; set; }

            [JsonProperty("media")]
            public UnitMedia Media { get; set; }
        }

        public class PriceRange
        {
            [JsonProperty("from")]
            public PriceValue From { get; set; }

            [JsonProperty("to")]
            public PriceValue To { get; set; }
        }

        // ── 物业类型 ──────────────────────────────────────────
        public class PropertyTypeInfo
        {
            [JsonProperty("property_group")]
            public CodeName PropertyGroup { get; set; }

            [JsonProperty("property_sub_group")]
            public CodeName PropertySubGroup { get; set; }

            [JsonProperty("property_type")]
            public CodeName PropertyType { get; set; }
        }

        public class CodeName
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        // ── 媒体 ─────────────────────────────────────────────
        public class MediaItem
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("ownerId")]
            public int OwnerId { get; set; }

            [JsonProperty("userId")]
            public int UserId { get; set; }

            [JsonProperty("mediaClass")]
            public string MediaClass { get; set; }

            [JsonProperty("mediaType")]
            public string MediaType { get; set; }

            [JsonProperty("statusCode")]
            public string StatusCode { get; set; }

            [JsonProperty("caption")]
            public string Caption { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }

            [JsonProperty("sortOrder")]
            public long SortOrder { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("isCover")]
            public bool IsCover { get; set; }

            [JsonProperty("bucketPath")]
            public string BucketPath { get; set; }

            [JsonProperty("V150")]
            public string V150 { get; set; }

            [JsonProperty("V550")]
            public string V550 { get; set; }

            [JsonProperty("V800")]
            public string V800 { get; set; }
        }

        public class ProjectMedia
        {
            [JsonProperty("owner_id")]
            public int OwnerId { get; set; }

            [JsonProperty("photos")]
            public List<MediaItem> Photos { get; set; }

            [JsonProperty("siteplans")]
            public List<MediaItem> SitePlans { get; set; }

            [JsonProperty("logos")]
            public List<MediaItem> Logos { get; set; }
        }

        public class UnitMedia
        {
            [JsonProperty("owner_id")]
            public int OwnerId { get; set; }

            [JsonProperty("floorplans")]
            public List<MediaItem> FloorPlans { get; set; }
        }

        // ── 衍生信息 ──────────────────────────────────────────
        public class DerivedInfo
        {
            [JsonProperty("total_floors")]
            public int? TotalFloors { get; set; }

            [JsonProperty("total_units")]
            public int TotalUnits { get; set; }

            [JsonProperty("buildings_count")]
            public int BuildingsCount { get; set; }

            [JsonProperty("property_type")]
            public PropertyTypeInfo PropertyType { get; set; }
        }

        // ── 政府信息 ──────────────────────────────────────────
        public class GovernmentInfo
        {
            [JsonProperty("reference_id")]
            public string ReferenceId { get; set; }

            [JsonProperty("reference_date")]
            public DateTime ReferenceDate { get; set; }
        }

        // ── 样板房 ────────────────────────────────────────────
        public class Showroom
        {
            [JsonProperty("showroom_address")]
            public Dictionary<string, string> ShowroomAddress { get; set; }

            [JsonProperty("localised_showroom_address")]
            public string LocalisedShowroomAddress { get; set; }
        }
   
}
