using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Listing.V2
{

    public class Titles
    {
        public string en { get; set; }
    }

    public class PricePerArea
    {
        public double? value { get; set; }
        public string unit { get; set; }
        public string reference { get; set; }
    }

    public class Type
    {
        public string code { get; set; }
        public string text { get; set; }
        public string pretty { get; set; }
    }

    public class Price
    {
        public int? value { get; set; }
        public string pretty { get; set; }
        public string periodCode { get; set; }
        public PricePerArea pricePerArea { get; set; }
        public Type type { get; set; }
        public int? valuation { get; set; }
        public string valuationText { get; set; }
        public int? completed { get; set; }
        public string currency { get; set; }
    }

    public class Bedrooms
    {
        public int? value { get; set; }
        public string text { get; set; }
    }

    public class Bathrooms
    {
        public int? value { get; set; }
        public string text { get; set; }
    }

    public class Extrarooms
    {
        public string value { get; set; }
        public string text { get; set; }
    }

    public class FloorAreaItem
    {
        public string unit { get; set; }

        private float? _value;

        public float? value
        {
            get { return _value.HasValue ? _value + 0.01f : _value; }
            set { _value = value; }
        }

        private string _text;
        public string text
        {
            get { return $"{Convert.ToInt32(value)} {unit}"; }
            set { _text = value; }
        }
    }

    public class LandAreaItem
    {
        public string unit { get; set; }
        public float? value { get; set; }
        public string text { get; set; }
    }

    public class Sizes
    {
        public Bedrooms bedrooms { get; set; }
        public Bathrooms bathrooms { get; set; }
        public Extrarooms extrarooms { get; set; }
        public List<FloorAreaItem> floorArea { get; set; }
        public List<LandAreaItem> landArea { get; set; }
        public string floorX { get; set; }
        public string floorY { get; set; }
        public string landX { get; set; }
        public string landY { get; set; }
    }

    public class PricePerArea1
    {
        public List<FloorAreaItem> floorArea { get; set; }
        public List<LandAreaItem> landArea { get; set; }
    }

    public class Created
    {
        public string date { get; set; }
        public int? unix { get; set; }
    }

    public class Updated
    {
        public string date { get; set; }
        public int? unix { get; set; }
    }

    public class Dates
    {
        public string timezone { get; set; } = "Asia/Singapore";
        public string firstPosted { get; set; }
        public string lastPosted { get; set; }
        public string expiry { get; set; }
        public Dates_Available available { get; set; } = null;
        public Created created { get; set; }
        public Updated updated { get; set; }
    }

    public class Agent
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string mobile { get; set; }
        public string mobilePretty { get; set; }
        public string phone { get; set; }
        public string phonePretty { get; set; }
        public string alternativePhone { get; set; }
        public string alternativeAgent { get; set; }
        public string alternativeMobile { get; set; }
        public string alternativeEmail { get; set; }
        public string jobTitle { get; set; }
        public string licenseNumber { get; set; }
        public bool? showProfile { get; set; }
        public string website { get; set; }
        public string email { get; set; }
        public string blackberryPin { get; set; }
        public string status { get; set; }
    }

    public class Agency
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string ceaLicenseNumber { get; set; }
    }

    public class Location
    {
        public int? id { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string distance { get; set; }
        public string regionCode { get; set; }
        public string regionText { get; set; }
        public string regionSlug { get; set; }
        public string districtCode { get; set; }
        public string districtText { get; set; }
        public string districtSlug { get; set; }
        public string areaCode { get; set; }
        public string areaText { get; set; }
        public string areaSlug { get; set; }
        public string fullAddress { get; set; }
        public string hdbEstateCode { get; set; }
        public string hdbEstateText { get; set; }
        public string postalCode { get; set; }
        public string block { get; set; }
        public string unit { get; set; }
        public string streetId { get; set; }
        public string streetName1 { get; set; }
        public string streetName2 { get; set; }
        public string streetNumber { get; set; }
        public string zoneIds { get; set; }
        public string subZoneIds { get; set; }
        public bool maskLocation { get; set; } = false;
    }

    public class AmenitiesItem
    {
        public string code { get; set; }
        public string description { get; set; }
    }

    public class Property
    {
        public int? id { get; set; }
        public string temporaryId { get; set; }
        public string statusCode { get; set; }
        public string name { get; set; }
        public string typeCode { get; set; }
        public string typeText { get; set; }
        public string typeGroup { get; set; }
        public string tenureCode { get; set; }
        public string tenureText { get; set; }
        public string topMonth { get; set; }
        public int? topYear { get; set; }
        public string developer { get; set; }
        public int? totalUnits { get; set; }
        public int? floors { get; set; }
        public List<AmenitiesItem> amenities { get; set; }
        public string newProject { get; set; }
    }

    public class FeaturesItem
    {
        public string code { get; set; }
        public string description { get; set; }
    }

    public class Tenancy
    {
        public string value { get; set; }
        public object tenantedUntilDate { get; set; }
    }

    public class MaintenanceFee
    {
        public string periodeCode { get; set; }
        public string pretty { get; set; }
        public double? value { get; set; }
    }

    public class PropertyUnit
    {
        public int? id { get; set; }
        public string centralAircon { get; set; }
        public string centralAirconHours { get; set; }
        public string description { get; set; }
        public string furnishingCode { get; set; }
        public string furnishingText { get; set; }
        public string hdbTypeCode { get; set; }
        public int? floorplanId { get; set; }
        public string floorLevelCode { get; set; }
        public string floorLevelText { get; set; }
        public string floorPosition { get; set; }
        public string cornerUnit { get; set; }
        public string facingCode { get; set; }
        public string occupancyCode { get; set; }
        public int? electricitySupply { get; set; }
        public string electricityPhase { get; set; }
        public string ceilingHeight { get; set; }
        public string floorLoading { get; set; }
        public string garages { get; set; }
        public string parkingSpaces { get; set; }
        public string parkingFees { get; set; }
        public MaintenanceFee maintenanceFee { get; set; }
        public string liftCargo { get; set; }
        public string liftPassenger { get; set; }
        public string liftCapacity { get; set; }
        public string ownerTypeCode { get; set; }
        public string sellerEthnic { get; set; } = "";
        public string sellerResidency { get; set; } = "";
        public string quotaEthnic { get; set; }
        public string quotaSpr { get; set; }
        public string telephoneLines { get; set; }
        public List<FeaturesItem> features { get; set; } = new List<FeaturesItem>();
        public Tenancy tenancy { get; set; } = new Tenancy();
        public string ramp { get; set; } = null;
        public string isHighCeiling { get; set; } = null;
        public string floorLoadingCategory { get; set; } = null;
        public string roomType { get; set; } = "COMMON";
        public string cookingType { get; set; } = null;
        public string tenantGender { get; set; } = "ANY";
        public int? maxTenants { get; set; } = 1;
        public string petFriendly { get; set; } = null;
        public string diversityFriendly { get; set; } = null;
        public string ownerStays { get; set; } = null;
        public string wifiIncluded { get; set; } = null;
        public string utilitiesIncluded { get; set; } = null;
        public string visitorsAllowed { get; set; } = null;
    }

    public class Cover
    {
        public int? id { get; set; }
        public string caption { get; set; }
        public string statusCode { get; set; }
        public int? sortOrder { get; set; }
        public string V150 { get; set; }
        public string V550 { get; set; }
    }

    public class ListingItem
    {
        public int? id { get; set; }
        public string caption { get; set; }
        public string statusCode { get; set; }
        public int? sortOrder { get; set; }
        public string V150 { get; set; }
        public string V550 { get; set; }
    }

    public class AgencyLogoItem
    {
        public int? id { get; set; }
        public string caption { get; set; }
        public string statusCode { get; set; }
        public int? sortOrder { get; set; }
        public string V120 { get; set; }
    }

    public class ListingVideosItem
    {
        public string isConverted { get; set; }
        public int? id { get; set; }
        public string embed_html { get; set; }
        public string file { get; set; }
        public string type { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string caption { get; set; }
        public int? sortOrder { get; set; }
        public string statusCode { get; set; }
        public string thumb { get; set; }
    }

    public class ListingVirtualToursItem
    {
        public string isConverted { get; set; }
        public int? id { get; set; }
        public string embed_html { get; set; }
        public string file { get; set; }
        public string type { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string caption { get; set; }
        public int? sortOrder { get; set; }
        public string statusCode { get; set; }
        public string thumb { get; set; }
    }

    public class PropertyItem
    {
        public int? id { get; set; }
        public string caption { get; set; }
        public string statusCode { get; set; }
        public int? sortOrder { get; set; }
        public string V150 { get; set; }
        public string V550 { get; set; }
    }

    public class Media
    {
        public Media()
        {
            cover = new Cover();
            excluded = new List<int>();
            included = new List<string>();
        }

        public Cover cover { get; set; }
        public List<ListingItem> listing { get; set; }
        public List<PropertyItem> property { get; set; }
        public string agent { get; set; }
        public List<object> agentLogo { get; set; }
        public List<AgencyLogoItem> agencyLogo { get; set; }
        public List<int> excluded { get; set; }
        public List<string> included { get; set; }
        public List<string> listingDocuments { get; set; }
        public List<ListingItem> propertyFloorplans { get; set; }
        public List<ListingItem> listingFloorplans { get; set; }
        public List<string> listingSiteplans { get; set; }
        public List<ListingVideosItem> listingVideos { get; set; }
        public List<ListingVirtualToursItem> listingVirtualTours { get; set; }
    }

    public class Metas
    {
        public string title { get; set; }
        public string description { get; set; }
        public string keywords { get; set; }
    }

    public class CreateOrUpdateListing
    {
        public CreateOrUpdateListing() { }

        public CreateOrUpdateListing(ListingModel listing, string type = "Create")
        {

        }

        public void Create(ListingModel listing)
        {
            this.id = null;
            this.localizedDescription = listing.LocalizedDescription;
            this.hasStream = listing.HasStream;
            this.statusCode = listing.StatusCode;
            this.sourceCode = listing.SourceCode;
            this.rentalType =listing.RentalType;
            this.typeCode = listing.TypeCode;
            this.leaseTermCode = listing.LeaseTermCode;
            this.featureCode = listing.FeatureCode;
            this.externalId = listing.ExternalId;
            this.Event = listing.Event;
            this.location = listing.Location;
            this.media = listing.Media;
            this.property = listing.Property;
            this.propertyUnit = listing.PropertyUnit;
            this.price = listing.Price;
            this.sizes = listing.Sizes;
            this.agent = listing.Agent;
            this.hasFloorplans = listing.HasFloorplans;
            this.dates = listing.Dates;
            this.descriptions = listing.Descriptions;
            this.qualityScore = listing.QualityScore;
            this.localizedHeadline = listing.LocalizedHeadline;
            this.headlines = listing.Headlines;
            this.isLiveTourAvailable = listing.IsLiveTourAvailable;

            if (listing.Price.value == 0)
            {
                listing.Price.type.code = "POA";
            }
        }

        public void Update(ListingModel listing)
        {
            this.localizedDescription = listing.LocalizedDescription;
            if (location.id != listing.Location.id)
            {
                this.location = listing.Location;
            }
            location.unit = listing.Location.unit;
            if (property.id != listing.Property.id)
            {
                this.property = listing.Property;
            }

            var tempPropertyUnit = new List<FeaturesItem>();
            var oldTempPropertyUnit = new List<FeaturesItem>();
            //对象深拷贝 Copy
            this.propertyUnit.features.ForEach(item =>
            {
                tempPropertyUnit.Add(item);
            });
            listing.PropertyUnit.features.ForEach(item =>
            {
                oldTempPropertyUnit.Add(item);
            });
            this.propertyUnit = listing.PropertyUnit;
            this.propertyUnit.features = new List<FeaturesItem>();

            if (listing.PropertyUnit.tenancy != null)
            {
                if (propertyUnit.tenancy.value != listing.PropertyUnit.tenancy.value)
                {
                    listing.PropertyUnit.tenancy = listing.PropertyUnit.tenancy;
                }
            }

            foreach (var item in oldTempPropertyUnit)
            {
                if (tempPropertyUnit.Where(q => q.code == item.code).Count() > 0)
                {
                    propertyUnit.features.Add(tempPropertyUnit.Where(q => q.code == item.code).FirstOrDefault());
                }
                else
                {
                    propertyUnit.features.Add(item);
                }
            }

            if (price.value != listing.Price.value)
            {
                this.price = listing.Price;
            }
            this.dates = listing.Dates;
            this.sizes = listing.Sizes;
            //this.agent = listing.Agent;
            this.descriptions = listing.Descriptions;
            this.leaseTermCode = listing.LeaseTermCode;
            headlines = listing.Headlines;
            //localizedHeadline = listing.LocalizedHeadline;
            this.isLiveTourAvailable = listing.IsLiveTourAvailable;
            if (listing.TypeCode == "ROOM")
            {
                this.typeCode = listing.TypeCode;
            }
            this.rentalType= listing.RentalType;
            if (listing.Price.value == 0)
            {
                listing.Price.type.code = "POA";
            }
        }

        public string version { get; set; } = "v3";

        public int? id { get; set; }
        public string statusCode { get; set; }
        public int? daysUntilExpire { get; set; }
        public bool? isExpiring { get; set; }
        public string sourceCode { get; set; }
        public string typeCode { get; set; }
        public string rentalType { get; set; } = "UNIT";
        public string typeText { get; set; }
        public string subTypeCode { get; set; }
        public string leaseTermCode { get; set; }
        public string leaseTermText { get; set; }
        public string featureCode { get; set; }
        public string accountTypeCode { get; set; }
        public string accountSubTypeCode { get; set; }
        public bool? isPremiumAccount { get; set; }
        public bool? isBumiLot { get; set; }
        public bool? isPropertySpecialistListing { get; set; }
        public bool? isMobilePropertySpotlightListing { get; set; }
        public bool? isTransactorListing { get; set; }
        public bool? isCommercial { get; set; }
        public bool? hasFloorplans { get; set; }
        public bool? hasStream { get; set; } = false;
        public List<string> featuredBy { get; set; }
        public string localizedHeadline { get; set; }
        public Headlines headlines { get; set; }
        public string localizedTitle { get; set; }
        public Titles titles { get; set; }
        public string localizedDescription { get; set; }
        public Descriptions descriptions { get; set; }
        public string notes { get; set; }
        public bool? isLiveTourAvailable { get; set; }
        public string externalId { get; set; }
        public string localizedTitleOnDb { get; set; }
        public string titleOnDb { get; set; }
        public string titlesOnDb { get; set; }
        public int? cobroke { get; set; }
        public Price price { get; set; }
        public Sizes sizes { get; set; }
        public PricePerArea pricePerArea { get; set; }
        public object dates { get; set; }
        public string _user { get; set; }
        public int? qualityScore { get; set; }
        public string finalScore { get; set; }
        public int? tier { get; set; }
        public bool? showAgentProfile { get; set; }
        public string Event { get; set; }
        public string mywebOrder { get; set; }
        public Agent agent { get; set; }
        public Agency agency { get; set; }
        public Location location { get; set; }
        public Property property { get; set; }
        public PropertyUnit propertyUnit { get; set; }
        public Media media { get; set; }
        public Metas metas { get; set; }
        public string alertBatchId { get; set; }
        public List<string> unitTypes { get; set; }
        public List<string> deals { get; set; }
        public List<string> dependencyErrors { get; set; }
        public string isFeaturedListing { get; set; }
    }

    public class Charges
    {
        public int activate { get; set; }
        public int repost { get; set; }

        [JsonProperty("boost-v2")]
        public int BoostV2 { get; set; }
        public int premium { get; set; }
        public int spotlight { get; set; }

        [JsonProperty("boost-plus-spotlight")]
        public int BoostPlusSpotlight { get; set; }

        [JsonProperty("boost-plus-repost")]
        public int BoostPlusRepost { get; set; }
    }

    public class Products
    {
        public string productType { get; set; }
        public bool isActive { get; set; }
        public bool isAuto { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int creditsCharged { get; set; }
        public int? durationInWeeks { get; set; }
        public bool isExpiring { get; set; }
    }

    public class ListingsListing : CreateOrUpdateListing
    {
        public List<Products> products { get; set; }
        public Charges charges { get; set; }
    }
}
