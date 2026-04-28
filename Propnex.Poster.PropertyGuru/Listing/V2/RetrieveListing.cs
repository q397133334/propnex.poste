using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Flurl.Http;

namespace Propnex.Poster.PropertyGuru.Listing.V2
{
    [System.Reflection.ObfuscationAttribute(Feature = "properties renaming")]
    public class RetrieveListing
    {
        public RetrieveListing()
        {
            stPropertyAccount = "";
            stPropertyStatus = "";
            Quality = "";
        }
        public bool IsResidential
        {
            get
            {
                return Details["property_type_group"] == "H" || Details["property_type_group"] == "N" || Details["property_type_group"] == "L" || Details["property_type_group"] == "T" || Details["property_type_group"] == "B" || Details["property_type_group"] == "S";
            }
        }
        public bool Selected { get; set; }
        public bool Featured { get; set; }
        public string Quality { get; set; }
        public string Errors { get; set; }
        public string ListingName { get; set; }

        public string ListingType { get; set; }

        public string PropertyType { get; set; }

        public string Photos { get; set; }
        public string OriginPhotos { get; set; }
        public string PhotosTime { get; set; }
        public string ProjectLogo { get; set; }
        public string DeveloperLogo { get; set; }
        public string Tours { get; set; }
        public string TourThumbnails { get; set; }
        public string Videos { get; set; }
        public string VideoThumbnails { get; set; }

        public bool NoGuruPhotos { get; set; }
        public bool NoiPropertyPhotos { get; set; }
        public bool NostPropertyPhotos { get; set; }
        public bool UseFileName { get; set; }
        public string EmbedVideo { get; set; }

        public string FloorPlan { get; set; }

        public string BedRooms { get; set; }

        public string FloorArea { get; set; }

        public string Price { get; set; }

        public string Notes { get; set; }
        public string ReportStatus { get; set; }
        public string Status { get; set; }
        public string iPropertyStatus { get; set; }
        public string stPropertyStatus { get; set; }
        public string PreeminentStatus { get; set; }
        public string GumtreeStatus { get; set; }
        public string MudahStatus { get; set; }
        public string PropwallStatus { get; set; }
        public string SRXStatus { get; set; }
        public string Accounts
        {
            get
            {
                string s = "";
                if (!string.IsNullOrEmpty(Account)) s += Account + "(GURU)\r\n";
                if (!string.IsNullOrEmpty(iPropertyAccount)) s += iPropertyAccount + "(IP)\r\n";
                if (!string.IsNullOrEmpty(stPropertyAccount)) s += stPropertyAccount + "(ST)\r\n";
                if (!string.IsNullOrEmpty(GumtreeStatus)) s += GumtreeStatus + "(ST)\r\n";
                if (!string.IsNullOrEmpty(MudahStatus)) s += MudahStatus + "(MUDAH)\r\n";
                if (!string.IsNullOrEmpty(PropwallStatus)) s += PropwallStatus + "(PROPWALL)\r\n";
                if (!string.IsNullOrEmpty(SRXStatus)) s += SRXStatus + "(SRX)\r\n";
                return s;
            }
        }

