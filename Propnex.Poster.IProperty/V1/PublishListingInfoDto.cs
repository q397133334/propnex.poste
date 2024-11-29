using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty.V1
{
    public class PublishListingInfoDto
    {
        public string id { get; set; }

        public bool? isAutoUpgrade { get; set; } = false;

        public bool? isPostCrossListing { get; set; } = false;

        public List<PublishListingInfoQuote> quotes { get; set; } = new List<PublishListingInfoQuote>();
    }

    public class PublishListingInfoQuote
    {
        public int channelCode { get; set; }

        public List<string> quoteIds { get; set; } = new List<string>();
    }
}
