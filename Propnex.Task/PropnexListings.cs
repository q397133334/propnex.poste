using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Propnex
{
    public class PropnexListings
    {
        public PropnexListings() 
        { 
            Listings=new List<PropnexListing>();
        }

        public PropnexListings(string listingContent)
        {
            Listings = new List<PropnexListing>();
            XDocument Document;
            try
            {
                Document = XDocument.Parse(listingContent);
            }
            catch
            {
                StringBuilder info = new StringBuilder();
                foreach (char cc in listingContent)
                {
                    int ss = (int)cc;
                    if (((ss >= 0) && (ss <= 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss <= 32)))
                        info.AppendFormat(" ", ss);//&#x{0:X};
                    else info.Append(cc);
                }
                try
                {
                    Document = XDocument.Parse(info.ToString());
                }
                catch (Exception ex)
                {
                    Document = null;
                }
            }
            if (Document != null)
            {
                var root = Document.Root;
                var listings = root.Elements("Listing");
                foreach (var element in listings)
                {
                    var listing=new PropnexListing();
                    listing.NoGuruPhotos = element.ElementBool("NoGuruPhotos");
                    listing.NoiPropertyPhotos = element.ElementBool("NoiPropertyPhotos");
                    listing.NostPropertyPhotos = element.ElementBool("NostPropertyPhotos");
                    listing.UseFileName = element.ElementBool("UseFileName");
                    listing.Id = element.ElementInt("ID");
                    listing.XID = element.ElementString("XID");
                    listing.Photos = element.ElementString("Photos", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                    listing.PhotosTime = element.ElementString("PhotosTime", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                    listing.Videos = element.ElementString("Videos", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                    listing.Tours = element.ElementString("Tours", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                    listing.FloorPlan = element.ElementString("FloorPlan", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                    listing.ListingType= element.ElementString("ListingType");
                    listing.ListingName= element.ElementString("ListingName");

                    var projectData = element.Element("ProjectData");
                    if (projectData != null)
                    {
                        var fileds = projectData.Elements();
                        foreach(var item in fileds)
                        {
                            listing.ProjectData.Add(item.Attribute("Name").Value, item.Attribute("Value").Value);
                        }
                    }

                    var detials = element.Element("Details");
                    if (detials != null)
                    {
                        var files = detials.Elements();
                        foreach(var item in files)
                        {
                            listing.Details.Add(item.Attribute("Name").Value, item.Attribute("Value").Value);
                        }
                    }
                    
                    var basic = element.Element("Basic");
                    if(basic!=null)
                    {
                        var fileds = basic.Elements();
                        foreach (var item in fileds)
                        {
                            listing.Basic.Add(item.Attribute("Name").Value, item.Attribute("Value").Value);
                        }
                    }

                    var Location = element.Element("Location");
                    if (Location != null)
                    {
                        var fileds = Location.Elements();
                        foreach (var item in fileds)
                        {
                            listing.Location.Add(item.Attribute("Name").Value, item.Attribute("Value").Value);
                        }
                    }

                    var media = element.Element("Media");
                    if (media != null)
                    {
                        var fileds = media.Elements();
                        foreach (var item in fileds)
                        {
                            listing.Media.Add(item.Attribute("Name").Value, item.Attribute("Value").Value);
                        }
                    }

                    var sales = element.Element("Sales");
                    if (media != null)
                    {
                        var fileds = sales.Elements();
                        foreach (var item in fileds)
                        {
                            listing.Sales.Add(item.Attribute("Name").Value, item.Attribute("Value").Value);
                        }
                    }

                    var summary = element.Element("Summary");
                    if (media != null)
                    {
                        var fileds = summary.Elements();
                        foreach (var item in fileds)
                        {
                            listing.Summary.Add(item.Attribute("Name").Value, item.Attribute("Value").Value);
                        }
                    }

                    Listings.Add(listing);
                }
            }
        }

        public List<PropnexListing> Listings { get; set; }
    }

    public class PropnexListing
    {
        public PropnexListing() 
        { 
            Photos=new List<string>();
            PhotosTime=new List<string>();
            Tours=new List<string>();
            ToursThumbnail=new List<string>();
            Videos=new List<string>();
            VideosThumbnail=new List<string>();
            ProjectData = new Dictionary<string, string>();
            Details= new Dictionary<string, string>();
            Basic = new Dictionary<string, string>();
            Location = new Dictionary<string, string>();
            Media=new Dictionary<string, string>();
            Sales=new Dictionary<string, string>();
            Summary= new Dictionary<string, string>();
        }

        public string iPropertyStatus { get; set; }

        public string stPropertyStatus { get; set; }

        public string RefencesNotes { get; set; }

        public bool NoGuruPhotos { get; set; }

        public bool NoiPropertyPhotos { get; set; }

        public bool NostPropertyPhotos { get; set; }

        public bool UseFileName { get; set; }

        public int Id { get; set; }

        public string LastPost { get; set; }

        public int PostCount { get; set; }

        public string XID { get; set; }

        public string ListingName { get; set; }

        public string ListingType { get; set; }

        public string PropertyType { get; set; }

        public List<string> PhotosTime { get; set; }

        public List<string> Photos { get; set; }

        public List<string> Tours { get; set; }

        public List<string> ToursThumbnail { get; set; }

        public List<string> Videos { get; set; }

        public List<string> VideosThumbnail { get; set; }

        public List<string> FloorPlan { get; set; }

        public double Price { get; set; }

        public int BedRooms { get; set; }

        public int FloorArea { get; set; }

        public string Status { get; set; }

        public string Account { get; set; }

        public string IPropertyAccount { get; set; }

        public string stPropertyAccount { get; set; }

        public string EmbedVideo { get; set; }

        public Dictionary<string, string> ProjectData { get; set;}

        public Dictionary<string, string> Details { get; set; }

        public Dictionary<string, string> Basic { get; set; }

        public Dictionary<string, string> Location { get; set; }

        public Dictionary<string, string> Media { get; set; }

        public Dictionary<string, string> Sales { get; set; }

        public Dictionary<string, string> Summary { get; set; }


    }
}