        public string StatusString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(Status))
                {
                    sb.Append("(PG) ").Append(Status);
                };
                if (!string.IsNullOrEmpty(iPropertyStatus))
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append("(iP) ").Append(iPropertyStatus);
                }
                if (!string.IsNullOrEmpty(stPropertyStatus))
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append("(ST) ").Append(stPropertyStatus);
                }
                if (!string.IsNullOrEmpty(GumtreeStatus))
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append("(GT) ").Append(GumtreeStatus);
                }
                if (!string.IsNullOrEmpty(MudahStatus))
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append("(MUDAH) ").Append(MudahStatus);
                }
                if (!string.IsNullOrEmpty(PropwallStatus))
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append("(PROPWALL) ").Append(PropwallStatus);
                }
                if (!string.IsNullOrEmpty(SRXStatus))
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append("(SRX) ").Append(SRXStatus);
                }
                return sb.ToString();
            }
        }
        public string Country { get; set; }
        public string ListingLink { get; set; }
        public string iPropertyLink { get; set; }
        public string stPropertyLink { get; set; }
        public string PreeminentLink { get; set; }
        public string GumtreeLink { get; set; }
        public string MudahLink { get; set; }
        public string PropwallLink { get; set; }
        public string SRXLink { get; set; }

        public string ID { get; set; }

        public string Account { get; set; }

        private string iPropertyAccount;
        public string IPropertyAccount
        {
            get { return iPropertyAccount; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    iPropertyAccount = "";
                else
                    iPropertyAccount = value;
            }
        }
        private string stPropertyAccount { get; set; }
        public string STPropertyAccount
        {
            get { return stPropertyAccount; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    stPropertyAccount = "";
                else
                    stPropertyAccount = value;
            }
        }
        private string xportal = "";
        public string XportalAccount
        {
            get { return xportal; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    xportal = "";
                else
                    xportal = value;
            }
        }
        private string xpressor = "";
        public string XpressorAccount
        {
            get { return xpressor; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    xpressor = "";
                else
                    xpressor = value;
            }
        }
        private string preeminent = "";
        public string PreeminentAccount
        {
            get { return preeminent; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    preeminent = "";
                else
                    preeminent = value;
            }
        }
        private string Gumtree = "";
        public string GumtreeAccount
        {
            get { return Gumtree; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Gumtree = "";
                else
                    Gumtree = value;
            }
        }
        private string Mudah = "";
        public string MudahAccount
        {
            get { return Mudah; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Mudah = "";
                else
                    Mudah = value;
            }
        }
        private string Propwall = "";
        public string PropwallAccount
        {
            get { return Propwall; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Propwall = "";
                else
                    Propwall = value;
            }
        }
        public string ListDate { get; set; }

        public string OriginPrice { get; set; }
        //spam detect and prevent
        public string XID { get; set; }

        public string LastPost { get; set; }

        public int PostCount { get; set; }

        public string RefencesNotes { get; set; }
        public bool IsRefreshed { get; set; }
        public string SRXEncryptedId { get; set; }
        public string SRXEncryptedAgencyId { get; set; }
        public Dictionary<string, string> Details = new Dictionary<string, string>();
        public Dictionary<string, string> Basic = new Dictionary<string, string>();
        public Dictionary<string, string> Location = new Dictionary<string, string>();
        public Dictionary<string, string> Media = new Dictionary<string, string>();
        public Dictionary<string, string> Sales = new Dictionary<string, string>();
        public Dictionary<string, string> Summary = new Dictionary<string, string>();
        public Dictionary<string, string> ProjectData = new Dictionary<string, string>();

        public static async System.Threading.Tasks.Task<RetrieveListing> Converter(CreateOrUpdateListing createOrUpdate, string account, string portal, string id)
        {
            var listing = new RetrieveListing();

            listing.Details["listing_description"] = System.Web.HttpUtility.HtmlDecode((string)createOrUpdate.localizedDescription);
            listing.Details["available_for_cobroke"] = createOrUpdate.cobroke?.ToString();
            listing.Details["hidden_listing_id"] = createOrUpdate.id.Value.ToString();
            listing.Details["listing_notes"] = createOrUpdate.notes?.ToString();
            listing.Details["lease_term"] = createOrUpdate.leaseTermCode;
            listing.ListingType = createOrUpdate.typeCode;
            listing.Quality = createOrUpdate.qualityScore.ToString() + "%";

            listing.PropertyType = createOrUpdate.typeText;
            listing.ListingName = createOrUpdate.localizedTitle;
            listing.Details["standard_name"] = createOrUpdate.property.name;
            listing.Details["property_name"] = createOrUpdate.property.name;
            listing.Details["property_url"] = "http://www.propertyguru.com.sg/project/" + createOrUpdate.property.id?.ToString();
            listing.Details["property_id"] = createOrUpdate.property.id?.ToString();
            listing.Details["property_type_code"] = createOrUpdate.property.typeCode;
            listing.Details["property_type_group"] = createOrUpdate.property.typeGroup;
            listing.Details["tenure"] = createOrUpdate.property.tenureCode;
            listing.Details["constructionyear"] = createOrUpdate.property.topYear?.ToString();
            listing.Details["numberofunits"] = createOrUpdate.property.totalUnits?.ToString();
            listing.Details["numberoffloors"] = createOrUpdate.property.floors?.ToString();

            foreach (var item in createOrUpdate.property.amenities)
            {
                listing.Details["amenities," + item.code] = "Checked";
            }

            listing.Details["hdb_type"] = createOrUpdate.propertyUnit.hdbTypeCode;

            listing.Details["fittings"] = createOrUpdate.propertyUnit.furnishingCode;
            listing.Details["furnishing"] = createOrUpdate.propertyUnit.furnishingCode;
            listing.Details["floor_level"] = createOrUpdate.propertyUnit.floorLevelCode;
            listing.Details["electricity_supply"] = createOrUpdate.propertyUnit.electricitySupply?.ToString();
            listing.Details["electricity_unit"] = "KVA";
            listing.Details["electricity_phase"] = createOrUpdate.propertyUnit.electricityPhase;
            listing.Details["ceiling_height"] = createOrUpdate.propertyUnit.ceilingHeight;
            listing.Details["floor_loading"] = createOrUpdate.propertyUnit.floorLoading;

            foreach (var item in createOrUpdate.propertyUnit.features)
            {
                listing.Details["unit_features," + item.code] = "Checked";
            }

            listing.Price = createOrUpdate.price.value.Value.ToString();
            listing.Details["price"] = listing.Price;
            listing.Details["price_type"] = createOrUpdate.price.type.code;
            listing.Details["valuation_price"] = createOrUpdate.price.valuation?.ToString();

            listing.BedRooms = createOrUpdate.sizes.bedrooms.value?.ToString();
            listing.Details["bedrooms"] = listing.BedRooms;
            listing.Details["bathrooms"] = createOrUpdate.sizes.bathrooms.value?.ToString();
            listing.FloorArea = createOrUpdate.sizes.floorArea[0].value?.ToString();
            listing.Details["floorarea"] = listing.FloorArea;
            listing.Details["landarea"] = createOrUpdate.sizes.landArea[0].value?.ToString();
            listing.Details["floorarea_unit"] = "sqft";

            listing.Details["district"] = createOrUpdate.location.districtCode;
            listing.Details["streetname"] = createOrUpdate.location.streetName1;
            listing.Details["streetnumber"] = createOrUpdate.location.streetNumber;
            listing.Details["location_id"] = createOrUpdate.location.id.ToString();
            listing.Details["latitude"] = createOrUpdate.location.latitude.ToString();
            listing.Details["longitude"] = createOrUpdate.location.longitude.ToString();
            listing.Details["postcode"] = createOrUpdate.location.postalCode;
            listing.Details["hdb_estate"] = createOrUpdate.location.hdbEstateCode;

            if (string.IsNullOrEmpty(createOrUpdate.location.unit) == false)
            {
                string[] ss = createOrUpdate.location.unit.Split(new char[] { '-' });
                listing.Details["property_level_number"] = ss[0];
                listing.Details["property_unit_number"] = ss.Length > 1 ? ss[1] : "";
            }

            listing.Details["alternative_agent"] = createOrUpdate.agent.alternativeAgent == null ? "" : createOrUpdate.agent.alternativeAgent;
            listing.Details["alternative_mobile"] = createOrUpdate.agent.alternativeMobile == null ? "" : createOrUpdate.agent.alternativeMobile;
            listing.Details["show_mobile"] = createOrUpdate.agent.showProfile.Value.ToString();
            listing.Details["alternative_phone"] = createOrUpdate.agent.alternativePhone == null ? "" : createOrUpdate.agent.alternativePhone;
            listing.Details["alternative_email"] = createOrUpdate.agent.alternativeEmail == null ? "" : createOrUpdate.agent.alternativeEmail;

            string agent = "";
            if (listing.Details.ContainsKey("alternative_agent") && !string.IsNullOrEmpty(listing.Details["alternative_agent"]))
                agent = listing.Details["alternative_agent"] + " - ";

            listing.Details["RetrieveFrom"] = string.Format("{2}{0}({1})", account, portal, agent);

            listing.XID = Guid.NewGuid().ToString();

            var savepath = "";
            if (createOrUpdate.media.listing.Count > 0)
            {
                listing.UseFileName = true;
                string baseDir = AppContext.BaseDirectory;
                // 如果 task 文件夹是在根目录下，保留原逻辑；如果是在程序目录下，则直接使用 baseDir
                // 原逻辑是取盘符根目录，例如 C:\task\{id}
                string rootPath = Path.GetPathRoot(baseDir);
                savepath = Path.Combine(rootPath, "task", id);
                if (!Directory.Exists(savepath))
                {
                    Directory.CreateDirectory(savepath);
                }
            }
            StringBuilder pics = new StringBuilder();
            foreach (var item in createOrUpdate.media.listing)
            {
                string p = item.V550;
                string filename = item.id.Value.ToString() + "." + Path.GetFileName(p);
                string title = item.caption?.Replace(" ", "-")?.Trim();
                filename = MakeValidFileName(title + ".(RP)" + filename);
                try
                {
                    var result = await p.WithTimeout(30).DownloadFileAsync(savepath, filename);
                    if (File.Exists(result))
                    {
                        result = OverlayWatermark(result);
                        pics.Append(result).Append(Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            listing.Photos = pics.ToString();
            listing.FloorPlan = "";
            if (createOrUpdate.media.listingFloorplans != null)
            {
                foreach (var item in createOrUpdate.media.listingFloorplans)
                {
                    string p = item.V550;
                    listing.FloorPlan = p;
                    string filename = p.Substring(p.LastIndexOf("/") + 1);
                    try
                    {
                        var result = await p.WithTimeout(30).DownloadFileAsync(savepath, filename);
                        if (System.IO.File.Exists(result))
                        {
                            result = OverlayWatermark(result);
                            listing.FloorPlan = result;
                        }
                        else
                        {
                            listing.FloorPlan = "";
                        };
                    }
                    catch (Exception ex)
                    {

                    }
                    break;
                }
            }
            StringBuilder vFiles = new StringBuilder();
            StringBuilder vThumbs = new StringBuilder();
            StringBuilder viFiles = new StringBuilder();
            StringBuilder viThumbs = new StringBuilder();
            if (createOrUpdate.media.listingVideos != null)
            {
                foreach (var item in createOrUpdate.media.listingVideos)
                {
                    string filename = "";
                    string title = item.caption;
                    bool isVtour = false;
                    StringBuilder sbFile = isVtour ? vFiles : viFiles;
                    StringBuilder sbThumb = isVtour ? vThumbs : viThumbs;
                    string p = item.file;
                    if (item.type != "htm")
                    {
                        filename = p.Substring(p.LastIndexOf("/") + 1);
                        if (string.IsNullOrEmpty(filename)) continue;
                        string ext = System.IO.Path.GetExtension(filename);
                        filename = MakeValidFileName(string.Format("{0}.{1}.{2}", title, DateTime.Now.ToString("yyyyMMdd"), filename));
                        try
                        {
                            var result = await p.WithTimeout(30).DownloadFileAsync(savepath, filename);
                            sbFile.Append(result).Append("#").Append(title).Append(Environment.NewLine);
                        }
                        catch (Exception ex)
                        {

                        };
                    }
                    else
                    {
                        p = item.embed_html;
                        sbFile.Append(p).Append("#").Append(title).Append(Environment.NewLine);
                    };
                    string turl = item.thumb;
                    filename = turl.Substring(turl.LastIndexOf("/") + 1);
                    if (turl.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase))
                    {
                        filename = await turl.WithTimeout(30).DownloadFileAsync(savepath, filename);
                    };
                    if (System.IO.File.Exists(filename))
                    {
                        byte[] bin = System.IO.File.ReadAllBytes(filename);
                        string base64 = System.Convert.ToBase64String(bin, 0, bin.Length);
                        sbThumb.Append(base64).Append(Environment.NewLine);
                    }
                    else
                    {
                        sbThumb.Append(Environment.NewLine);
                    };
                }
            }

            if (createOrUpdate.media.listingVirtualTours != null)
            {
                foreach (var item in createOrUpdate.media.listingVirtualTours)
                {
                    string filename = "";
                    string title = item.caption;
                    bool isVtour = true;
                    StringBuilder sbFile = isVtour ? vFiles : viFiles;
                    StringBuilder sbThumb = isVtour ? vThumbs : viThumbs;
                    string p = item.file;
                    if (item.type != "htm")
                    {
                        filename = p.Substring(p.LastIndexOf("/") + 1);
                        if (string.IsNullOrEmpty(filename)) continue;
                        string ext = System.IO.Path.GetExtension(filename);
                        filename = MakeValidFileName(string.Format("{0}.{1}.{2}", title, DateTime.Now.ToString("yyyyMMdd"), filename));
                        try
                        {
                            var result = await p.DownloadFileAsync(savepath, filename);
                            sbFile.Append(result).Append("#").Append(title).Append(Environment.NewLine);
                        }
                        catch (Exception ex)
                        {

                        };
                    }
                    else
                    {
                        p = item.embed_html;
                        sbFile.Append(p).Append("#").Append(title).Append(Environment.NewLine);
                    };
                    string turl = item.thumb;
                    filename = turl.Substring(turl.LastIndexOf("/") + 1);
                    if (turl.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase))
                    {
                        filename = await turl.DownloadFileAsync(savepath, filename);
                    };
                    if (System.IO.File.Exists(filename))
                    {
                        byte[] bin = System.IO.File.ReadAllBytes(filename);
                        string base64 = System.Convert.ToBase64String(bin, 0, bin.Length);
                        sbThumb.Append(base64).Append(Environment.NewLine);
                    }
                    else
                    {
                        sbThumb.Append(Environment.NewLine);
                    };
                }
            }
            listing.TourThumbnails = vThumbs.ToString();
            listing.Tours = vFiles.ToString();
            listing.Videos = viFiles.ToString();
            listing.VideoThumbnails = viThumbs.ToString();
            listing.EmbedVideo = "";

            return listing;
        }


        public static Tuple<Dictionary<string, string>, bool, string> GetData(RetrieveListing l, string user, string password, string task_id)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["account_name"] = user;
            data["account_password"] = password;
            data["task_id"] = task_id;
            data["Listings[listing_type]"] = l.ListingType;
            data["Listings[property_type]"] = string.IsNullOrEmpty(l.PropertyType) ? "" : l.PropertyType;
            data["Listings[listing_name]"] = l.ListingName;
            data["Listings[price]"] = l.Price;
            data["Listings[floor_area]"] = l.FloorArea;
            data["Listings[bedrooms]"] = l.BedRooms;
            data["Listings[xid]"] = l.XID;
            data["Listings[embed_video]"] = l.EmbedVideo;
            data["Listings[no_portal_photo]"] = "0";
            data["Listings[notes]"] = l.RefencesNotes;

            if (l.Basic.ContainsKey("district"))
            {
                data["Listings[district]"] = l.Basic["district"];
            }
            else
            {
                if (l.Details.ContainsKey("RetrieveFrom") && l.Details["RetrieveFrom"].IndexOf("(MyIP)") < 0)
                {
                    if (l.Details.ContainsKey("district"))
                    {
                        if (l.PropertyType != null && l.PropertyType.StartsWith("HDB") && l.Details["district"] == "")
                        {
                            l.Details["district"] = "D04";
                        };
                        data["Listings[district]"] = l.Details["district"];
                    }
                    else
                    {
                        return new Tuple<Dictionary<string, string>, bool, string>(null, false, "Listing missing data: district");
                    };
                };
            };
            foreach (var item in l.Details)
            {
                data["Listings[details][" + System.Web.HttpUtility.UrlEncode(item.Key) + "]"] = item.Value;
            };
            foreach (var item in l.Basic)
            {
                data["Listings[basic][" + System.Web.HttpUtility.UrlEncode(item.Key) + "]"] = item.Value;
            };
            foreach (var item in l.Location)
            {
                data["Listings[location][" + System.Web.HttpUtility.UrlEncode(item.Key) + "]"] = item.Value;
            };
            foreach (var item in l.Media)
            {
                data["Listings[media][" + System.Web.HttpUtility.UrlEncode(item.Key) + "]"] = item.Value;
            };
            foreach (var item in l.Sales)
            {
                data["Listings[sales][" + System.Web.HttpUtility.UrlEncode(item.Key) + "]"] = item.Value;
            };
            foreach (var item in l.Summary)
            {
                data["Listings[summary][" + System.Web.HttpUtility.UrlEncode(item.Key) + "]"] = item.Value;
            };

            return new Tuple<Dictionary<string, string>, bool, string>(data, true, "");
        }

        private static string OverlayWatermark(string filename)
        {
            return filename;
            //try
            //{
            //    if (filename.EndsWith("pdf")) return filename;
            //    Image img = Bitmap.FromFile(filename);
            //    int X, Y, H, W;
            //    X = Y = H = W = 0;
            //    Y = (int)System.Math.Round((img.Height * 0.5), 0);
            //    X = (int)(img.Width - 360);
            //    W = 360;
            //    H = 55;
            //    int Width = (int)System.Math.Round(H * 6.18, 0);
            //    if (img.Width - Width < 56)
            //    {
            //        int newWidth = img.Width - 56;
            //        int newH = H * newWidth / Width;
            //        Y -= H - newH + 2;
            //        H = newH + 2;
            //        Width = newWidth;
            //    };
            //    string photo = "";
            //    using (Image src = Image.FromFile("480x80.png"))
            //    using (Bitmap dst = new Bitmap(img.Width, img.Height))
            //    using (Graphics g = Graphics.FromImage(dst))
            //    {
            //        g.SmoothingMode = SmoothingMode.AntiAlias;
            //        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //        g.DrawImage(img, 0, 0, dst.Width, dst.Height);
            //        g.DrawImage(src, X, Y, W, H);
            //        photo = System.IO.Path.GetDirectoryName(filename) + "\\" + System.IO.Path.GetFileNameWithoutExtension(filename) + "_fixed" + ".jpg";
            //        dst.Save(photo, System.Drawing.Imaging.ImageFormat.Jpeg);
            //    }
            //    img.Dispose();
            //    return photo;
            //}
            //catch (Exception ex)
            //{
            //    return "";
            //}
        }

        public static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}]+", invalidChars);
            return Regex.Replace(name, invalidReStr, "-");
        }
    }

    public class WebClientEx : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = 1000 * 60 * 3;
            return request;
        }
    }
}
