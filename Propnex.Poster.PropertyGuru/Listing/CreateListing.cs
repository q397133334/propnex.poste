using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Propnex.Poster.PropertyGuru.Listing
{
    public partial class CreateListing
    {
        public CreateListing() 
        {
            Agent = new CreateAgent();
            Dates = new CreateDates();
            Location= new CreateLocation();
            Price=new CreatePrice();
            Property=new CreateProperty();
            PropertyUnit=new CreatePropertyUnit();
            Sizes= new CreateSizes();

        }

        [JsonProperty("agent")]
        public CreateAgent Agent { get; set; }

        [JsonProperty("dates")]
        public CreateDates Dates { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("headline")]
        public string Headline { get; set; }

        [JsonProperty("isLiveTourAvailable")]
        public bool IsLiveTourAvailable { get; set; }

        [JsonProperty("localizedDescription")]
        public string LocalizedDescription { get; set; }

        [JsonProperty("localizedHeadline")]
        public string LocalizedHeadline { get; set; }

        [JsonProperty("location")]
        public CreateLocation Location { get; set; }

        [JsonProperty("price")]
        public CreatePrice Price { get; set; }

        [JsonProperty("property")]
        public CreateProperty Property { get; set; }

        [JsonProperty("propertyUnit")]
        public CreatePropertyUnit PropertyUnit { get; set; }

        [JsonProperty("sizes")]
        public CreateSizes Sizes { get; set; }

        [JsonProperty("statusCode")]
        public string StatusCode { get; set; } = "DRAFT";

        [JsonProperty("typeCode")]
        public string TypeCode { get; set; }
    }

    public partial class CreateAgent
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class CreateDates
    {
        [JsonProperty("available")]
        public CreateAvailable Available { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }

    public partial class CreateAvailable
    {
        [JsonProperty("unix")]
        public long Unix { get; set; }
    }

    public partial class CreateLocation
    {
        [JsonProperty("hdbEstateCode")]
        public string HdbEstateCode { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("streetName1")]
        public string StreetName1 { get; set; }

        [JsonProperty("streetNumber")]
        public string StreetNumber { get; set; }

        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }
    }

    public partial class CreatePrice
    {
        [JsonProperty("value")]
        public long Value { get; set; }
    }

    public partial class CreateProperty
    {
        [JsonProperty("developerName")]
        public string DeveloperName { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tenureCode")]
        public string TenureCode { get; set; }

        [JsonProperty("typeCode")]
        public string TypeCode { get; set; }

        [JsonProperty("typeGroup")]
        public string TypeGroup { get; set; }

        [JsonProperty("typeText")]
        public string TypeText { get; set; }
    }

    public partial class CreatePropertyUnit
    {
        [JsonProperty("features")]
        public System.Collections.Generic.List<CreateFeature> Features { get; set; }

        [JsonProperty("hdbTypeCode")]
        public string HdbTypeCode { get; set; }

        [JsonProperty("tenancy")]
        public Tenancy Tenancy { get; set; }
    }

    public partial class CreateFeature
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public partial class CreateTenancy
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class CreateSizes
    {
        [JsonProperty("bathrooms")]
        public CreateBathrooms Bathrooms { get; set; }

        [JsonProperty("bedrooms")]
        public CreateBedrooms Bedrooms { get; set; }

        [JsonProperty("floorArea")]
        public List<CreateFloorArea> FloorArea { get; set; }
    }

    public partial class CreateBathrooms
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class CreateBedrooms
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class CreateFloorArea
    {
        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public partial class CreateListing
    {
        public static CreateListing FromJson(string json) => JsonConvert.DeserializeObject<CreateListing>(json, Propnex.Poster.PropertyGuru.Listing.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this CreateListing self) => JsonConvert.SerializeObject(self, Propnex.Poster.PropertyGuru.Listing.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
