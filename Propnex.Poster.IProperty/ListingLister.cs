using Propnex.Poster.IProperty.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class ListingLister
    {
        public string Id { get; set; }

        public ListingMultiLangTextV1? FullName { get; set; } = new ListingMultiLangTextV1();

        public ListingMultiLangTextV1? firstName { get; set; } = new ListingMultiLangTextV1();

        public ListingMultiLangTextV1? lastName { get; set; } = new ListingMultiLangTextV1();


        public string? Email { get; set; }

        public string __typename { get; set; } = "User";
    }
}
