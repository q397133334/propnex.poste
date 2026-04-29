using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Listing.V3;
using Propnex.Poster.PropertyGuru.Xml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Propnex.Poster.PropertyGuru.Tasks
{
    public class GuruTaskListings
    {
        public GuruTaskListings()
        {
        }

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
                        info.AppendFormat(" ", ss); //&#x{0:X};
                    else info.Append(cc);
                }

                try
                {
                    Document = XDocument.Parse(info.ToString());
                    Init();
                }
                catch (Exception)
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
            List<string> features = new List<string>()
            {
                "BAL","STOR","WICL","HLRM","ALCV","UTLY","BBQP","BQPV",
                "SECG","CCTV","ALRM","SMALM","CMALM","CRPK","COPK","EVPK","TWPK","CHPK","BIPK","GARG", 
                "AMTH", "BIRM", "BOAL", "HMLIB", "IDPL", "IFPL", "PRPL", "JCUZ", "SAUNA", "CITV", "GRNV", "LAKEV", "POOLV", "SEAV", 
                "COLO", "CORN", "ORIG", "PENT", "RENO", "TRGN", "TERC", "ARCON", "INPLA", "CHSGT", "FREXT", "EMEX", "PNTRY", "RCPAR",
                "BKGEN", "DPWR", "LDBAY", "HDFRD", "KTCHN", "FLRPT", "CHLPL", "BTRM", "VRM", "CTCAB", "CAB", "LUXLT", "CRWIN", "MTRM", 
                "AVEQP", "VCONF", "BRCON", "ITSPT", "SECSR", "TNSYS", "SCSYS", "CONCR", "STOVE", "BLATD", "LOCKR", "COMAR"
            };
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



                var listing = ParseListingData(element, projectDatas, detialss, features);


                //listing - projectData
                ListingModel listingModel = new ListingModel();
                listing.Listing = listingModel;
                CreateListingModel();
                CreateListingV3();

                void CreateListingModel()
                {
                    listingModel.Title = projectDatas.FindAttribute("Name", "name").GetAttributeValue("Value", "");
                    listingModel.LocalizedDescription = detialss.FindAttribute("Name", "listing_description")
                        .GetAttributeValue("Value", ""); //.Replace("\n", "").Replace("\r", "");
                    listingModel.TypeCode = detialss.FindAttribute("Name", "listing_type")
                        .GetAttributeValue("Value", "SALE");
                    if (listingModel.TypeCode == "RENT")
                    {
                        listingModel.Dates.available = new Dates_Available();
                        var available_date = detialss.FindAttribute("Name", "available_date")
                            .GetAttributeValue("Value", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        //"2026-02-13T16:00:00.000Z"
                        try
                        {
                            listingModel.Dates.available.date =
                                Convert.ToDateTime(available_date).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        catch
                        {
                        }

                        if (listingModel.Dates.available.date == "")
                        {
                            listingModel.Dates.available.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }

                        if (Convert.ToDateTime(listingModel.Dates.available.date) < DateTime.Now)
                        {
                            listingModel.Dates.available.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            ;
                        }
                    }


                    listingModel.Location = new Location()
                    {
                        id = detialss.FindAttribute("Name", "location_id")
                            .GetAttributeValue<int?>("Value", null), //projectData.ElementString("locationId"),
                        longitude = detialss.FindAttribute("Name", "longitude")
                            .GetAttributeValue("Value", 0.00), //projectData.ElementDouble("longitude"),
                        latitude = detialss.FindAttribute("Name", "latitude")
                            .GetAttributeValue("Value", 0.00), //projectData.ElementDouble("latitude"),
                        postalCode =
                            detialss.FindAttribute("Name", "postcode")
                                .GetAttributeValue("Value", ""), //projectData.ElementString("postcode"),
                        regionCode = projectDatas.FindAttribute("Name", "regionCode")
                            .GetAttributeValue("Value", ""), //projectData.ElementString("regionCode"),
                        districtCode =
                            detialss.FindAttribute("Name", "district")
                                .GetAttributeValue("Value", ""), //projectData.ElementString("districtCode")
                        unit =
                            $"{detialss.FindAttribute("Name", "property_level_number").GetAttributeValue("Value", "01")}-{detialss.FindAttribute("Name", "property_unit_number").GetAttributeValue("Value", "01")}",
                        streetName1 = detialss.FindAttribute("Name", "streetname").GetAttributeValue("Value", ""),
                        hdbEstateCode = detialss.FindAttribute("Name", "hdb_estate").GetAttributeValue("Value", "")
                    };

                    listingModel.Property = new Property()
                    {
                        id = detialss.FindAttribute("Name", "property_id").GetAttributeValue<int?>("Value", null),
                        name = detialss.FindAttribute("Name", "property_name").GetAttributeValue<string>("Value"),
                        typeCode = detialss.FindAttribute("Name", "property_type_code").GetAttributeValue("Value", ""),
                        typeGroup = detialss.FindAttribute("Name", "property_type_group")
                            .GetAttributeValue("Value", "N"),
                        floors = projectDatas.FindAttribute("Name", "floors").GetAttributeValue("Value", 0),
                        tenureCode = detialss.FindAttribute("Name", "tenure").GetAttributeValue("Value", ""),
                    };

                    if (listingModel.Property.typeGroup == "L")
                    {
                        listingModel.Location.maskLocation = true;
                    }

                    listingModel.PropertyUnit = new PropertyUnit();
                    listingModel.PropertyUnit.floorLevelCode = detialss.FindAttribute("Name", "floor_level")
                        .GetAttributeValue("Value", "").ToUpper();
                    listingModel.PropertyUnit.sellerEthnic =
                        detialss.FindAttribute("Name", "sellerEthnic").GetAttributeValue("Value", "");
                    listingModel.PropertyUnit.sellerResidency = detialss.FindAttribute("Name", "sellerResidency")
                        .GetAttributeValue("Value", "");

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

                    listingModel.PropertyUnit.furnishingCode = detialss.FindAttribute("Name", "furnishing")
                        .GetAttributeValue<string>("Value", null);
                    if (listingModel.Property.typeCode == "CLAND")
                    {
                        listingModel.PropertyUnit.furnishingCode = "";
                    }

                    listingModel.PropertyUnit.hdbTypeCode = detialss.FindAttribute("Name", "hdb_type")
                        .GetAttributeValue<string>("Value", null);
                    listingModel.PropertyUnit.ceilingHeight = detialss.FindAttribute("Name", "ceiling_height")
                        .GetAttributeValue<string>("Value", null);
                    if (string.IsNullOrEmpty(listingModel.PropertyUnit.ceilingHeight))
                    {
                        listingModel.PropertyUnit.ceilingHeight = null;
                    }

                    listingModel.PropertyUnit.electricitySupply = detialss.FindAttribute("Name", "electricity_supply")
                        .GetAttributeValue<int?>("Value", null);
                    listingModel.PropertyUnit.electricityPhase = detialss.FindAttribute("Name", "electricity_phase")
                        .GetAttributeValue<string>("Value", null);
                    if (string.IsNullOrEmpty(listingModel.PropertyUnit.electricityPhase))
                    {
                        listingModel.PropertyUnit.electricityPhase = null;
                    }

                    listingModel.PropertyUnit.floorLoading = detialss.FindAttribute("Name", "floor_loading")
                        .GetAttributeValue<string>("Value", null);
                    if (string.IsNullOrEmpty(listingModel.PropertyUnit.floorLoading))
                    {
                        listingModel.PropertyUnit.floorLoading = null;
                    }


                    if (detialss.FindAttribute("Name", "srx_tenanted").GetAttributeValue("Value", "") == "Yes")
                    {
                        listingModel.PropertyUnit.tenancy = new Tenancy()
                        {
                            value = "TENANTED",
                            tenantedUntilDate = new
                            {
                                date = detialss.FindAttribute("Name", "tenanted_until")
                                    .GetAttributeValue("Value", $"{DateTime.Now.ToString("yyyy-MM-dd")}")
                            }
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
                    listingModel.Price.value =
                        detialss.FindAttribute("Name", "price").GetAttributeValue<int>("Value", 0);
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
                        new FloorAreaItem()
                        {
                            value = detialss.FindAttribute("Name", "floorarea").GetAttributeValue<int?>("Value", null)
                        }
                    };
                    var landarea = detialss.FindAttribute("Name", "landarea").GetAttributeValue<int?>("Value", null);
                    if (landarea.HasValue)
                    {
                        listingModel.Sizes.landArea = new List<LandAreaItem>()
                        {
                            new LandAreaItem()
                            {
                                value = landarea
                            }
                        };
                    }

                    listingModel.Descriptions = new Descriptions()
                    {
                        En = listingModel.LocalizedTitle
                    };
                    listingModel.LocalizedHeadline = detialss.FindAttribute("Name", "listing_title")
                        .GetAttributeValue("Value", DefaultTitles.GetTitle());
                    if (listingModel.LocalizedHeadline != null && string.IsNullOrEmpty(listingModel.LocalizedHeadline))
                    {
                        listingModel.LocalizedHeadline = DefaultTitles.GetTitle();
                    }

                    listingModel.Headlines = new Headlines()
                    {
                        En = listingModel.LocalizedHeadline
                    };
                }


                void CreateListingV3()
                {
                    var listingV3 = new CreateListingV3();
                    listingV3.ListingType = new ListingTypeV3()
                    {
                        Code = listing.Details.ListingType
                    };
                    listingV3.UnitDetails.RentalType = "ENT";
                    if (listing.Details.ListingType == "ROOM")
                    {
                        listingV3.ListingType.Code = "RENT";
                        listingV3.UnitDetails.RentalType = "ROOM";
                    }
                    listingV3.UnitDetails.IsAvailableNow = true;
                    try
                    {
                        listingV3.UnitDetails.IsAvailableNow = Convert.ToDateTime(listing.Details.AvailableDate) <= DateTime.Now;
                        if (listingV3.UnitDetails.IsAvailableNow == false)
                        {
                            listingV3.Dates.Available = Convert.ToDateTime(listing.Details.AvailableDate).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
                        }
                    }
                    catch { }

                    listingV3.Descriptions.Add(new LocalizedTextV3()
                    {
                        Brand = "pg",
                        Locale = "en",
                        Text = listing.Details.ListingDescription
                    });
                    listingV3.Headlines.Add(new LocalizedTextV3()
                    {
                        Brand = "pg",
                        Locale = "en",
                        Text = listing.Details.ListingTitle
                    });

                    listingV3.Lease = new LeaseV3()
                    {
                        Code = listing.Details.LeaseTerm,
                        Remaining = null
                    };

                    listingV3.Location = new LocationV3()
                    {
                        Address = new AddressV3()
                        {
                            Floor = "1",
                            MaskUnitNumber = false,
                            PostalCode = listing.Details.PostalCode,
                            Unit = listing.Details.UnitNumber
                        }
                    };

                    listingV3.Price = new PriceV3()
                    {
                        Value = listing.Details.Price,
                        MaintenanceFee = listing.Details.MaintenanceFee,
                    };

                    listingV3.Project = new ProjectV3()
                    {
                        MetaByType = new MetaByTypeV3()
                        {
                            Verified = new VerifiedMetaV3()
                            {
                                Id = listing.Details.PgVerifiedId,
                                LocationId = listing.Details.LocationId,
                                Property = new VerifiedPropertyV3()
                                {
                                    SubType = null
                                }
                            }
                        }
                    };

                    listingV3.UnitDetails.Condition = listing.Details.Condition;

                    listingV3.UnitDetails.Configuration = new ConfigurationV3()
                    {
                        Bedrooms = listing.Details.Bedrooms,
                        Bathrooms = listing.Details.Bathrooms,
                        extrarooms = null,
                    };

                    listingV3.UnitDetails.Dimensions = new DimensionsV3()
                    {
                        Floor = new FloorDimensionV3()
                        {
                            Size = new SizeV3()
                            {
                                Value = listing.Details.FloorArea,
                                Uom = "sqft"
                            }
                        },
                        land = new FloorDimensionV3(),
                        room = new FloorDimensionV3()
                    };

                    listingV3.UnitDetails.Electricity = new ElectricityV3()
                    {
                        Phase = new ElectricityPhaseV3() { Code = listing.Details.ElectricityPhase }
                    };

                    listingV3.UnitDetails.Features = listing.Details.UnitFeatures;

                    listingV3.UnitDetails.FloorLevel = listing.Details.FloorLevel;

                    listingV3.UnitDetails.FloorLoadingCapacity = null;

                    listingV3.UnitDetails.FloorLoadingCategory = listing.Details.FloorLoadingCategory;

                    listingV3.UnitDetails.Furnishing = listing.Details.Furnishing;

                    listingV3.UnitDetails.HdbTypeCode = listing.Details.HdbType;

                    listingV3.UnitDetails.IsBumiLot = false;

                    listingV3.UnitDetails.IsHighCeiling = null;

                    listingV3.UnitDetails.LandTitleType = null;

                    listingV3.UnitDetails.Lift = new LiftV3()
                    {
                        Cargo = listing.Details.LiftCargo,
                        TotalPassenger = listing.Details.LiftPassenger,
                        Capacity = null
                    };

                    listingV3.UnitDetails.MaxTenants = null;
                    listingV3.UnitDetails.OwnerStays = null;
                    listingV3.UnitDetails.ParkingSpots = null;
                    listingV3.UnitDetails.PetFriendly = null;
                    listingV3.UnitDetails.PropertyUses = new List<string>() { listing.Details.PropertyUse };
                    listingV3.UnitDetails.QuotaEthnic = null;
                    listingV3.UnitDetails.QuotaSpr = null;
                    listingV3.UnitDetails.Ramp = null;
                    listingV3.UnitDetails.SellerEthnic = listing.Details.SellerEthnic;
                    listingV3.UnitDetails.SellerResidency = listing.Details.SellerResidency;
                    listingV3.UnitDetails.TenantEligibility = false;

                    listing.ListingV3 = listingV3;
                }
                Listings.Add(listing);
            }
        }

        /// <summary>
        /// 将 &lt;ProjectData&gt; 和 &lt;Details&gt; 的 Field 元素解析为强类型 GuruListingData 模型
        /// </summary>
        private static GuruTaskListing ParseListingData(
            XElement element,
            IEnumerable<XElement> projectDatas,
            List<XElement> detialss,
            List<string> features)
        {
            var d = new GuruTaskListing();

            // ── 顶层 Listing 字段 ──────────────────────────────────────
            d.ListingId = element.ElementInt("ID");
            d.XID = element.ElementString("XID");
            d.ListingName = element.ElementString("ListingName");
            d.ListingTypeRaw = element.ElementString("ListingType");
            d.PropertyType = element.ElementString("PropertyType");

            d.NoGuruPhotos = element.ElementBool("NoGuruPhotos");
            d.NoiPropertyPhotos = element.ElementBool("NoiPropertyPhotos");
            d.NostPropertyPhotos = element.ElementBool("NostPropertyPhotos");
            d.UseFileName = element.ElementBool("UseFileName");
            d.Id = element.ElementInt("ID");
            d.XID = element.ElementString("XID");
            d.UpdateTime = detialss.FindAttribute("Name", "UpdateTime").GetAttributeValue("Value", "");
            d.Photos = element.ElementString("Photos", "")
                .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
            d.PhotosTime = element.ElementString("PhotosTime", "")
                .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
            d.Videos = element.ElementString("Videos", "")
                .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
            d.Tours = element.ElementString("Tours", "")
                .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
            d.FloorPlan = element.ElementString("FloorPlan", "")
                .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
            d.FastRepost = detialss.FindAttribute("Name", "FastRepost").GetAttributeValue("Value", "0");
            d.TaskItemId = detialss.FindAttribute("Name", "taskitem_id").GetAttributeValue("Value", "0");
            d.Listing.Agent = new Agent()
            {
                id = 0,
                alternativeAgent = detialss.FindAttribute("Name", "alternative_agent")
                    .GetAttributeValue("Value", ""),
                alternativeEmail = detialss.FindAttribute("Name", "alternative_email")
                    .GetAttributeValue("Value", ""),
                alternativeMobile = detialss.FindAttribute("Name", "alternative_mobile")
                    .GetAttributeValue("Value", ""),
                alternativePhone = detialss.FindAttribute("Name", "alternative_phone")
                    .GetAttributeValue("Value", "")
            };
            d.Listing.Id = detialss.FindAttribute("Name", "hidden_listing_id")
                .GetAttributeValue<int?>("Value", null);
            d.Listing.LeaseTermCode =
                detialss.FindAttribute("Name", "lease_term").GetAttributeValue("Value", "");

            // ── ProjectData 子模型 ─────────────────────────────────────
            d.ProjectData = new GuruProjectData
            {
                Id = projectDatas.FindAttribute("Name", "id").GetAttributeValue<int?>("Value", null),
                TypeCode = projectDatas.FindAttribute("Name", "typeCode").GetAttributeValue("Value", ""),
                PropertyTypeGroup = projectDatas.FindAttribute("Name", "propertyTypeGroup").GetAttributeValue("Value", ""),
                ProjectName = projectDatas.FindAttribute("Name", "name").GetAttributeValue("Value", ""),
                NewLaunch = projectDatas.FindAttribute("Name", "newLaunch").GetAttributeValue("Value", ""),
                TotalUnits = projectDatas.FindAttribute("Name", "totalUnits").GetAttributeValue("Value", ""),
                TopYear = projectDatas.FindAttribute("Name", "topYear").GetAttributeValue("Value", ""),
                Tenure = projectDatas.FindAttribute("Name", "tenure").GetAttributeValue("Value", ""),
                RegionCode = projectDatas.FindAttribute("Name", "regionCode").GetAttributeValue("Value", ""),
                Developer = projectDatas.FindAttribute("Name", "developer").GetAttributeValue("Value", ""),
                DistrictCode = projectDatas.FindAttribute("Name", "districtCode").GetAttributeValue("Value", ""),
                PostCode = projectDatas.FindAttribute("Name", "postcode").GetAttributeValue("Value", ""),
                StreetName = projectDatas.FindAttribute("Name", "streetname").GetAttributeValue("Value", ""),
                StreetNumber = projectDatas.FindAttribute("Name", "streetnumber").GetAttributeValue("Value", ""),
                Longitude = projectDatas.FindAttribute("Name", "longitude").GetAttributeValue("Value", 0.00),
                Latitude = projectDatas.FindAttribute("Name", "latitude").GetAttributeValue("Value", 0.00),
                StatusCode = projectDatas.FindAttribute("Name", "statusCode").GetAttributeValue("Value", ""),
                EstateCode = projectDatas.FindAttribute("Name", "estateCode").GetAttributeValue("Value", ""),
                Url = projectDatas.FindAttribute("Name", "url").GetAttributeValue("Value", ""),
                PropertyId = projectDatas.FindAttribute("Name", "propertyId").GetAttributeValue<int?>("Value", null),
                PropertyName = projectDatas.FindAttribute("Name", "propertyName").GetAttributeValue("Value", ""),
                ProjectNameAlt = projectDatas.FindAttribute("Name", "projectName").GetAttributeValue("Value", ""),
                PropertyType = projectDatas.FindAttribute("Name", "propertyType").GetAttributeValue("Value", ""),
                ProjectFloors = projectDatas.FindAttribute("Name", "floors").GetAttributeValue("Value", 0)
            };

            // ── Details 子模型 ─────────────────────────────────────────
            var det = new GuruDetailsData();
            d.Details = det;

            // 物业基本信息
            det.PropertyName = detialss.FindAttribute("Name", "property_name").GetAttributeValue("Value", "");
            det.PropertyId = detialss.FindAttribute("Name", "property_id").GetAttributeValue<int?>("Value", null);
            det.LocationId = detialss.FindAttribute("Name", "location_id").GetAttributeValue<int?>("Value", null);
            det.PgVerifiedId = detialss.FindAttribute("Name", "pg_verified_id").GetAttributeValue("Value", "");
            det.PropertyTypeGroup = detialss.FindAttribute("Name", "property_type_group").GetAttributeValue("Value", "N");
            det.PropertyTypeCode = detialss.FindAttribute("Name", "property_type_code").GetAttributeValue("Value", "");
            det.HdbType = detialss.FindAttribute("Name", "hdb_type").GetAttributeValue<string>("Value", null);
            det.HdbEstate = detialss.FindAttribute("Name", "hdb_estate").GetAttributeValue("Value", "");
            det.District = detialss.FindAttribute("Name", "district").GetAttributeValue("Value", "");
            det.Tenure = detialss.FindAttribute("Name", "tenure").GetAttributeValue("Value", "");

            // 挂牌信息
            det.ListingType = detialss.FindAttribute("Name", "listing_type").GetAttributeValue("Value", "SALE");
            det.ListingTitle = detialss.FindAttribute("Name", "listing_title").GetAttributeValue("Value", DefaultTitles.GetTitle());
            if (string.IsNullOrEmpty(det.ListingTitle)) det.ListingTitle = DefaultTitles.GetTitle();
            det.ListingDescription = detialss.FindAttribute("Name", "listing_description").GetAttributeValue("Value", "");
            if (det.ListingDescription.Length > 2000) det.ListingDescription = det.ListingDescription.Substring(0, 1999);
            det.LeaseTerm = detialss.FindAttribute("Name", "lease_term").GetAttributeValue("Value", "");
            det.AvailableDate = detialss.FindAttribute("Name", "available_date").GetAttributeValue("Value", "");

            // 价格
            det.Price = detialss.FindAttribute("Name", "price").GetAttributeValue<int>("Value", 0);
            det.PriceType = detialss.FindAttribute("Name", "price_type").GetAttributeValue("Value", "VTO");
            det.MaintenanceFee = detialss.FindAttribute("Name", "tep_maintenance_fee").GetAttributeValue("Value", 0);

            // 面积
            det.FloorArea = detialss.FindAttribute("Name", "floorarea").GetAttributeValue<int?>("Value", null);
            det.LandArea = detialss.FindAttribute("Name", "landarea").GetAttributeValue<int?>("Value", null);

            // 房间配置
            det.Bedrooms = detialss.FindAttribute("Name", "bedrooms").GetAttributeValue<int?>("Value", null);
            det.Bathrooms = detialss.FindAttribute("Name", "bathrooms").GetAttributeValue<int?>("Value", null);
            det.RoomType = detialss.FindAttribute("Name", "room_type").GetAttributeValue("Value", "");

            // 地址
            det.PostalCode = detialss.FindAttribute("Name", "postcode").GetAttributeValue("Value", "");
            det.StreetName = detialss.FindAttribute("Name", "streetname").GetAttributeValue("Value", "");
            det.StreetNumber = detialss.FindAttribute("Name", "streetnumber").GetAttributeValue("Value", "");
            det.Longitude = detialss.FindAttribute("Name", "longitude").GetAttributeValue("Value", 0.00);
            det.Latitude = detialss.FindAttribute("Name", "latitude").GetAttributeValue("Value", 0.00);
            det.FloorNumber = detialss.FindAttribute("Name", "property_level_number").GetAttributeValue("Value", "");
            det.UnitNumber = detialss.FindAttribute("Name", "property_unit_number").GetAttributeValue("Value", "");

            // 单元属性
            det.FloorLevel = detialss.FindAttribute("Name", "floor_level").GetAttributeValue("Value", "").ToUpper();
            det.Furnishing = detialss.FindAttribute("Name", "furnishing").GetAttributeValue<string>("Value", null);
            det.CeilingHeight = detialss.FindAttribute("Name", "ceiling_height").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.CeilingHeight)) det.CeilingHeight = null;
            det.SrxTenanted = detialss.FindAttribute("Name", "srx_tenanted").GetAttributeValue("Value", "No");
            det.TenantedUntil = detialss.FindAttribute("Name", "tenanted_until").GetAttributeValue("Value", "");
            det.SellerEthnic = detialss.FindAttribute("Name", "sellerEthnic").GetAttributeValue("Value", "");
            det.SellerResidency = detialss.FindAttribute("Name", "sellerResidency").GetAttributeValue("Value", "");

            foreach (var item in detialss)
            {
                if (item.Attribute("Name").Value.Contains("unit_features[],"))
                {
                    var code = item.Attribute("Name").Value.Replace("unit_features[],", "");
                    if (features.Any(q => q == code))
                        det.UnitFeatures.Add(code);
                }
            }

            // 工业 / 商业专用
            det.ElectricitySupply = detialss.FindAttribute("Name", "electricity_supply").GetAttributeValue<int?>("Value", null);
            det.ElectricityPhase = detialss.FindAttribute("Name", "electricity_phase").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.ElectricityPhase)) det.ElectricityPhase = null;
            det.FloorLoading = detialss.FindAttribute("Name", "floor_loading").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.FloorLoading)) det.FloorLoading = null;
            det.FloorLoadingCategory = detialss.FindAttribute("Name", "floor_loading_category").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.FloorLoadingCategory)) det.FloorLoadingCategory = null;
            det.IsHighCeiling = detialss.FindAttribute("Name", "is_high_ceiling").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.IsHighCeiling)) det.IsHighCeiling = null;
            det.LiftCargo = detialss.FindAttribute("Name", "lift_cargo").GetAttributeValue<int?>("Value", null);
            det.LiftPassenger = detialss.FindAttribute("Name", "lift_passenger").GetAttributeValue<int?>("Value", null);
            det.Ramp = detialss.FindAttribute("Name", "ramp").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.Ramp)) det.Ramp = null;
            det.Condition = detialss.FindAttribute("Name", "condition").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.Condition)) det.Condition = null;
            det.PropertyUse = detialss.FindAttribute("Name", "property_use").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.PropertyUse)) det.PropertyUse = null;
            det.CookingType = detialss.FindAttribute("Name", "cooking_type").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.CookingType)) det.CookingType = null;

            // 经纪人信息
            det.AlternativeAgent = detialss.FindAttribute("Name", "alternative_agent").GetAttributeValue("Value", "");
            det.AlternativeMobile = detialss.FindAttribute("Name", "alternative_mobile").GetAttributeValue("Value", "");
            det.AlternativePhone = detialss.FindAttribute("Name", "alternative_phone").GetAttributeValue("Value", "");
            det.AlternativeEmail = detialss.FindAttribute("Name", "alternative_email").GetAttributeValue("Value", "");

            // 任务信息
            det.HiddenListingId = detialss.FindAttribute("Name", "hidden_listing_id").GetAttributeValue<int?>("Value", null);
            det.TaskItemId = detialss.FindAttribute("Name", "taskitem_id").GetAttributeValue("Value", "0");
            det.UpdateTime = detialss.FindAttribute("Name", "UpdateTime").GetAttributeValue("Value", "");
            det.FastRepost = detialss.FindAttribute("Name", "FastRepost").GetAttributeValue("Value", "0");

            return d;
        }
    }
}