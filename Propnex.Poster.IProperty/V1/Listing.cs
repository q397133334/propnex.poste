using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty.V1
{
    public class Listing
    {
        public string Id { get; set; }

        public List<string>? Amenities { get; set; } = new List<string>();

        public int? Bathroom { get; set; }

        public int? Bedroom { get; set; }

        public ListingItemType? BedroomRef { get; set; } = new ListingItemType();

        public List<ListingItemType>? BuildingFacilities { get; set; }

        public int? CarPark { get; set; }

        public ListingItemType? Channel { get; set; } = new ListingItemType();

        public List<string>? Conditions { get; set; } = new List<string>();

        public DateTime? ContentUpdatedDate { get; set; }

        public ListingMultiLangText? Description { get; set; }

        public string? Direction { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public ListingExtension? Extension { get; set; } = new ListingExtension();

        public ListingAttributes? Attributes { get; set; } = new ListingAttributes();

        public DateTime? FirstPublishedDate { get; set; }

        public List<ListingImage>? FloorPlans { get; set; } = new List<ListingImage>();

        public List<ListingImage>? Photo360s { get; set; }

        public int? PropertyCategoryTypeCode { get; set; }

        public string? FloorZone { get; set; }

        public ListingMultiLangText? FullAddress { get; set; } = new ListingMultiLangText();

        public string? GrossArea { get; set; }

        public ListingItemType? GrossAreaMeasurement { get; set; }

        public string? Image360s { get; set; }

        public List<ListingImage>? Images { get; set; } = new List<ListingImage>();

        public string? Layout { get; set; }

        public string? ListerId { get; set; }

        public ListingItemType? ListingProduct { get; set; }

        public string? ListingRefNo { get; set; }

        public ListingLocation? Location { get; set; } = new ListingLocation();

        public DateTime? NextRotationDate { get; set; }

        public string? OrganisationId { get; set; }

        public string? OutdoorArea { get; set; }

        public DateTime? ProductExpiryDate { get; set; }

        public DateTime? ProductUpgradeDate { get; set; }

        public ListingItemType? PropertyType { get; set; } = new ListingItemType();

        public ListingSalePrice? RentPrice { get; set; } = new ListingSalePrice();

        public string? RepresentationListerId { get; set; }

        public int? SaleableArea { get; set; }

        public ListingItemType? SaleableAreaMeasurement { get; set; }

        public ListingSalePrice? SalePrice { get; set; } = new ListingSalePrice();

        public ListingItemType? Status { get; set; } = new ListingItemType();

        public string? StoreRoom { get; set; }

        public ListingMultiLangText? Title { get; set; }

        public List<ListingItemType>? UnitFeatures { get; set; }

        public List<ListingRichMedia>? Videos { get; set; }

        public List<string>? Views { get; set; }

        public List<ListingRichMedia>? Photo360players { get; set; }

        public string __typename { get; set; } = "Listing";

        public DateTime? ActionDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public ListingLister? Lister { get; set; } = new ListingLister();

        public DateTime? PostedDate { get; set; }

        public bool? quality { get; set; }

        public bool? isDegraded { get; set; }

        public bool? isAutoUpgrade { get; set; }

        public bool? isAuction { get; set; }

        public DateTime? auctionDate { get; set; }

        public string? reAdvertiseJobs { get; set; }

        public string? partnerRefId { get; set; }

        public ListingRequestorListingPermission? RequestorListingPermission { get; set; }
    }

    public class ListingItemType
    {
        public string Id { get; set; }

        public int? Code { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public string __typename { get; set; }
    }

    public class ListingExtension
    {
        public bool? IsSaleableAreaVerified { get; set; } = null;

        public string PropertyNo { get; set; }

        public bool? IsMigrationUpdateRequired { get; set; }

        public bool? IsWithinMigrationGracePeriod { get; set; }


        public string __typename { get; set; } = "Extension";
    }

    public class ListingAttributes
    {
        public bool? IsWithinMigrationGracePeriod { get; set; }

        public List<string>? MandatoryFields { get; set; }

        public int? CrossListTypeCode { get; set; }

        public string __typename { get; set; } = "ListingAttributes";
    }

    public class ListingRichMedia
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string __typename { get; set; } = "RichMedia";
    }

    public class ListingImage
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? Height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? Quality { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string __typename { get; set; } = "Image";
    }

    public class ListingMultiLangText
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string en_GB { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string zh_HK { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string zh_CN { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ms_MY { get; set; }

        [JsonIgnore]
        public string? __typename { get; set; }= "MultiLangText"

    }

    public class ListingLocation
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingLevel? Level1 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingLevel? Level2 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingLevel? Level3 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingLevel? Level4 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingLevel? Level5 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingMultiLangText? Address { get; set; } = new ListingMultiLangText();
    }

    public class ListingLevel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ListingMultiLangText? Text { get; set; }

        [JsonIgnore]
        public string __typename { get; set; }
    }

    public class ListingSalePrice
    {
        public int? Fixed { get; set; }

        public string CurrencyCode { get; set; } = "MYR";

        [JsonIgnore]
        public string __typename { get; set; } = "Price";
    }

    public class ListingLister
    {
        public string Id { get; set; }

        public ListingMultiLangText? FullName { get; set; } = new ListingMultiLangText();

        public ListingMultiLangText? firstName { get; set; } = new ListingMultiLangText();

        public ListingMultiLangText? lastName { get; set; } = new ListingMultiLangText();

        public string __typename { get; set; } = "User";
    }

    public class ListingRequestorListingPermission
    {
        /// <summary>
        /// 
        /// </summary>
        public bool? canDuplicateListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLister { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditRepresentationLister { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canReAdvertiseListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canPublishListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canUpgradeListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canModifyAutoUpgrade { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canOfflineListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canDeleteListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool canDowngradeListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canDegradeListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canExtendListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationFloor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationLevel1Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationLevel2Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationLevel3Text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationLevel5Text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? canEditLocationLevel5Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationPostalCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? canEditLocationCoordinate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string __typename { get; set; } = "RequestorListingPermission";
    }

    public enum ListingType
    {
        Draft = 1,
        Online = 2,
        Offline = 3,
        Expired = 5
    }
}
