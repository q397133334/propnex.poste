using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty.V1
{
    public class QuoteDto
    {
        public int? cost { get; set; }

        public string description { get; set; }

        public bool? disabled { get; set; }

        public bool? isPromo { get; set; }

        public ListingItemType listingProduct { get; set; }

        public int? productCode { get; set; }

        public int? productDuration { get; set; }

        public dynamic? promoBase { get; set; }

        public string quoteId { get; set; }

        public string reaId { get; set; }

        public int? remaining { get; set; }

        public string title { get; set; }
    }

    public class ListingQuoteDto
    {
        public List<QuoteDto> agentNetQuotes { get; set; }

        public string id { get; set; }
        public List<QuoteDto> quotes { get; set; }
    }
}
