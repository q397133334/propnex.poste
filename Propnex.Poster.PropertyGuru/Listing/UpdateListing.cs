using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Propnex.Poster.PropertyGuru.Listing
{


    public class Titles
    {
        /// <summary>
        /// 
        /// </summary>
        public string en { get; set; }
    }


    public class PricePerArea
    {
        /// <summary>
        /// 
        /// </summary>
        public double? value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string reference { get; set; }
    }

    public class Type
    {
        /// <summary>
        /// 
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pretty { get; set; }
    }

    public class Price
    {
        /// <summary>
        /// 
        /// </summary>
        public int? value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pretty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string periodCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PricePerArea pricePerArea { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Type type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? valuation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string valuationText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? completed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string currency { get; set; }
    }

    public class Bedrooms
    {
        /// <summary>
        /// 
        /// </summary>
        public int? value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string text { get; set; }
    }

    public class Bathrooms
    {
        /// <summary>
        /// 
        /// </summary>
        public int? value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string text { get; set; }
    }

    public class Extrarooms
    {
        /// <summary>
        /// 
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string text { get; set; }
    }

    public class FloorAreaItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string unit { get; set; }

        private float? _value;

        /// <summary>
        /// 
        /// </summary>
        public float? value
        {
            get { return _value.HasValue ? _value + 0.01f : _value; }
            set { _value = value; }
        }

        private string _text;
        /// <summary>
        /// 
        /// </summary>
        public string text
        {
            get { return $"{Convert.ToInt32(value)} {unit}"; }
            set { _text = value; }
        }
    }

    public class LandAreaItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float? value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string text { get; set; }
    }

    public class Sizes
    {
        /// <summary>
        /// 
        /// </summary>
        public Bedrooms bedrooms { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Bathrooms bathrooms { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Extrarooms extrarooms { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<FloorAreaItem> floorArea { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<LandAreaItem> landArea { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string floorX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string floorY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string landX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string landY { get; set; }
    }

    public class PricePerArea1
    {
        /// <summary>
        /// 
        /// </summary>
        public List<FloorAreaItem> floorArea { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<LandAreaItem> landArea { get; set; }
    }

    public class Created
    {
        /// <summary>
        /// 
        /// </summary>
        public string date { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? unix { get; set; }
    }

    public class Updated
    {
        /// <summary>
        /// 
        /// </summary>
        public string date { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? unix { get; set; }
    }

    public class Dates
    {
        /// <summary>
        /// 
        /// </summary>
        public string timezone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string firstPosted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string lastPosted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string expiry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dates_Available available { get; set; } = null;
        /// <summary>
        /// 
        /// </summary>
        public Created created { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Updated updated { get; set; }
    }


    public class Agent
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mobilePretty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string phonePretty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string alternativePhone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string alternativeAgent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string alternativeMobile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string alternativeEmail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string jobTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string licenseNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? showProfile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string website { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string blackberryPin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string status { get; set; }
    }

    public class Agency
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ceaLicenseNumber { get; set; }
    }

    public class Location
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double? latitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double? longitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string distance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string regionCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string regionText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string regionSlug { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string districtCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string districtText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string districtSlug { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string areaCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string areaText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string areaSlug { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fullAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string hdbEstateCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string hdbEstateText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string postalCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string block { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string streetId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string streetName1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string streetName2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string streetNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string zoneIds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string subZoneIds { get; set; }

        public bool maskLocation { get; set; } = false;
    }

    public class AmenitiesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
    }

    public class Property
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string temporaryId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string typeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string typeText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string typeGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string tenureCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string tenureText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string topMonth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? topYear { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string developer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? totalUnits { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? floors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<AmenitiesItem> amenities { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string newProject { get; set; }
    }

    public class FeaturesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
    }

    public class Tenancy
    {
        /// <summary>
        /// 
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object tenantedUntilDate { get; set; }
    }

    public class MaintenanceFee
    {
        /// <summary>
        /// 
        /// </summary>
        public string periodeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pretty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double? value { get; set; }
    }

    public class PropertyUnit
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }

        public string centralAircon { get; set; }

        public string centralAirconHours { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string furnishingCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string furnishingText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string hdbTypeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? floorplanId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string floorLevelCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string floorLevelText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string floorPosition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string cornerUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string facingCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string occupancyCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? electricitySupply { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string electricityPhase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ceilingHeight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string floorLoading { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string garages { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string parkingSpaces { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string parkingFees { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MaintenanceFee maintenanceFee { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string liftCargo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string liftPassenger { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string liftCapacity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ownerTypeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sellerEthnic { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string sellerResidency { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string quotaEthnic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string quotaSpr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string telephoneLines { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<FeaturesItem> features { get; set; } = new List<FeaturesItem>();
        /// <summary>
        /// 
        /// </summary>
        public Tenancy tenancy { get; set; } = new Tenancy();

        public string ramp { get; set; } = null;
        public string isHighCeiling { get; set; } = null;
        public string floorLoadingCategory { get; set; } = null;
        public string roomType { get; set; } = "COMMON";
        public string cookingType { get; set; } = null;
        public string tenantGender { get; set; } = "ANY";
        public int maxTenants { get; set; } = 1;
        public string petFriendly { get; set; } = null;
        public string diversityFriendly { get; set; } = null;
        public string ownerStays { get; set; } = null;
        public string wifiIncluded { get; set; } = null;
        public string utilitiesIncluded { get; set; } = null;
        public string visitorsAllowed { get; set; } = null;
    }

    public class Cover
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? sortOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string V150 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string V550 { get; set; }
    }

    public class ListingItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? sortOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string V150 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string V550 { get; set; }
    }

    public class AgencyLogoItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? sortOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string V120 { get; set; }
    }

    public class ListingVideosItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string isConverted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string embed_html { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string file { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? sortOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string thumb { get; set; }
    }

    public class ListingVirtualToursItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string isConverted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string embed_html { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string file { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? sortOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string thumb { get; set; }
    }

    public class PropertyItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? sortOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string V150 { get; set; }
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public Cover cover { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ListingItem> listing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<PropertyItem> property { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string agent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<object> agentLogo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<AgencyLogoItem> agencyLogo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<int> excluded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> included { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> listingDocuments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ListingItem> propertyFloorplans { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ListingItem> listingFloorplans { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> listingSiteplans { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ListingVideosItem> listingVideos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ListingVirtualToursItem> listingVirtualTours { get; set; }
    }

    public class Metas
    {
        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 
        /// </summary>
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

            if (listing.Price.value == 0)
            {
                listing.Price.type.code = "POA";
            }
        }

        public string version { get; set; } = "v3";

        /// <summary>
        /// 
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? daysUntilExpire { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? isExpiring { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sourceCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string typeCode { get; set; }

        public string rentalType { get; set; } = "UNIT";

        /// <summary>
        /// 
        /// </summary>
        public string typeText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string subTypeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string leaseTermCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string leaseTermText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string featureCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string accountTypeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string accountSubTypeCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? isPremiumAccount { get; set; }

        public bool? isBumiLot { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? isPropertySpecialistListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? isMobilePropertySpotlightListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? isTransactorListing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? isCommercial { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? hasFloorplans { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? hasStream { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public List<string> featuredBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string localizedHeadline { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Headlines headlines { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string localizedTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Titles titles { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string localizedDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Descriptions descriptions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string notes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? isLiveTourAvailable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string externalId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string localizedTitleOnDb { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string titleOnDb { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string titlesOnDb { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? cobroke { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Price price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Sizes sizes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PricePerArea pricePerArea { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object dates { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string _user { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? qualityScore { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string finalScore { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? tier { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? showAgentProfile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Event { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mywebOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Agent agent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Agency agency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Location location { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Property property { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PropertyUnit propertyUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Media media { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Metas metas { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string alertBatchId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> unitTypes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> deals { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> dependencyErrors { get; set; }
        /// <summary>
        /// 
        /// </summary>
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
        public int durationInWeeks { get; set; }
        public bool isExpiring { get; set; }
    }

    public class ListingsListing : CreateOrUpdateListing
    {
        public List<Products> products { get; set; }

        public Charges charges { get; set; }
    }
}
