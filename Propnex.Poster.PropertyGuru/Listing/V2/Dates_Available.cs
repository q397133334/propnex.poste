using System;

namespace Propnex.Poster.PropertyGuru.Listing.V2
{
    public class Dates_Available
    {
        public Dates_Available() { }

        private DateTime now = DateTime.UtcNow.Date;

        public string date { get; set; }

        public long unix { get => unix_timestamp(Convert.ToDateTime(date)); }

        public long unix_timestamp(DateTime dt)
        {
            TimeSpan unix_time = (dt.Date - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)unix_time.TotalSeconds;
        }
    }
}
