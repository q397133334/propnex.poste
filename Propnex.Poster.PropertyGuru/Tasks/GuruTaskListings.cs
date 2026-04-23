using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Listing.V3;
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

                var location = element.Element("Location");
                if (location != null)
                {
                    foreach (var item in location.Elements().ToList())
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


                listing.Photos = element.ElementString("Photos", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.PhotosTime = element.ElementString("PhotosTime", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.Videos = element.ElementString("Videos", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.Tours = element.ElementString("Tours", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.FloorPlan = element.ElementString("FloorPlan", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
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
                    //"2026-02-13T16:00:00.000Z"
                    try
                    {
                        listingModel.Dates.available.date = Convert.ToDateTime(available_date).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    catch { }
                    if (listingModel.Dates.available.date == "")
                    {
                        listingModel.Dates.available.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (Convert.ToDateTime(listingModel.Dates.available.date) < DateTime.Now)
                    {
                        listingModel.Dates.available.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); ;
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

                if (listingModel.Property.typeGroup == "L")
                {
                    listingModel.Location.maskLocation = true;
                }

                listingModel.PropertyUnit = new PropertyUnit();
                listingModel.PropertyUnit.floorLevelCode = detialss.FindAttribute("Name", "floor_level").GetAttributeValue("Value", "").ToUpper();
                listingModel.PropertyUnit.sellerEthnic = detialss.FindAttribute("Name", "sellerEthnic").GetAttributeValue("Value", "");
                listingModel.PropertyUnit.sellerResidency = detialss.FindAttribute("Name", "sellerResidency").GetAttributeValue("Value", "");
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
                if (string.IsNullOrEmpty(listingModel.PropertyUnit.ceilingHeight))
                {
                    listingModel.PropertyUnit.ceilingHeight = null;
                }
                listingModel.PropertyUnit.electricitySupply = detialss.FindAttribute("Name", "electricity_supply").GetAttributeValue<int?>("Value", null);
                listingModel.PropertyUnit.electricityPhase = detialss.FindAttribute("Name", "electricity_phase").GetAttributeValue<string>("Value", null);
                if (string.IsNullOrEmpty(listingModel.PropertyUnit.electricityPhase))
                {
                    listingModel.PropertyUnit.electricityPhase = null;
                }
                listingModel.PropertyUnit.floorLoading = detialss.FindAttribute("Name", "floor_loading").GetAttributeValue<string>("Value", null);
                if (string.IsNullOrEmpty(listingModel.PropertyUnit.floorLoading))
                {
                    listingModel.PropertyUnit.floorLoading = null;
                }


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
                listingModel.Price.type = new Listing.V2.Type()
                {
                    code = detialss.FindAttribute("Name", "price_type").GetAttributeValue("Value", "VTO")
                };

                listingModel.Sizes = new Sizes();
                listingModel.Sizes.bedrooms = new Bedrooms()
                {
                    value = detialss.FindAttribute("Name", "bedrooms").GetAttributeValue<int?>("Value", 0)
                };
                listingModel.Sizes.bathrooms = new Bathrooms()
                {
                    value = detialss.FindAttribute("Name", "bathrooms").GetAttributeValue<int?>("Value", 0)
                };
                if (listingModel.Sizes.bedrooms != null)
                {
                    if (listingModel.Sizes.bedrooms.value == null)
                    {
                        listingModel.Sizes.bedrooms.value = 0;
                    }
                }
                if (listingModel.Sizes.bathrooms != null)
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

                // 直接从 XML 构建 V3 格式，无需经过 V2 转换
                var v3TypeCode = detialss.FindAttribute("Name", "listing_type").GetAttributeValue("Value", "SALE");
                var v3Headline = detialss.FindAttribute("Name", "listing_title").GetAttributeValue("Value", DefaultTitles.GetTitle());
                if (string.IsNullOrEmpty(v3Headline)) v3Headline = DefaultTitles.GetTitle();
                var v3Description = detialss.FindAttribute("Name", "listing_description").GetAttributeValue("Value", "");
                if (v3Description.Length > 2000) v3Description = v3Description.Substring(0, 1999);
                var v3PostalCode = detialss.FindAttribute("Name", "postcode").GetAttributeValue("Value", "");
                var v3Floor = detialss.FindAttribute("Name", "property_level_number").GetAttributeValue("Value", "");
                var v3Unit = detialss.FindAttribute("Name", "property_unit_number").GetAttributeValue("Value", "");
                var v3TypeGroup = detialss.FindAttribute("Name", "property_type_group").GetAttributeValue("Value", "N");
                var v3Bedrooms = detialss.FindAttribute("Name", "bedrooms").GetAttributeValue<int?>("Value", null);
                var v3Bathrooms = detialss.FindAttribute("Name", "bathrooms").GetAttributeValue<int?>("Value", null);
                var v3FloorArea = detialss.FindAttribute("Name", "floorarea").GetAttributeValue<int?>("Value", null);
                var v3FloorLevel = detialss.FindAttribute("Name", "floor_level").GetAttributeValue("Value", "").ToUpper();
                var v3Furnishing = detialss.FindAttribute("Name", "furnishing").GetAttributeValue<string>("Value", null);
                var v3PgVerifiedId = detialss.FindAttribute("Name", "pg_verified_id").GetAttributeValue("Value", "");
                var v3LocationId = detialss.FindAttribute("Name", "location_id").GetAttributeValue<int?>("Value", null);
                var v3PropTypeCode = detialss.FindAttribute("Name", "property_type_code").GetAttributeValue("Value", "");
                var v3Price = detialss.FindAttribute("Name", "price").GetAttributeValue<int>("Value", 0);
                var v3Maintenance = detialss.FindAttribute("Name", "maintenance_fee").GetAttributeValue("Value", 0);
                var v3ListingId = detialss.FindAttribute("Name", "hidden_listing_id").GetAttributeValue<int?>("Value", null);

                var v3Features = new List<string>();
                foreach (var item in detialss)
                {
                    if (item.Attribute("Name").Value.Contains("unit_features"))
                    {
                        var code = item.Attribute("Name").Value.Replace("unit_features[],", "");
                        if (features.Any(q => q == code))
                            v3Features.Add(code);
                    }
                }

                bool v3IsAvailableNow = true;
                if (v3TypeCode?.ToUpper() == "RENT")
                {
                    var v3AvailDate = detialss.FindAttribute("Name", "available_date").GetAttributeValue("Value", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    try { v3IsAvailableNow = Convert.ToDateTime(v3AvailDate) <= DateTime.Now; }
                    catch { }
                }

                listing.ListingV3 = new CreateListingV3
                {
                    Id = v3ListingId,
                    ListingType = new ListingTypeV3 { Code = v3TypeCode },
                    Price = new PriceV3
                    {
                        Value = v3Price,
                        MaintenanceFee = v3Maintenance > 0 ? v3Maintenance : (int?)null
                    },
                    Location = new LocationV3
                    {
                        Address = new AddressV3
                        {
                            PostalCode = v3PostalCode,
                            Floor = string.IsNullOrEmpty(v3Floor) ? null : v3Floor,
                            Unit = string.IsNullOrEmpty(v3Unit) ? null : v3Unit,
                            MaskUnitNumber = v3TypeGroup == "L"
                        }
                    },
                    Headlines = new List<LocalizedTextV3>
                    {
                        new LocalizedTextV3 { Text = v3Headline, Locale = "en", Brand = "pg" }
                    },
                    Descriptions = new List<LocalizedTextV3>
                    {
                        new LocalizedTextV3 { Text = v3Description, Locale = "en", Brand = "pg" }
                    },
                    UnitDetails = new UnitDetailsV3
                    {
                        Configuration = new ConfigurationV3
                        {
                            Bedrooms = v3Bedrooms,
                            Bathrooms = v3Bathrooms
                        },
                        Dimensions = v3FloorArea.HasValue
                            ? new DimensionsV3
                            {
                                Floor = new FloorDimensionV3
                                {
                                    Size = new SizeV3 { Value = v3FloorArea, Uom = "sqft" }
                                }
                            }
                            : null,
                        TenantEligibility = false,
                        IsAvailableNow = v3IsAvailableNow,
                        FloorLevel = string.IsNullOrEmpty(v3FloorLevel) ? null : v3FloorLevel,
                        Furnishing = string.IsNullOrEmpty(v3Furnishing) ? null : v3Furnishing,
                        Features = v3Features.Count > 0 ? v3Features : null,
                        IsBumiLot = null
                    },
                    Project = new ProjectV3
                    {
                        Type = "verified",
                        MetaByType = new MetaByTypeV3
                        {
                            Verified = new VerifiedMetaV3
                            {
                                Id = string.IsNullOrEmpty(v3PgVerifiedId) ? null : v3PgVerifiedId,
                                LocationId = v3LocationId,
                                Property = new VerifiedPropertyV3 { SubType = v3PropTypeCode }
                            }
                        }
                    }
                };

                Listings.Add(listing);
            }
        }
    }
}
