using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty.V1
{

    public class AddListingPayloadV1
    {
        public PayloadV1<AddListingDtoV1> addListing { get; set; }
    }

    public class UpdateListingPayloadV1
    {
        public PayloadV1<Listing> updateListing { get; set; }
    }

    public class PublishListingPayloadV1
    {
        public PayloadV1<Listing> publishListing { get; set; }
    }

    public class PayloadV1<T>
    {

        public T listing { get; set; }

        public List<T>? listings { get; set; }
    }

    public class AddListingDtoV1
    {
        public string Id { get; set; }

        public ListingItemTypeV1 Channel { get; set; } = new ListingItemTypeV1();

        public ListingItemTypeV1 PropertyType { get; set; } = new ListingItemTypeV1();

        public ListingListerV1 Lister { get; set; } = new ListingListerV1();

        public object? representationLister { get; set; }

        public string ListingRefNo { get; set; }

        public AddListingDxtensionV1 extension { get; set; }

        public ListingItemTypeV1 propertyCategoryType { get; set; }

        public bool? isAuction { get; set; }

        public DateTime? auctionDate { get; set; }

        public DateTime? PostedDate { get; set; }

        public string __typename { get; set; } = "Listing";
    }

    public class AddListingDxtensionV1
    {
        public bool isCoAgency { get; set; } = false;

        public ListingItemTypeV1 listingExclusivity { get; set; } = new ListingItemTypeV1();
    }
}
