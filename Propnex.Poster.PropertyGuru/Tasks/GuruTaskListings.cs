using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Propnex.Poster.PropertyGuru.Tasks
{
    public class GuruTaskListings
    {
        public GuruTaskListings() { }
        public List<GuruTaskListing> Listings { get; set; }
        public XDocument Document { get; set; }

        public string TaskType { get; set; }

        public long TaskId { get; set; }

        public GuruTaskListings(string listsContext, string taskType = "Create")
        {
            Listings = new List<GuruTaskListing>();
            TaskType = taskType;
            try
            {
                Document = XDocument.Parse(listsContext);
                Init();
            }
            catch
            {
                StringBuilder info = new StringBuilder();
                foreach (char cc in listsContext)
                {
                    int ss = (int)cc;
                    if (((ss >= 0) && (ss <= 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss <= 32)))
                        info.AppendFormat(" ", ss);//&#x{0:X};
                    else info.Append(cc);
                }
                try
                {
                    Document = XDocument.Parse(info.ToString());
                    Init();
                }
                catch (Exception ex)
                {

                }
            }

        }

        private void Init()
        {
            if (Document == null)
                return;
            var root = Document.Root;
            var listings = root.Elements("Listing");
            foreach (var element in listings)
            {
                var projectData = element.Element("ProjectData");

                IEnumerable<XElement> projectDatas = new List<XElement>();

                if (projectData != null)
                {
                    projectDatas = projectData.Elements();
                }

                var detials = element.Element("Details");

                List<XElement> detialss = new List<XElement>();
                if (detials != null)
                {
                    detialss = detials.Elements().ToList();
                }

                var basic = element.Element("Basic");

                if (basic != null)
                {
                    var basics = basic.Elements();
                    foreach (var item in basics)
                    {
                        detialss.Add(item);
                    }
                }

                var location= element.Element("Location");
                if(location!=null)
                {
                    foreach(var item in location.Elements().ToList())
                    {
                        detialss.Add(item);
                    }
                }

                var listing = new GuruTaskListing();
                listing.NoGuruPhotos = element.ElementBool("NoGuruPhotos");
                listing.NoiPropertyPhotos = element.ElementBool("NoiPropertyPhotos");
                listing.NostPropertyPhotos = element.ElementBool("NostPropertyPhotos");
                listing.UseFileName = element.ElementBool("UseFileName");
                listing.Id = element.ElementInt("ID");
                listing.XID = element.ElementString("XID");
                listing.UpdateTime = detialss.FindAttribute("Name", "UpdateTime").GetAttributeValue("Value", "");


                listing.Photos = element.ElementString("Photos", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                listing.PhotosTime = element.ElementString("PhotosTime", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                listing.Videos = element.ElementString("Videos", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                listing.Tours = element.ElementString("Tours", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                listing.FloorPlan = element.ElementString("FloorPlan", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").ToList();
                listing.FastRepost = detialss.FindAttribute("Name", "FastRepost").GetAttributeValue("Value", "0");
                listing.TaskItemId = detialss.FindAttribute("Name", "taskitem_id").GetAttributeValue("Value", "0");



                //listing - projectData
                ListingModel listingModel = new ListingModel();
                listing.Listing = listingModel;
                listing.Listing.Agent = new Agent()
                {
                    id = 0,
                    alternativeAgent = detialss.FindAttribute("Name", "alternative_agent").GetAttributeValue("Value", ""),
                    alternativeEmail = detialss.FindAttribute("Name", "alternative_email").GetAttributeValue("Value", ""),
                    alternativeMobile = detialss.FindAttribute("Name", "alternative_mobile").GetAttributeValue("Value", ""),
                    alternativePhone = detialss.FindAttribute("Name", "alternative_phone").GetAttributeValue("Value", "")
                };

                listing.Listing.Id = detialss.FindAttribute("Name", "hidden_listing_id").GetAttributeValue<int?>("Value", null);
                listingModel.Title = projectDatas.FindAttribute("Name", "name").GetAttributeValue("Value", "");
                listingModel.LocalizedDescription = detialss.FindAttribute("Name", "listing_description").GetAttributeValue("Value", "");//.Replace("\n", "").Replace("\r", "");
                listingModel.TypeCode = detialss.FindAttribute("Name", "listing_type").GetAttributeValue("Value", "SALE");
                listing.Listing.LeaseTermCode = detialss.FindAttribute("Name", "lease_term").GetAttributeValue("Value", "");

                if (listingModel.TypeCode == "RENT")
                {
                    listingModel.Dates.available = new Dates_Available();
                    var available_date = detialss.FindAttribute("Name", "available_date").GetAttributeValue("Value", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    try
                    {
                        listingModel.Dates.available.Date = Convert.ToDateTime(available_date).ToString("yyyy-MM-dd 00:00:00");
                    }
                    catch { }
                    if (listingModel.Dates.available.Date == "")
                    {
                        listingModel.Dates.available.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (Convert.ToDateTime(listingModel.Dates.available.Date) < DateTime.Now)
                    {
                        listingModel.Dates.available.Date = DateTime.Now.AddDays(+1).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }


                listingModel.Location = new Location()
                {
                    id = detialss.FindAttribute("Name", "location_id").GetAttributeValue<int?>("Value", null),//projectData.ElementString("locationId"),
                    longitude = detialss.FindAttribute("Name", "longitude").GetAttributeValue("Value", 0.00),//projectData.ElementDouble("longitude"),
                    latitude = detialss.FindAttribute("Name", "latitude").GetAttributeValue("Value", 0.00),//projectData.ElementDouble("latitude"),
                    postalCode = detialss.FindAttribute("Name", "postcode").GetAttributeValue("Value", ""),//projectData.ElementString("postcode"),
                    regionCode = projectDatas.FindAttribute("Name", "regionCode").GetAttributeValue("Value", ""),//projectData.ElementString("regionCode"),
                    districtCode = detialss.FindAttribute("Name", "district").GetAttributeValue("Value", ""),//projectData.ElementString("districtCode")
                    unit = $"{detialss.FindAttribute("Name", "property_level_number").GetAttributeValue("Value", "01")}-{detialss.FindAttribute("Name", "property_unit_number").GetAttributeValue("Value", "01")}",
                    streetName1 = detialss.FindAttribute("Name", "streetname").GetAttributeValue("Value", ""),
                    hdbEstateCode = detialss.FindAttribute("Name", "hdb_estate").GetAttributeValue("Value", "")

                };

                listingModel.Property = new Property()
                {
                    id = detialss.FindAttribute("Name", "property_id").GetAttributeValue<int?>("Value", null),
                    name = detialss.FindAttribute("Name", "property_name").GetAttributeValue<string>("Value"),
                    typeCode = detialss.FindAttribute("Name", "property_type_code").GetAttributeValue("Value", ""),
                    typeGroup = detialss.FindAttribute("Name", "property_type_group").GetAttributeValue("Value", "N"),
                    floors = projectDatas.FindAttribute("Name", "floors").GetAttributeValue("Value", 0),
                    tenureCode = detialss.FindAttribute("Name", "tenure").GetAttributeValue("Value", ""),

                };

                listingModel.PropertyUnit = new PropertyUnit();
                listingModel.PropertyUnit.floorLevelCode = detialss.FindAttribute("Name", "floor_level").GetAttributeValue("Value", "").ToUpper();
                List<string> features = new List<string>()
                {
                    "AIRC",
                    "AUD",
                    "BAL",
                    "BATH",
                    "BED",
                    "BOMB",
                    "CAB",
                    "CITYV",
                    "COLO",
                    "COOK",
                    "CORN",
                    "DIN",
                    "DISH",
                    "DRY",
                    "DVD",
                    "FRI",
                    "GAR",
                    "GARD",
                    "GFLO",
                    "GREEN",
                    "HAIR",
                    "HFLO",
                    "INET",
                    "INT",
                    "IRO",
                    "JACZ",
                    "KUT",
                    "LAKEV",
                    "LFLO",
                    "LFUR",
                    "MAID",
                    "ORIG",
                    "OVEN",
                    "PAT",
                    "PENT",
                    "POOLV",
                    "PPOOL",
                    "RENO",
                    "ROOF",
                    "SEAV",
                    "TERR",
                    "TV",
                    "VAC",
                    "WAR",
                    "WAS",
                    "WHEAT"
                };
                foreach (var item in detialss)
                {
                    if (item.Attribute("Name").Value.Contains("unit_features"))
                    {
                        var code = item.Attribute("Name").Value.Replace("unit_features[],", "");
                        if (features.Where(q => q == code).Count() > 0)
                        {
                            listingModel.PropertyUnit.features.Add(new FeaturesItem() { code = code });
                        }

                    }
                }
                listingModel.PropertyUnit.furnishingCode = detialss.FindAttribute("Name", "furnishing").GetAttributeValue<string>("Value", null);
                if (listingModel.Property.typeCode == "CLAND")
                {
                    listingModel.PropertyUnit.furnishingCode = "";
                }
                listingModel.PropertyUnit.hdbTypeCode = detialss.FindAttribute("Name", "hdb_type").GetAttributeValue<string>("Value", null);
                listingModel.PropertyUnit.ceilingHeight = detialss.FindAttribute("Name", "ceiling_height").GetAttributeValue<string>("Value", null);
                listingModel.PropertyUnit.electricitySupply = detialss.FindAttribute("Name", "electricity_supply").GetAttributeValue<int?>("Value", null);
                listingModel.PropertyUnit.electricityPhase = detialss.FindAttribute("Name", "electricity_phase").GetAttributeValue<string>("Value", null);
                listingModel.PropertyUnit.floorLoading = detialss.FindAttribute("Name", "floor_loading").GetAttributeValue<string>("Value", null);

                if (detialss.FindAttribute("Name", "srx_tenanted").GetAttributeValue("Value", "") == "Yes")
                {
                    listingModel.PropertyUnit.tenancy = new Tenancy()
                    {
                        value = "TENANTED",
                        tenantedUntilDate = new { date = detialss.FindAttribute("Name", "tenanted_until").GetAttributeValue("Value", $"{DateTime.Now.ToString("yyyy-MM-dd")}") }
                    };
                }
                else
                {
                    listingModel.PropertyUnit.tenancy = new Tenancy()
                    {
                        value = "UNTENANTED",
                        tenantedUntilDate = null
                    };
                }

                listingModel.Price = new Price();
                listingModel.Price.value = detialss.FindAttribute("Name", "price").GetAttributeValue<int>("Value", 0);
                listingModel.Price.type = new Listing.Type()
                {
                    code = detialss.FindAttribute("Name", "price_type").GetAttributeValue("Value", "VTO")
                };

                listingModel.Sizes = new Sizes();
                listingModel.Sizes.bedrooms = new Bedrooms()
                {
                    value = detialss.FindAttribute("Name", "bedrooms").GetAttributeValue<int?>("Value", null)
                };
                listingModel.Sizes.bathrooms = new Bathrooms()
                {
                    value = detialss.FindAttribute("Name", "bathrooms").GetAttributeValue<int?>("Value", null)
                };
                if (listingModel.Sizes.bedrooms.value != null)
                {
                    if (listingModel.Sizes.bathrooms.value == null)
                    {
                        listingModel.Sizes.bathrooms.value = 0;
                    }
                }
                listingModel.Sizes.floorArea = new List<FloorAreaItem>()
                {
                    new FloorAreaItem(){
                        value=detialss.FindAttribute("Name", "floorarea").GetAttributeValue<int?>("Value", null)
                    }
                };
                var landarea = detialss.FindAttribute("Name", "landarea").GetAttributeValue<int?>("Value", null);
                if (landarea.HasValue)
                {
                    listingModel.Sizes.landArea = new List<LandAreaItem>()
                    {
                        new LandAreaItem(){
                            value=landarea
                        }
                    };
                }
                listingModel.Descriptions = new Descriptions()
                {
                    En = listingModel.LocalizedTitle
                };
                listingModel.LocalizedHeadline = detialss.FindAttribute("Name", "listing_title").GetAttributeValue("Value", DefaultTitles.GetTitle());
                if (listingModel.LocalizedHeadline != null && string.IsNullOrEmpty(listingModel.LocalizedHeadline))
                {
                    listingModel.LocalizedHeadline = DefaultTitles.GetTitle();
                }
                listingModel.Headlines = new Headlines()
                {
                    En = listingModel.LocalizedHeadline
                };
                listing.Listing = listingModel;
                Listings.Add(listing);
            }
        }
    }
}
