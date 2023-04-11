using CefSharp;
using CefSharp.Dom;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;
using Propnex.Poster.IProperty;
using Propnex.Poster.Share;
using PropnexPoster.NetCoreWinForm;
using Serilog;
using System.CodeDom;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;

namespace Propnex.Poster.NetCoreWinForm
{
    public partial class IPropertyCefPoster : Form, ITransientDependency, IPosterStart
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IPropnexTaskProvider _propnexTaskProvider;

        private PropnexTasks propnexTasks;
        private PropnexTask propnexTask;
        private List<Listing> Listings = new List<Listing>();

        private DevToolsContext DevToolsContext;
        private ILogger? _logger;

        private static object _lock = new object();
        private JsonSerializerSettings jsonSerialzerSettings = new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        public IPropertyCefPoster(
                ILocalEventBus localEventBus,
                IPropnexTaskProvider propnexTaskProvider)
        {
            InitializeComponent();
            _localEventBus = localEventBus;
            _propnexTaskProvider = propnexTaskProvider;
        }

        public async Task Start()
        {
            await PublishMessageAsync("Start a new task");
            await GetTask();
            _logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Async(c => c.File($"{Directory.GetDirectoryRoot(System.AppDomain.CurrentDomain.BaseDirectory)}\\logs\\task\\{"test"}.txt"))
                        .CreateLogger();
            foreach (var item in propnexTasks.Tasks)
            {
                propnexTask = item;
                var loginResult = await Login();
                if (loginResult.Status != PosterActionResultStatus.Success)
                {
                    await PublishMessageAsync(loginResult.Message);
                    break;
                }
                var listingResult = await GetListings();
                if (listingResult.Status != PosterActionResultStatus.Success)
                {
                    await PublishMessageAsync(listingResult.Message);
                    break;
                }
                if (item.TaskType == "Post Only")
                {
                    propnexTask = item;
                    await PostOnly();
                }
            }
            Close();
        }

        public async Task PostOnly()
        {
            foreach (var task in propnexTask.Listings.Listings)
            {
                var result = await CreateListing(task);
            }
        }


        private RequestData<Variables<AddListingMutationDto>>? addListingMutationDto;
        private LocationDto locationDto;

        public async Task<PosterActionResult> CreateListing(PropnexListing propnexListing)
        {
            string addListingMutationUrl = "https://www.iproperty.com.my/pro/rasor/graphql/addListingMutation";
            string? listingId = "104938596";

            var resultListingDetails = await listingDetails();
            if (resultListingDetails.Status == PosterActionResultStatus.Success)
                listingId = resultListingDetails.Data;
            else
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultListingDetails.Message
                };


