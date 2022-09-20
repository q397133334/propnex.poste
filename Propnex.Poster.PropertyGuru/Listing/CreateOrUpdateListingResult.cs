using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Listing
{
    public class CreateOrUpdateListingResult
    {
        public int Id { get; set; }
        //public string ExternalId { get; set; }
        //public int QualityScore { get; set; }
        //public Dictionary<string, object> PropertyUnit { get; set; }
        //public Dictionary<string, object> Property { get; set; }
        //public Dictionary<string, object> Location { get; set; }
        //public Dictionary<string, object> ListingQuality { get; set; }
        public object errors { get; set; }
    }
}
