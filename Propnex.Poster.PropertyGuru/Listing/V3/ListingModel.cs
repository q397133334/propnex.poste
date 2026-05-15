using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Listing.V3
{
    public class PostedOnBrandsType
    {
        public bool? pg { get; set; }

        public bool? ipp { get; set; }

    }

    public class VerificationType
    {
        public bool isVerified { get; set; }

        public object? expiryDate { get; set; }

        public string statusUpdatedAt { get; set; }

        public string status { get; set; }

    }

    public class PublishingSourceType
    {
        public string code { get; set; }

    }

    public class LeaseType
    {
        public object? code { get; set; }

        public object? text { get; set; }

        public object? remaining { get; set; }

    }

    public class ListingTypeType
    {
        public string code { get; set; }

        public object? subTypeCode { get; set; }

        public string text { get; set; }

    }

    public class TitleType
    {
        public string en { get; set; }

    }

    public class HeadlinesType
    {
        public string text { get; set; }

        public string locale { get; set; }

        public string brand { get; set; }

    }

    public class DescriptionsType
    {
        public string text { get; set; }

        public string locale { get; set; }

        public string brand { get; set; }

    }

    public class CreatedType
    {
        public string date { get; set; }

        public int? timestamp { get; set; }

    }

    public class UpdatedType
    {
        public string date { get; set; }

        public int? timestamp { get; set; }

    }

    public class ContentUpdatedType
    {
        public string date { get; set; }

        public int? timestamp { get; set; }

    }

    public class DatesType
    {
        public string timezone { get; set; }

        public object? auction { get; set; }

        public object? available { get; set; }

        public CreatedType? created { get; set; }

        public object? expiry { get; set; }

        public object? firstPosted { get; set; }

        public object? lastPosted { get; set; }

        public UpdatedType? updated { get; set; }

        public ContentUpdatedType? contentUpdated { get; set; }

    }

    public class UrlsType
    {
    }

    public class OrganizationType
    {
        public int? id { get; set; }

        public object? legacyId { get; set; }

        public string name { get; set; }

        public string license { get; set; }

        public object? brandColor { get; set; }

        public object? address { get; set; }

        public List<object>? contacts { get; set; }

    }

    public class AccountType
    {
        public string code { get; set; }

        public object? subTypeCode { get; set; }

    }

    public class BadgesType
    {
        public object? academy { get; set; }

        public object? verification { get; set; }

    }

    public class ContactsType
    {
        public string type { get; set; }

        public string value { get; set; }

        public string pretty { get; set; }

    }

    public class AgentType
    {
        public int? id { get; set; }

        public object? legacyId { get; set; }

        public string name { get; set; }

        public bool? isVerified { get; set; }

        public string jobTitle { get; set; }

        public string description { get; set; }

        public string license { get; set; }

        public string statusCode { get; set; }

        public bool? showProfile { get; set; }

        public object? alternativeAgent { get; set; }

        public object? alternativeLicense { get; set; }

        public bool isPremiumAccount { get; set; }

        public AccountType? account { get; set; }

        public BadgesType? badges { get; set; }

        public bool? showSellerProfile { get; set; }

        public List<object>? agentAwards { get; set; }

        public List<object>? awards { get; set; }

        public List<ContactsType>? contacts { get; set; }

    }

    public class MetaByTypeType
    {
        public AgentType? agent { get; set; }

    }

    public class ListerType
    {
        public string type { get; set; }

        public MetaByTypeType? metaByType { get; set; }

    }

    public class AddressType
    {
        public string formatted { get; set; }

        public bool? maskUnitNumber { get; set; }

        public object? block { get; set; }

        public string unit { get; set; }

        public string floor { get; set; }

        public string streetNumber { get; set; }

        public string postalCode { get; set; }

        public bool? maskLocation { get; set; }

    }

    public class HdbEstateType
    {
        public object? code { get; set; }

        public object? text { get; set; }

    }

    public class PointType
    {
        public double? lat { get; set; }

        public double? lon { get; set; }

    }

    public class LevelsType
    {
        public string levelName { get; set; }

        public int? level { get; set; }

        public string id { get; set; }

        public string value { get; set; }

        public string slug { get; set; }

    }

    public class LocationType
    {
        public int? id { get; set; }

        public AddressType address { get; set; }

        public HdbEstateType hdbEstate { get; set; }

        public PointType? point { get; set; }

        public object? subZoneIds { get; set; }

        public object? zoneIds { get; set; }

        public List<LevelsType> levels { get; set; }

        public string street { get; set; }

        public object? streetName2 { get; set; }

        public object? streetId { get; set; }

        public bool noLevel600 { get; set; }

        public bool noLevel700 { get; set; }

        public bool noLevel800 { get; set; }

    }

    public class AgencyLogosType
    {
        public int? id { get; set; }

        public object? caption { get; set; }

        public string statusCode { get; set; }

        public int? sortOrder { get; set; }

        public object? thumbnailUrl { get; set; }

        public string type { get; set; }

        public string mimeType { get; set; }

        public string urlTemplate { get; set; }

        public bool isCover { get; set; }

    }

    public class CoverType
    {
        public int? id { get; set; }

        public object? caption { get; set; }

        public string statusCode { get; set; }

        public int? sortOrder { get; set; }

        public object? thumbnailUrl { get; set; }

        public string type { get; set; }

        public string mimeType { get; set; }

        public string urlTemplate { get; set; }

        public bool isCover { get; set; }

    }

    public class ListingImagesType
    {
        public int? id { get; set; }

        public object? caption { get; set; }

        public string statusCode { get; set; }

        public int? sortOrder { get; set; }

        public object? thumbnailUrl { get; set; }

        public string type { get; set; }

        public string mimeType { get; set; }

        public string urlTemplate { get; set; }

        public bool isCover { get; set; }

    }

    public class ListingFloorPlansType
    {
        public int? id { get; set; }

        public object? caption { get; set; }

        public string statusCode { get; set; }

        public int? sortOrder { get; set; }

        public object? thumbnailUrl { get; set; }

        public string type { get; set; }

        public string mimeType { get; set; }

        public string urlTemplate { get; set; }

        public bool isCover { get; set; }

    }

    public class ListingVideosType
    {
        public bool isConverted { get; set; }

        public string url { get; set; }

        public int? id { get; set; }

        public string type { get; set; }

        public string mimeType { get; set; }

        public object? caption { get; set; }

        public string statusCode { get; set; }

        public int? sortOrder { get; set; }

        public string embedHtml { get; set; }

        public string thumbnailUrl { get; set; }

        public bool isCover { get; set; }

    }

    public class ProjectType
    {
        public int? id { get; set; }

        public object? caption { get; set; }

        public string statusCode { get; set; }

        public int? sortOrder { get; set; }

        public object? thumbnailUrl { get; set; }

        public string type { get; set; }

        public string mimeType { get; set; }

        public string urlTemplate { get; set; }

    }

    public class UnitTypesType
    {
        public List<object> listing { get; set; }

        public List<object> project { get; set; }

    }

    public class MediaType
    {
        public List<AgencyLogosType> agencyLogos { get; set; }

        public List<AgentType> agent { get; set; }

        public List<object> agentLogos { get; set; }

        public List<CoverType> cover { get; set; }

        public List<object> developerLogos { get; set; }

        public List<object> excludedProjectPhotos { get; set; }

        public List<object> includedProjectFloorPlans { get; set; }

        public List<object> includedVirtualTours { get; set; }

        public bool hasFloorPlans { get; set; }

        public bool hasStream { get; set; }

        public List<object> included { get; set; }

        public List<ListingImagesType> listingImages { get; set; }

        public List<object> listingAwardBadges { get; set; }

        public List<object> listingDocuments { get; set; }

        public List<ListingFloorPlansType> listingFloorPlans { get; set; }

        public List<object> listingHeroVideos { get; set; }

        public List<object> listingSitePlans { get; set; }

        public List<ListingVideosType> listingVideos { get; set; }

        public List<object> listingVirtualTours { get; set; }

        public List<object> projectLogos { get; set; }

        public List<ProjectType> project { get; set; }

        public List<object> projectFloorPlans { get; set; }

        public List<object> projectSitePlans { get; set; }

        public List<object> propertyBrochures { get; set; }

        public List<object> reviewCovers { get; set; }

        public UnitTypesType unitTypes { get; set; }

    }

    public class OfferingsType
    {
        public bool isFeaturedListing { get; set; }

        public bool isPropertySpecialistListing { get; set; }

    }

    public class FloorType
    {
        public string text { get; set; }

        public string uom { get; set; }

        public double? min { get; set; }

        public double? max { get; set; }

    }

    public class PerAreaType
    {
        public List<object>? land { get; set; }

        public List<FloorType>? floor { get; set; }

    }

    public class PriceType
    {
        public int? min { get; set; }

        public int? max { get; set; }

        public string formatted { get; set; }

        public int? maintenanceFee { get; set; }

        public object? maintenanceFeePerUnit { get; set; }

        public PerAreaType? perArea { get; set; }

        public object? type { get; set; }

        public string currency { get; set; }

    }

    public class LandType
    {
        public object? length { get; set; }

        public object? width { get; set; }

        public List<object>? size { get; set; }

    }

    public class RoomType
    {
        public List<object>? size { get; set; }

    }

    public class DimensionsType
    {
        public FloorType? floor { get; set; }

        public LandType? land { get; set; }

        public RoomType? room { get; set; }

    }

    public class BedroomsType
    {
        public object? value { get; set; }

        public object? text { get; set; }

    }

    public class BathroomsType
    {
        public object? value { get; set; }

        public object? text { get; set; }

    }

    public class ExtraroomsType
    {
        public object? value { get; set; }

        public object? text { get; set; }

    }

    public class ConfigurationType
    {
        public BedroomsType? bedrooms { get; set; }

        public BathroomsType? bathrooms { get; set; }

        public ExtraroomsType? extrarooms { get; set; }

    }

    public class ConditionType
    {
        public string code { get; set; }

        public string description { get; set; }

    }

    public class FeaturesType
    {
        public string code { get; set; }

        public string description { get; set; }

    }

    public class TenancyType
    {
        public object? tenantedUntilDate { get; set; }

        public string value { get; set; }

    }

    public class ElectricityType
    {
        public object? phase { get; set; }

        public object? supply { get; set; }

    }

    public class FloorLevelType
    {
        public string code { get; set; }

        public string description { get; set; }

    }

    public class LiftType
    {
        public object? capacity { get; set; }

        public object? cargo { get; set; }

        public object? totalPassenger { get; set; }

    }

    public class UnitDetailsType
    {
        public bool? isAvailableNow { get; set; }

        public object? titleType { get; set; }

        public object? landTitleType { get; set; }

        public object? rentalType { get; set; }

        public DimensionsType? dimensions { get; set; }

        public ConfigurationType? configuration { get; set; }

        public ConditionType? condition { get; set; }

        public object? furnishing { get; set; }

        public List<FeaturesType>? features { get; set; }

        public List<object>? furnishingDetails { get; set; }

        public bool? isBumiLot { get; set; }

        public object? ceilingHeight { get; set; }

        public TenancyType? tenancy { get; set; }

        public List<object>? propertyUses { get; set; }

        public object? centralAircon { get; set; }

        public object? centralAirconHours { get; set; }

        public ElectricityType? electricity { get; set; }

        public object? direction { get; set; }

        public object? floorLoadingCapacity { get; set; }

        public object? floorLoadingCategory { get; set; }

        public FloorLevelType? floorLevel { get; set; }

        public object? hdbType { get; set; }

        public object? isHighCeiling { get; set; }

        public LiftType? lift { get; set; }

        public object? occupancy { get; set; }

        public object? ownerType { get; set; }

        public object? parkingFee { get; set; }

        public object? parkingSpots { get; set; }

        public object? quotaEthnic { get; set; }

        public object? quotaSpr { get; set; }

        public object? ramp { get; set; }

        public object? sellerEthnic { get; set; }

        public object? sellerResidency { get; set; }

        public object? roomType { get; set; }

        public object? cookingType { get; set; }

        public object? tenantGender { get; set; }

        public object? maxTenants { get; set; }

        public object? petFriendly { get; set; }

        public bool? tenantEligibility { get; set; }

        public object? ownerStays { get; set; }

        public object? wifiIncluded { get; set; }

        public object? utilitiesIncluded { get; set; }

        public object? visitorsAllowed { get; set; }

    }

    public class MetasType
    {
        public string title { get; set; }

        public string description { get; set; }

        public string keywords { get; set; }

    }

    public class MetadataType
    {
        public string homeSchDistanceBucket { get; set; }

    }

    public class SchoolsType
    {
        public MetadataType? metadata { get; set; }

        public double? walkingDistanceKm { get; set; }

        public int? walkingDurationMins { get; set; }

        public string name { get; set; }

        public string id { get; set; }

        public string category { get; set; }

        public string subcategory { get; set; }

        public PointType? point { get; set; }

        public object? stationId { get; set; }

        public double? distanceKm { get; set; }

    }

    public class MrtType
    {
        public double? walkingDistanceKm { get; set; }

        public int? walkingDurationMins { get; set; }

        public string name { get; set; }

        public string id { get; set; }

        public PointType? point { get; set; }

        public string stationId { get; set; }

        public double? distanceKm { get; set; }

    }

    public class PointOfInterestType
    {
        public List<SchoolsType>? schools { get; set; }

        public List<MrtType>? mrt { get; set; }

    }

    public class ListingModel
    {
        public int? id { get; set; }

        public string statusCode { get; set; }

        public int? version { get; set; }

        public bool? cobroke { get; set; }

        public object? referenceNote { get; set; }

        public object? referenceNumber { get; set; }

        public string externalId { get; set; }

        public PostedOnBrandsType? postedOnBrands { get; set; }

        public int? qualityScore { get; set; }

        public object? notes { get; set; }

        public string crossListingType { get; set; }

        public VerificationType? verification { get; set; }

        public PublishingSourceType? publishingSource { get; set; }

        public LeaseType? lease { get; set; }

        public ListingTypeType? listingType { get; set; }

        public TitleType? title { get; set; }

        public List<HeadlinesType>? headlines { get; set; }

        public List<DescriptionsType>? descriptions { get; set; }

        public DatesType? dates { get; set; }

        public UrlsType? urls { get; set; }

        public OrganizationType? organization { get; set; }

        public ListerType? lister { get; set; }

        public LocationType? location { get; set; }

        public MediaType? media { get; set; }

        public OfferingsType? offerings { get; set; }

        public ProjectType? project { get; set; }

        public PriceType? price { get; set; }

        public UnitDetailsType? unitDetails { get; set; }

        public List<object>? unitTypes { get; set; }

        public object? propertySpecialist { get; set; }

        public MetasType? metas { get; set; }

        public object? qualityScoreDetails { get; set; }

        public object? rotation { get; set; }

        public bool? isSearchHidden { get; set; }

        public object? isUnifiedCredit { get; set; }

        public PointOfInterestType? pointOfInterest { get; set; }

        public List<object>? dependencyErrors { get; set; }

    }
}
