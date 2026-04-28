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
                listing.Photos = element.ElementString("Photos", "")
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                    .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.PhotosTime = element.ElementString("PhotosTime", "")
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                    .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.Videos = element.ElementString("Videos", "")
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                    .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.Tours = element.ElementString("Tours", "")
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                    .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.FloorPlan = element.ElementString("FloorPlan", "")
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()
                    .Where(q => q.Trim() != "").Where(q => q.Length > 10).ToList();
                listing.FastRepost = detialss.FindAttribute("Name", "FastRepost").GetAttributeValue("Value", "0");
                listing.TaskItemId = detialss.FindAttribute("Name", "taskitem_id").GetAttributeValue("Value", "0");
                listing.Listing.Agent = new Agent()
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
                listing.Listing.Id = detialss.FindAttribute("Name", "hidden_listing_id")
                    .GetAttributeValue<int?>("Value", null);
                listing.Listing.LeaseTermCode =
                    detialss.FindAttribute("Name", "lease_term").GetAttributeValue("Value", "");

                //listing - projectData
                ListingModel listingModel = new ListingModel();
                CreateListingModel();
                listing.Listing = listingModel;

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

                // 直接从 XML 构建 V3 格式，无需经过 V2 转换
                var v3TypeCode = detialss.FindAttribute("Name", "listing_type").GetAttributeValue("Value", "SALE");
                var v3Headline = detialss.FindAttribute("Name", "listing_title")
                    .GetAttributeValue("Value", DefaultTitles.GetTitle());
                if (string.IsNullOrEmpty(v3Headline)) v3Headline = DefaultTitles.GetTitle();
                var v3Description = detialss.FindAttribute("Name", "listing_description")
                    .GetAttributeValue("Value", "");
                if (v3Description.Length > 2000) v3Description = v3Description.Substring(0, 1999);
                var v3PostalCode = detialss.FindAttribute("Name", "postcode").GetAttributeValue("Value", "");
                var v3Floor = detialss.FindAttribute("Name", "property_level_number").GetAttributeValue("Value", "");
                var v3Unit = detialss.FindAttribute("Name", "property_unit_number").GetAttributeValue("Value", "");
                var v3TypeGroup = detialss.FindAttribute("Name", "property_type_group").GetAttributeValue("Value", "N");
                var v3Bedrooms = detialss.FindAttribute("Name", "bedrooms").GetAttributeValue<int?>("Value", null);
                var v3Bathrooms = detialss.FindAttribute("Name", "bathrooms").GetAttributeValue<int?>("Value", null);
                var v3FloorArea = detialss.FindAttribute("Name", "floorarea").GetAttributeValue<int?>("Value", null);
                var v3FloorLevel = detialss.FindAttribute("Name", "floor_level").GetAttributeValue("Value", "")
                    .ToUpper();
                var v3Furnishing = detialss.FindAttribute("Name", "furnishing")
                    .GetAttributeValue<string>("Value", null);
                var v3PgVerifiedId = detialss.FindAttribute("Name", "pg_verified_id").GetAttributeValue("Value", "");
                var v3LocationId = detialss.FindAttribute("Name", "location_id").GetAttributeValue<int?>("Value", null);
                var v3PropTypeCode =
                    detialss.FindAttribute("Name", "property_type_code").GetAttributeValue("Value", "");
                var v3Price = detialss.FindAttribute("Name", "price").GetAttributeValue<int>("Value", 0);
                var v3Maintenance = detialss.FindAttribute("Name", "tep_maintenance_fee").GetAttributeValue("Value", 0);
                var v3ListingId = detialss.FindAttribute("Name", "hidden_listing_id")
                    .GetAttributeValue<int?>("Value", null);
                var v3RoomType = detialss.FindAttribute("Name", "room_type").GetAttributeValue("Value", "");
                var v3HdbTypeCode = detialss.FindAttribute("Name", "hdb_type").GetAttributeValue<string>("Value", null);
                var v3SellerEthnic = detialss.FindAttribute("Name", "sellerEthnic").GetAttributeValue("Value", "");
                var v3SellerResidency = detialss.FindAttribute("Name", "sellerResidency").GetAttributeValue("Value", "");
                var v3LeaseTerm = detialss.FindAttribute("Name", "lease_term").GetAttributeValue("Value", "");
                var v3Tenanted = detialss.FindAttribute("Name", "srx_tenanted").GetAttributeValue("Value", "No");
                var v3TenantedUntil = detialss.FindAttribute("Name", "tenanted_until").GetAttributeValue("Value", "");
                var v3ElecPhase = detialss.FindAttribute("Name", "electricity_phase").GetAttributeValue<string>("Value", null);
                var v3ElecSupply = detialss.FindAttribute("Name", "electricity_supply").GetAttributeValue<int?>("Value", null);
                var v3FloorLoadingCat = detialss.FindAttribute("Name", "floor_loading_category").GetAttributeValue<string>("Value", null);
                if (string.IsNullOrEmpty(v3FloorLoadingCat)) v3FloorLoadingCat = null;
                var v3IsHighCeilingStr = detialss.FindAttribute("Name", "is_high_ceiling").GetAttributeValue<string>("Value", null);
                bool? v3IsHighCeiling = string.IsNullOrEmpty(v3IsHighCeilingStr) ? (bool?)null
                    : v3IsHighCeilingStr.Equals("true", StringComparison.OrdinalIgnoreCase) || v3IsHighCeilingStr == "1" || v3IsHighCeilingStr.Equals("yes", StringComparison.OrdinalIgnoreCase);
                var v3LiftCargo = detialss.FindAttribute("Name", "lift_cargo").GetAttributeValue<int?>("Value", null);
                var v3LiftPassenger = detialss.FindAttribute("Name", "lift_passenger").GetAttributeValue<int?>("Value", null);
                var v3RampStr = detialss.FindAttribute("Name", "ramp").GetAttributeValue<string>("Value", null);
                bool? v3Ramp = string.IsNullOrEmpty(v3RampStr) ? (bool?)null
                    : v3RampStr.Equals("true", StringComparison.OrdinalIgnoreCase) || v3RampStr == "1" || v3RampStr.Equals("yes", StringComparison.OrdinalIgnoreCase);
                var v3Condition = detialss.FindAttribute("Name", "condition").GetAttributeValue<string>("Value", null);
                if (string.IsNullOrEmpty(v3Condition)) v3Condition = null;
                var v3PropertyUseStr = detialss.FindAttribute("Name", "property_use").GetAttributeValue<string>("Value", null);
                var v3PropertyUses = string.IsNullOrEmpty(v3PropertyUseStr)
                    ? null
                    : new List<string> { v3PropertyUseStr };
                var v3CookingType = detialss.FindAttribute("Name", "cooking_type").GetAttributeValue<string>("Value", null);
                if (string.IsNullOrEmpty(v3CookingType)) v3CookingType = null;

                var v3Features = new List<string>();
                foreach (var item in detialss)
                {
                    if (item.Attribute("Name").Value.Contains("unit_features[],"))
                    {
                        var code = item.Attribute("Name").Value.Replace("unit_features[],", "");
                        if (features.Any(q => q == code))
                            v3Features.Add(code);
                    }
                }

                bool v3IsAvailableNow = true;
                var v3rentalType = "ENT";
                if (v3TypeCode == "ROOM")
                {
                    v3TypeCode = "RENT";
                    v3rentalType = "ROOM";
                }
                if (v3TypeCode?.ToUpper() == "RENT")
                {
                    var v3AvailDate = detialss.FindAttribute("Name", "available_date")
                        .GetAttributeValue("Value", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
                        IsBumiLot = null,
                        RentalType = v3rentalType,
                        RoomType = string.IsNullOrEmpty(v3RoomType) ? null : v3RoomType,
                        HdbTypeCode = string.IsNullOrEmpty(v3HdbTypeCode) ? null : v3HdbTypeCode,
                        SellerEthnic = string.IsNullOrEmpty(v3SellerEthnic) ? null : v3SellerEthnic,
                        SellerResidency = string.IsNullOrEmpty(v3SellerResidency) ? null : v3SellerResidency,
                        CookingType = v3CookingType,
                        Condition = v3Condition,
                        PropertyUses = v3PropertyUses,
                        IsHighCeiling = v3IsHighCeiling,
                        Ramp = v3Ramp,
                        FloorLoadingCategory = v3FloorLoadingCat,
                        Electricity = (string.IsNullOrEmpty(v3ElecPhase) && !v3ElecSupply.HasValue) ? null
                            : new ElectricityV3
                            {
                                Phase = string.IsNullOrEmpty(v3ElecPhase) ? null : new ElectricityPhaseV3 { Code = v3ElecPhase },
                                Supply = v3ElecSupply
                            },
                        Lift = (v3LiftCargo.HasValue || v3LiftPassenger.HasValue) ? new LiftV3
                        {
                            Cargo = v3LiftCargo,
                            TotalPassenger = v3LiftPassenger
                        } : null,
                        Tenancy = v3Tenanted.Equals("Yes", StringComparison.OrdinalIgnoreCase)
                            ? new TenancyV3
                            {
                                Value = "TENANTED",
                                TenantedUntilDate = string.IsNullOrEmpty(v3TenantedUntil) ? null
                                    : new { date = v3TenantedUntil }
                            }
                            : new TenancyV3 { Value = "UNTENANTED" }
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
                    },
                    Lease = string.IsNullOrEmpty(v3LeaseTerm) ? null : new LeaseV3 { Code = v3LeaseTerm }
                };

                listing.Data = ParseListingData(element, projectDatas, detialss, features);
                Listings.Add(listing);
            }
        }

        /// <summary>
        /// 将 &lt;ProjectData&gt; 和 &lt;Details&gt; 的 Field 元素解析为强类型 GuruListingData 模型
        /// </summary>
        private static GuruListingData ParseListingData(
            XElement element,
            IEnumerable<XElement> projectDatas,
            List<XElement> detialss,
            List<string> features)
        {
            var d = new GuruListingData();

            // ── 顶层 Listing 字段 ──────────────────────────────────────
            d.ListingId      = element.ElementInt("ID");
            d.XID            = element.ElementString("XID");
            d.ListingName    = element.ElementString("ListingName");
            d.ListingTypeRaw = element.ElementString("ListingType");
            d.PropertyType   = element.ElementString("PropertyType");

            // ── ProjectData 子模型 ─────────────────────────────────────
            d.ProjectData = new GuruProjectData
            {
                Id              = projectDatas.FindAttribute("Name", "id").GetAttributeValue<int?>("Value", null),
                TypeCode        = projectDatas.FindAttribute("Name", "typeCode").GetAttributeValue("Value", ""),
                PropertyTypeGroup = projectDatas.FindAttribute("Name", "propertyTypeGroup").GetAttributeValue("Value", ""),
                ProjectName     = projectDatas.FindAttribute("Name", "name").GetAttributeValue("Value", ""),
                NewLaunch       = projectDatas.FindAttribute("Name", "newLaunch").GetAttributeValue("Value", ""),
                TotalUnits      = projectDatas.FindAttribute("Name", "totalUnits").GetAttributeValue("Value", ""),
                TopYear         = projectDatas.FindAttribute("Name", "topYear").GetAttributeValue("Value", ""),
                Tenure          = projectDatas.FindAttribute("Name", "tenure").GetAttributeValue("Value", ""),
                RegionCode      = projectDatas.FindAttribute("Name", "regionCode").GetAttributeValue("Value", ""),
                Developer       = projectDatas.FindAttribute("Name", "developer").GetAttributeValue("Value", ""),
                DistrictCode    = projectDatas.FindAttribute("Name", "districtCode").GetAttributeValue("Value", ""),
                PostCode        = projectDatas.FindAttribute("Name", "postcode").GetAttributeValue("Value", ""),
                StreetName      = projectDatas.FindAttribute("Name", "streetname").GetAttributeValue("Value", ""),
                StreetNumber    = projectDatas.FindAttribute("Name", "streetnumber").GetAttributeValue("Value", ""),
                Longitude       = projectDatas.FindAttribute("Name", "longitude").GetAttributeValue("Value", 0.00),
                Latitude        = projectDatas.FindAttribute("Name", "latitude").GetAttributeValue("Value", 0.00),
                StatusCode      = projectDatas.FindAttribute("Name", "statusCode").GetAttributeValue("Value", ""),
                EstateCode      = projectDatas.FindAttribute("Name", "estateCode").GetAttributeValue("Value", ""),
                Url             = projectDatas.FindAttribute("Name", "url").GetAttributeValue("Value", ""),
                PropertyId      = projectDatas.FindAttribute("Name", "propertyId").GetAttributeValue<int?>("Value", null),
                PropertyName    = projectDatas.FindAttribute("Name", "propertyName").GetAttributeValue("Value", ""),
                ProjectNameAlt  = projectDatas.FindAttribute("Name", "projectName").GetAttributeValue("Value", ""),
                PropertyType    = projectDatas.FindAttribute("Name", "propertyType").GetAttributeValue("Value", ""),
                ProjectFloors   = projectDatas.FindAttribute("Name", "floors").GetAttributeValue("Value", 0)
            };

            // ── Details 子模型 ─────────────────────────────────────────
            var det = new GuruDetailsData();
            d.Details = det;

            // 物业基本信息
            det.PropertyName      = detialss.FindAttribute("Name", "property_name").GetAttributeValue("Value", "");
            det.PropertyId        = detialss.FindAttribute("Name", "property_id").GetAttributeValue<int?>("Value", null);
            det.LocationId        = detialss.FindAttribute("Name", "location_id").GetAttributeValue<int?>("Value", null);
            det.PgVerifiedId      = detialss.FindAttribute("Name", "pg_verified_id").GetAttributeValue("Value", "");
            det.PropertyTypeGroup = detialss.FindAttribute("Name", "property_type_group").GetAttributeValue("Value", "N");
            det.PropertyTypeCode  = detialss.FindAttribute("Name", "property_type_code").GetAttributeValue("Value", "");
            det.HdbType           = detialss.FindAttribute("Name", "hdb_type").GetAttributeValue<string>("Value", null);
            det.HdbEstate         = detialss.FindAttribute("Name", "hdb_estate").GetAttributeValue("Value", "");
            det.District          = detialss.FindAttribute("Name", "district").GetAttributeValue("Value", "");
            det.Tenure            = detialss.FindAttribute("Name", "tenure").GetAttributeValue("Value", "");

            // 挂牌信息
            det.ListingType = detialss.FindAttribute("Name", "listing_type").GetAttributeValue("Value", "SALE");
            det.ListingTitle = detialss.FindAttribute("Name", "listing_title").GetAttributeValue("Value", DefaultTitles.GetTitle());
            if (string.IsNullOrEmpty(det.ListingTitle)) det.ListingTitle = DefaultTitles.GetTitle();
            det.ListingDescription = detialss.FindAttribute("Name", "listing_description").GetAttributeValue("Value", "");
            if (det.ListingDescription.Length > 2000) det.ListingDescription = det.ListingDescription.Substring(0, 1999);
            det.LeaseTerm    = detialss.FindAttribute("Name", "lease_term").GetAttributeValue("Value", "");
            det.AvailableDate = detialss.FindAttribute("Name", "available_date").GetAttributeValue("Value", "");

            // 价格
            det.Price          = detialss.FindAttribute("Name", "price").GetAttributeValue<int>("Value", 0);
            det.PriceType      = detialss.FindAttribute("Name", "price_type").GetAttributeValue("Value", "VTO");
            det.MaintenanceFee = detialss.FindAttribute("Name", "tep_maintenance_fee").GetAttributeValue("Value", 0);

            // 面积
            det.FloorArea = detialss.FindAttribute("Name", "floorarea").GetAttributeValue<int?>("Value", null);
            det.LandArea  = detialss.FindAttribute("Name", "landarea").GetAttributeValue<int?>("Value", null);

            // 房间配置
            det.Bedrooms  = detialss.FindAttribute("Name", "bedrooms").GetAttributeValue<int?>("Value", null);
            det.Bathrooms = detialss.FindAttribute("Name", "bathrooms").GetAttributeValue<int?>("Value", null);
            det.RoomType  = detialss.FindAttribute("Name", "room_type").GetAttributeValue("Value", "");

            // 地址
            det.PostalCode   = detialss.FindAttribute("Name", "postcode").GetAttributeValue("Value", "");
            det.StreetName   = detialss.FindAttribute("Name", "streetname").GetAttributeValue("Value", "");
            det.StreetNumber = detialss.FindAttribute("Name", "streetnumber").GetAttributeValue("Value", "");
            det.Longitude    = detialss.FindAttribute("Name", "longitude").GetAttributeValue("Value", 0.00);
            det.Latitude     = detialss.FindAttribute("Name", "latitude").GetAttributeValue("Value", 0.00);
            det.FloorNumber  = detialss.FindAttribute("Name", "property_level_number").GetAttributeValue("Value", "");
            det.UnitNumber   = detialss.FindAttribute("Name", "property_unit_number").GetAttributeValue("Value", "");

            // 单元属性
            det.FloorLevel    = detialss.FindAttribute("Name", "floor_level").GetAttributeValue("Value", "").ToUpper();
            det.Furnishing    = detialss.FindAttribute("Name", "furnishing").GetAttributeValue<string>("Value", null);
            det.CeilingHeight = detialss.FindAttribute("Name", "ceiling_height").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.CeilingHeight)) det.CeilingHeight = null;
            det.SrxTenanted   = detialss.FindAttribute("Name", "srx_tenanted").GetAttributeValue("Value", "No");
            det.TenantedUntil = detialss.FindAttribute("Name", "tenanted_until").GetAttributeValue("Value", "");
            det.SellerEthnic   = detialss.FindAttribute("Name", "sellerEthnic").GetAttributeValue("Value", "");
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
            det.ElectricityPhase  = detialss.FindAttribute("Name", "electricity_phase").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.ElectricityPhase)) det.ElectricityPhase = null;
            det.FloorLoading         = detialss.FindAttribute("Name", "floor_loading").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.FloorLoading)) det.FloorLoading = null;
            det.FloorLoadingCategory = detialss.FindAttribute("Name", "floor_loading_category").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.FloorLoadingCategory)) det.FloorLoadingCategory = null;
            det.IsHighCeiling = detialss.FindAttribute("Name", "is_high_ceiling").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.IsHighCeiling)) det.IsHighCeiling = null;
            det.LiftCargo    = detialss.FindAttribute("Name", "lift_cargo").GetAttributeValue<int?>("Value", null);
            det.LiftPassenger = detialss.FindAttribute("Name", "lift_passenger").GetAttributeValue<int?>("Value", null);
            det.Ramp         = detialss.FindAttribute("Name", "ramp").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.Ramp)) det.Ramp = null;
            det.Condition    = detialss.FindAttribute("Name", "condition").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.Condition)) det.Condition = null;
            det.PropertyUse  = detialss.FindAttribute("Name", "property_use").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.PropertyUse)) det.PropertyUse = null;
            det.CookingType  = detialss.FindAttribute("Name", "cooking_type").GetAttributeValue<string>("Value", null);
            if (string.IsNullOrEmpty(det.CookingType)) det.CookingType = null;

            // 经纪人信息
            det.AlternativeAgent  = detialss.FindAttribute("Name", "alternative_agent").GetAttributeValue("Value", "");
            det.AlternativeMobile = detialss.FindAttribute("Name", "alternative_mobile").GetAttributeValue("Value", "");
            det.AlternativePhone  = detialss.FindAttribute("Name", "alternative_phone").GetAttributeValue("Value", "");
            det.AlternativeEmail  = detialss.FindAttribute("Name", "alternative_email").GetAttributeValue("Value", "");

            // 任务信息
            det.HiddenListingId = detialss.FindAttribute("Name", "hidden_listing_id").GetAttributeValue<int?>("Value", null);
            det.TaskItemId  = detialss.FindAttribute("Name", "taskitem_id").GetAttributeValue("Value", "0");
            det.UpdateTime  = detialss.FindAttribute("Name", "UpdateTime").GetAttributeValue("Value", "");
            det.FastRepost  = detialss.FindAttribute("Name", "FastRepost").GetAttributeValue("Value", "0");

            return d;
        }
    }
}