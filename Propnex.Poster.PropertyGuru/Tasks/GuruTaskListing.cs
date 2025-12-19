using Propnex.Poster.PropertyGuru.Listing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Tasks
{
    /// <summary>
    /// app 接口使用
    /// </summary>
    public class TaskListing
    {
        public string FastRepost { get; set; }

        public CreateOrUpdateListing Listing { get; set; }

        public List<string> Photos { get; set; }

        public List<string> Videos { get; set; }

        public List<string> Tours { get; set; }

        public List<string> FloorPlan { get; set; }
    }


    public class Retrieve
    {
        public string Account { get; set; }

        public string TargetPortal { get; set; }

        public string Id { get; set; }

        public string Password { get; set; }

        public List<CreateOrUpdateListing> Listings { get; set; }
    }

    public class GuruTaskListing
    {
        public GuruTaskListing()
        {
            Listing = new ListingModel();
            Photos = new List<string>();
            Videos = new List<string>();
        }

        public ListingModel Listing { get; set; }

        public List<string> Photos { get; set; }

        public List<string> Videos { get; set; }

        public List<string> PhotosTime { get; set; }

        public List<string> Tours { get; set; }

        public List<string> FloorPlan { get; set; }

        public bool NoGuruPhotos { get; set; }

        public bool NoiPropertyPhotos { get; set; }

        public bool NostPropertyPhotos { get; set; }

        public string iPropertyStatus { get; set; }

        public string stPropertyStatus { get; set; }

        public string RefencesNotes { get; set; }

        public bool UseFileName { get; set; }

        public int Id { get; set; }

        public string LastPost { get; set; }

        public int PostCount { get; set; }

        public string XID { get; set; }

        public string FastRepost { get; set; }

        public string TaskItemId { get; set; }

        public string UpdateTime { get; set; }
    }
}
