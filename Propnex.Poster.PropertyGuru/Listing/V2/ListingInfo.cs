namespace Propnex.Poster.PropertyGuru.Listing.V2
{
    public class ListingInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Score { get; set; }
        public string TypeCode { get; set; }
        public string StatusCode { get; set; }
        public string PropertyTypeCode { get; set; }
        public int Credits { get; set; }
        public int RepostCharge { get; set; }
        public bool IsBoosted { get; set; } = false;
        public bool IsTurbo { get; set; } = false;
        public string Sqft { get; set; }
        public string Prece { get; set; }
        public string StreetName { get; set; }
        public string PostCode { get; set; }
        public string StreetNumber { get; set; }
    }
}
