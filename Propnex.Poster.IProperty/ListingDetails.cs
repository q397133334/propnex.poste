using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class DeleteListingDto
    {
        public string id { get; set; }
    }

    public class AddListingMutationDto
    {
        public DateTime? auctionDate { get; set; }

        public int? channelCode { get; set; }

        public ExtensionDto extension { get; set; } = new ExtensionDto();

        public bool? isAuction { get; set; } = false;

        public string? listingRefNo { get; set; }

        public NullClass location { get; set; }=new NullClass();

        public int? propertyCategoryTypeCode { get; set; }

        public int? propertyGroupTypeCode { get; set; }

        public int? propertyTypeCode { get; set; }

        public int? saleableAreaMeasurementCode { get; set; }

        public int? storeyCode { get; set; }
    }

    public class UpdateListingMutationDto:AddListingMutationDto
    { 
        public string id { get; set; }

    }


    public class NullClass
    {

    }

    public class ExtensionDto
    {
        public bool isCoAgency { get; set; } = false;
    }

    public class Variables<T>
    {
        public T input { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? shouldExtendsFields { get; set; } = null;


    }
}
