using System.Collections.Generic;

namespace Propnex.Poster.PropertyGuru.Listing.V2
{
    public class ListingsResult
    {
        public int total { get; set; }
        public int page { get; set; }
        public int limit { get; set; }
        public int totalPages { get; set; }
        public string currency { get; set; }
        public string searchCriteriaText { get; set; }
        public object heading { get; set; }
        public List<object> humanizedParams { get; set; }
        public Metas metas { get; set; }
        public List<ListingsListing> listings { get; set; }
        public List<object> dependencyErrors { get; set; }
    }
}
