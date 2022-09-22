using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Tasks
{
    public class GuruTask
    {
        public string Id { get; set; }

        public string TaskType { get; set; }

        public string MaxDeay { get; set; }

        public string EndTime { get; set; }

        public string AccountId { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public string TargetPortal { get; set; }

        public string AccountNotes { get; set; }

        public string UserName { get; set; }

        public string UserGroup { get; set; }

        public string ListingFile { get; set; }

        public string ListingCategory { get; set; }

        public string Country { get; set; }

        public string Status { get; set; }

        public string Timing { get; set; }

        public string DropToPage { get; set; }

        public bool IsRetry { get; set; } = false;

        public int RetryCount { get; set; } = 0;

        public string Source { get; set; }

        public GuruTaskListings Listings { get; set; } = new GuruTaskListings();
    }
}