            var locationResult = await location(propnexListing, listingId);
            if (locationResult.Status == PosterActionResultStatus.Error)
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = locationResult.Message
                };

            var resultProperytDetails = await propertyDetails(propnexListing, listingId);


            async Task<PosterActionResult<string>> listingDetails()
            {
                return await GetPolicy<string>().ExecuteAsync(async (ctx) =>
                {
                    propnexListing.Details["data_step1"] = propnexListing.Details["data_step1"].Replace("\"location\":[]", "\"location\":{}");
                    addListingMutationDto = JsonConvert.DeserializeObject<RequestData<Variables<AddListingMutationDto>>>(propnexListing.Details["data_step1"]);
                    string jsonData = JsonConvert.SerializeObject(addListingMutationDto, jsonSerialzerSettings);

                    var result = await AjaxJsonPostAsync(addListingMutationUrl, "https://www.iproperty.com.my/pro/add-listing/listing-details", data: jsonData);
                    if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                    {
                        await PublishMessageAsync(result);
                        throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                    }
                    _logger.Information(result);
                    var data = JsonConvert.DeserializeObject<ResponseData<AddListingPayload>>(result);
                    listingId = data?.Data.addListing.listing.Id;
                    await PublishMessageAsync($"create listing details success,listing is is {listingId}");
                    return new PosterActionResult<string>()
                    {
                        Data = listingId,
                        Status = PosterActionResultStatus.Success
                    };

                }, new Context("listingDetails"));
            }


            async Task descriptionMedia() { }

            async Task UpgradePublish() { }

            return new PosterActionResult();
        }

        public async Task<PosterActionResult<Listing>> propertyDetails(PropnexListing propnexListing, string listingId)
        {

            return new PosterActionResult<Listing>();
        }

        public async Task<PosterActionResult<Listing>> location(PropnexListing propnexListing, string listingId)
        {
            //1.解析location数据
            var location = JsonConvert.DeserializeObject<RequestData<Variables<LocationDto>>>(propnexListing.Details["data_location"].Replace("\"extension\":[]", "\"extension\":{}"));
            location.variables.input.id = listingId;
            string buildingText = "";
            if (propnexListing.Basic.ContainsKey("txtBuilding"))
            {
                buildingText = propnexListing.Basic["txtBuilding"];
            }
            if (string.IsNullOrEmpty(buildingText))
            {
                if (propnexListing.Basic.ContainsKey("txtBuildingCom"))
                {
                    buildingText = propnexListing.Basic["txtBuildingCom"];
                }
            }
            if (string.IsNullOrEmpty(buildingText) && propnexListing.Details.ContainsKey("standard_name"))
            {
                if (!string.IsNullOrEmpty(propnexListing.Details["standard_name"]))
                {
                    buildingText = propnexListing.Details["standard_name"];
                }
            }
            if (string.IsNullOrEmpty(buildingText) && propnexListing.ProjectData.ContainsKey("name"))
            {
                buildingText = propnexListing.ProjectData["name"];
            }

            var addInput = addListingMutationDto.variables.input;

            if (!(addInput.propertyTypeCode.Value == 1 ||
    addInput.propertyTypeCode.Value == 2 ||
    addInput.propertyTypeCode.Value == 3 ||
    addInput.propertyTypeCode.Value == 4))
            {
                buildingText = "";
            }
            if (!propnexListing.Basic.ContainsKey("ddlCity")) propnexListing.Basic["ddlCity"] = "";
            if (!propnexListing.Basic.ContainsKey("txtMapLat")) propnexListing.Basic["txtMapLat"] = "";
            if (!propnexListing.Basic.ContainsKey("txtMapLon")) propnexListing.Basic["txtMapLon"] = "";
            if (!propnexListing.Basic.ContainsKey("txtAddress")) propnexListing.Basic["txtAddress"] = "";
            if (!propnexListing.Basic.ContainsKey("txtPostCode")) propnexListing.Basic["txtPostCode"] = "";
            if (!propnexListing.Basic.ContainsKey("txtTownship")) propnexListing.Basic["txtTownship"] = "";

            PlaceDto? place = null;

            if (buildingText != "")
            {
                var buildingsResult = await BuildingQuery(buildingText);
                if (buildingsResult.Status == PosterActionResultStatus.Success)
                {
                    var buildings = buildingsResult.Data;
                    place = buildings.FirstOrDefault(q => q.postCode == propnexListing.Basic["txtPostCode"]);
                    if (place == null)
                    {
                        return new PosterActionResult<Listing>()
                        {
                            Status = PosterActionResultStatus.Error,
                            Message = $"Not find building,data:{buildingText}"
                        };
                    }

                    var input = location.variables.input;
                    var loc = location.variables.input.location;
                    loc.Address.en_GB = place.Address.en_GB;
                    input.propertyGroupTypeCode = place.propertyGroupTpye.Code;
                    input.propertyTypeCode = place.propertyType.Code;
                    loc.postalCode = place.postalCode;
                    loc.longitude = place.longitude;
                    loc.latitude = place.latitude;
                    loc.Level1.Id = place.Level1.Id;
                    loc.Level2.Id = place.Level2.Id;
                    loc.Level3.Text.en_GB = place.Level3.Text.en_GB;
                    loc.Level5.Id = place.Level5.Id;
                }
                else
                {
                    return new PosterActionResult<Listing>()
                    {
                        Status = PosterActionResultStatus.Error,
                        Message = $"Find building error ,data:{buildingText},message :{buildingsResult.Message}"
                    };
                }
            }
            else
            {
                //get level2 id 
                var buildingsResult = await level2Query(location.variables.input.location.Level2.Text.en_GB, location.variables.input.location.Level1.Id);
                if (buildingsResult.Status == PosterActionResultStatus.Success)
                {
                    var buildings = buildingsResult.Data;
                    if (buildings.Count == 0)
                    {
                        return new PosterActionResult<Listing>()
                        {
                            Status = PosterActionResultStatus.Error,
                            Message = $"Not find building,data :{location.variables.input.location.Level2.Text.en_GB},{location.variables.input.location.Level1.Id}"
                        };
                    }

                    place = buildings.FirstOrDefault();

                    var input = location.variables.input;
                    var loc = location.variables.input.location;
                    //loc.Address.en_GB = place.Address.en_GB;
                    //input.propertyGroupTypeCode = place.propertyGroupTpye.Code;
                    //input.propertyTypeCode = place.propertyType.Code;
                    //loc.postalCode = place.postalCode;
                    //loc.longitude = place.longitude;
                    //loc.latitude = place.latitude;
                    //loc.Level1.Id = place.Level1.Id;
                    loc.Level2.Id = place.Level2.Id;
                    //loc.Level3.Text.en_GB = place.Level3.Text.en_GB;
                    //loc.Level5.Id = place.Level5.Id;
                }
                else
                {
                    return new PosterActionResult<Listing>()
                    {
                        Status = PosterActionResultStatus.Error,
                        Message = $"find building error ,data :{location.variables.input.location.Level2.Text.en_GB},{location.variables.input.location.Level1.Id},message :{buildingsResult.Message}"
                    };
                }
            }




            if (place.buildingFacilities != null && place.buildingFacilities.Count > 1)
            {
                location.variables.input.buildingFacilityCodes = place.buildingFacilities.Select(q => q.Code).ToList();
                location.variables.shouldExtendsFields = true;
            }
            location.variables.input.buildingFacilityCodes = null;
            location.extensions.persistedQuery.Sha256Hash = addListingMutationDto.extensions.persistedQuery.Sha256Hash;

            return await GetPolicy<Listing>().ExecuteAsync(async (ctx) =>
             {
                 var result = await AjaxJsonPostAsync("https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo", $"https://www.iproperty.com.my/pro/add-listing/location/{listingId}", data: JsonConvert.SerializeObject(location, jsonSerialzerSettings));

                 if (result.Contains("PersistedQueryNotFound"))
                 {
                     throw new Exception("PersistedQueryNotFound");
                 }
                 var listing = JsonConvert.DeserializeObject<ResponseData<UpdateListingPayload>>(result);
                 return new PosterActionResult<Listing>()
                 {
                     Data = listing.Data.updateListing.listing,
                     Status = PosterActionResultStatus.Success
                 };
             }, new Context("location"));
        }

        public async Task<PosterActionResult> Login()
        {
            var loginUrl = "https://www.iproperty.com.my/pro/listings?lang=en-GB";
            await chromiumWebBrowser.LoadUrlAsync("https://www.baidu.com");
            await PublishMessageAsync("DeleteCookie");
            //var cookieManager = chromiumWebBrowser.GetCookieManager();
            //await cookieManager.DeleteCookiesAsync();
            await PublishMessageAsync($"LoadUrl:{loginUrl}");
            await chromiumWebBrowser.LoadUrlAsync(loginUrl);
            await watiForIsLoading();
            DevToolsContext = await chromiumWebBrowser.CreateDevToolsContextAsync();
            chromiumWebBrowser.ShowDevTools();

            //await Delay(60);

            //check page
            var checkPageResult = await CheckPage();
            if (checkPageResult.Status != PosterActionResultStatus.Success)
                return checkPageResult;
            //input login user name
            //var userNameInput = await DevToolsContext.QuerySelectorAsync<HtmlElement>("#login-userid");
            //await Delay();
            //await userNameInput.SetAttributeAsync("value", propnexTask.Account);
            ////input password
            //var userPasswordInput = await DevToolsContext.QuerySelectorAsync<HtmlElement>("#login-password");
            //await Delay();
            //await userPasswordInput.SetAttributeAsync("value", propnexTask.Password);
            ////login button
            //var loginButton = await DevToolsContext.QuerySelectorAsync<HtmlElement>("#btn_login");
            //await Delay(1);
            //await loginButton.ClickAsync();
            //await Delay();

            await watiForIsLoading();
            await PublishMessageAsync("Login success");
            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success
            };
        }

        public async Task<PosterActionResult<List<Listing>>> GetListings()
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/listingsQuery?" +
                $"operationName=listingsQuery&variables=%7B%22shouldExtendsFields%22%3Atrue%2C%22statusCode%22%3A{2}%2C%22isExcludeChild%22%3Afalse%2C%22sortBy%22%3A%22new-to-old%22%2C%22limit%22%3A500%2C%22page%22%3A%221%22%2C%22includeReAdvertiseJob%22%3Atrue%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%228893c19fbd672297adbdd3bf3eba0c22544d6ef0517a2c3153f36b2c64f86659%22%7D%7D";

            return await GetPolicy<List<Listing>>().ExecuteAsync(async (ctx) =>
            {
                string jscode = $@"()=> {{return fetch(""{url}"", {{
                                    ""headers"": {{
                                        ""accept"": ""application/json, text/plain, */*"",
                                        ""if-none-match"": ""W/\""42c3f-i6z2s6ipfF/j1sd6HDcrj3E+{new Random(_lock.GetHashCode()).Next(100, 999)}\"""",
                                    }},
                                    ""method"": ""GET"",
                                    ""mode"": ""cors"",
                                    ""credentials"": ""include""
                                }}).then(res=>{{
                                      return res.text()
                                }})}}";
                var result = await DevToolsContext.EvaluateFunctionAsync<string>(jscode);
                if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                {
                    throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                }
                _logger?.Information(result);
                var jsonResult = JsonConvert.DeserializeObject<ResponseData<ListingsData>>(result);
                //await Delay(60);
                return new PosterActionResult<List<Listing>>()
                {
                    Data = jsonResult.Data.listings.Data,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("GetListings"));

        }
        private AsyncPolicyWrap<PosterActionResult<T>> GetPolicy<T>()
        {
            var retryPolicy = Policy<PosterActionResult<T>>
           .Handle<Exception>()
           .WaitAndRetryAsync(5, retryNumber => TimeSpan.FromSeconds(60), async (exception, timeSpan, retryCount, context) =>
           {
               await PublishMessageAsync($"retry count {retryCount}, exctption {exception.Exception.Message}");
           });

            var fallbackPolicy = Policy<PosterActionResult<T>>
           .Handle<Exception>()
           .FallbackAsync(fallbackAction: async (c) =>
           {
               return await Task.Run<PosterActionResult<T>>(() =>
               {
                   return new PosterActionResult<T>()
                   {
                       Status = PosterActionResultStatus.Error
                   };
               });
           });
            return Policy.WrapAsync(fallbackPolicy, retryPolicy);
        }


        public async Task<PosterActionResult<List<PlaceDto>>> BuildingQuery(string key)
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/buildingQuery?" +
                $"operationName=buildingQuery&" +
                $"variables=%7B%22q%22%3A%22level5%22%2C%22shouldExtendsFields%22%3Atrue%2C%22includeBuildingFacilityCodes%22%3Atrue%2C%22keyword%22%3A%22{key}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%229adf6bae0454ed8c07e24e43e1f2b622d0649a9fb96a056659ab644ee2b06e63%22%7D%7D";
            return await GetPolicy<List<PlaceDto>>().ExecuteAsync(async (ctx) =>
            {
                var result = await AjaxJsonGetAsync(url);
                var resultData = JsonConvert.DeserializeObject<ResponseData<BuildingRequestData>>(result);
                return new PosterActionResult<List<PlaceDto>>()
                {
                    Data = resultData.Data.places.Data,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("BuildingQuery"));
        }

        public async Task<PosterActionResult<List<PlaceDto>>> level2Query(string name, string id)
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/level2Query?operationName=level2Query&variables=%7B%22keyword%22%3A%22{System.Web.HttpUtility.UrlEncode(name)}%22%2C%22level1Id%22%3A%22{id}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%2250af390e2fb55a7d6b9d8497c7062017baa2976c23106a571b08125ed31b7242%22%7D%7D";

            return await GetPolicy<List<PlaceDto>>().ExecuteAsync(async (ctx) =>
            {
                var result = await AjaxJsonGetAsync(url);
                var resultData = JsonConvert.DeserializeObject<ResponseData<BuildingRequestData>>(result);
                return new PosterActionResult<List<PlaceDto>>()
                {
                    Data = resultData.Data.places.Data,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("level2Query"));


        }

        public async Task<PosterActionResult> CheckPage()
        {
            var gRecaptcha = await DevToolsContext.QuerySelectorAsync(".g-recaptcha");
            if (gRecaptcha != null)
            {
                return new PosterActionResult()
                {
                    Status = PosterActionResultStatus.Error,
                    Message = "g-recaptcha"
                };
            }

            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success
            };
        }

        /// <summary>
        /// 获取post 数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="referrerUrl"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task<string> AjaxJsonPostAsync(string url, string referrerUrl, string type = "POST", string data = "")
        {
            string jscode = $"()=> fetch('{url}',{{ method:\"{type}\",referrer:'{referrerUrl}',headers:{{'content-type': 'application/json'}},body:{(data == "" ? "''" : "JSON.stringify(" + data.Replace('\"', '"') + ")")}}}) .then(response => response.text())";
            string result = "";
            try
            {
                result = await DevToolsContext.EvaluateFunctionAsync<string>(jscode);
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }

        /// <summary>
        /// 获取json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task<string> AjaxJsonGetAsync(string url)
        {
            try
            {
                string jscode = $@"()=> {{return fetch(""{url}"", {{
                                  ""headers"": {{
                                    ""accept"": ""application/json, text/plain, */*"",
                                  }},
                                  ""method"": ""GET"",
                                  ""mode"": ""cors""
                                }}).then(res=>{{
                                      return res.text()
                                }})}}";

                var result = await DevToolsContext.EvaluateFunctionAsync<string>(jscode);

                return result;
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        private async Task watiForIsLoading()
        {
            while (chromiumWebBrowser.IsLoading)
            {
                await Delay();
                await PublishMessageAsync($"Waiting loading {chromiumWebBrowser.IsLoading}");
            }
            await Delay();
        }

        public async Task Delay(int delay = 5)
        {
            await Task.Delay(delay * 1000);
        }

        public async Task GetTask()
        {

            //propnexTasks = _propnexTaskProvider.GetTasks(System.IO.File.ReadAllText("E:\\114.cef.tsk"));
            //if (propnexTasks == null)
            //{
            //    await PublishMessageAsync("Not find tasks ,dealy 1 min");
            //    await Task.Delay(1000 * 60);
            //    Close();
            //}
            //return;
            var context = "";
            var pnTask = await WebServer.GetTask();
            if (pnTask != null)
            {
                context = await WebServer.GetTaskContent(pnTask);
                //propnexTasks = _propnexTaskProvider.GetTasks(System.IO.File.ReadAllText("E:\\111.cef.tsk"));
                propnexTasks = _propnexTaskProvider.GetTasks(context);
                if (propnexTasks == null)
                {
                    await PublishMessageAsync("Not find tasks ,dealy 1 min");
                    await Task.Delay(1000 * 60);
                    Close();
                }
            }
            else
            {
                await PublishMessageAsync("Not find tasks ,dealy 1 min");
                await Task.Delay(1000 * 60);
                Close();
            }
        }

        public async Task PublishMessageAsync(string message)
        {
            _logger?.Information(message);
            await _localEventBus.PublishAsync(new LogEvent()
            {
                Message = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}-{message}"
            });
        }
    }
}