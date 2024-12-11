using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{

    public class AddListingPayload
    {
        public Payload<AddListingDto> addListing { get; set; }
    }

    public class UpdateListingPayload
    {
        public Payload<V1.Listing> updateListing { get; set; }
    }

    public class PublishListingPayload
    {
        public Payload<V1.Listing> publishListing { get; set; }
    }

    public class Payload<T>
    {

        public T listing { get; set; }

        public List<T>? listings { get; set; }
    }

    public class AddListingDto
    {
        public string Id { get; set; }

        public ListingItemType Channel { get; set; } = new ListingItemType();

        public ListingItemType PropertyType { get; set; } = new ListingItemType();

        public ListingLister Lister { get; set; } = new ListingLister();

        public object? representationLister { get; set; }

        public string ListingRefNo { get; set; }

        public AddListingExtension extension { get; set; }

        public ListingItemType propertyCategoryType { get; set; }

        public bool? isAuction { get; set; }

        public DateTime? auctionDate { get; set; }

        public DateTime? PostedDate { get; set; }

        public string __typename { get; set; } = "Listing";
    }

    public class AddListingExtension
    {
        public bool? isCoAgency { get; set; } = false;

        public ListingItemType listingExclusivity { get; set; } = new ListingItemType();
    }
}
