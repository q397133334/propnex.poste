using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class ListingItemType
    {
        public string Id { get; set; }

        public int? Code { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public string __typename { get; set; }
    }
}
