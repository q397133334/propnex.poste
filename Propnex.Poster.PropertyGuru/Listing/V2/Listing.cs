namespace Propnex.Poster.PropertyGuru.Listing.V2
{
    public class ListingModel : IdModel
    {
        public ListingModel()
        {
            this.Id = null;
        }

        public string UpdateTime { get; set; }

        public string Title { get; set; }

        public string LocalizedTitle { get; set; }

        public string Description { get; set; }

        private string _LocalizedDescription = "";

        public string LocalizedDescription
        {
            get
            {
                if (_LocalizedDescription.Length > 2000)
                {
                    _LocalizedDescription = _LocalizedDescription.Substring(0, 1999);
                }
                return _LocalizedDescription;
            }
            set { _LocalizedDescription = value; }
        }

        public bool HasStream { get; set; } = false;

        public string StatusCode { get; set; } = "DRAFT";

        public string SourceCode { get; set; }

        public string TypeCode { get; set; }

        public string LeaseTermCode { get; set; }

        public string FeatureCode { get; set; }

        public string ExternalId { get; set; }

        public string Event { get; set; }

        public Location Location { get; set; } = new Location();

        public Media Media { get; set; } = new Media();

        public Property Property { get; set; } = new Property();

        public PropertyUnit PropertyUnit { get; set; } = new PropertyUnit();

        public Price Price { get; set; } = new Price();

        public Sizes Sizes { get; set; } = new Sizes();

        public Agent Agent { get; set; } = new Agent();

        public bool HasFloorplans { get; set; } = false;

        public Boost Boost { get; set; } = new Boost();

        public Dates Dates { get; set; } = new Dates();

        public string SubTypeCode { get; set; }

        public Descriptions Descriptions { get; set; } = new Descriptions();

        public int QualityScore { get; set; }

        public string LocalizedHeadline { get; set; } = "";

        public Headlines Headlines { get; set; } = new Headlines();

        public bool IsLiveTourAvailable { get; set; } = false;

    }
}
