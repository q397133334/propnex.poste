using CefSharp;
using CefSharp.Dom;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Wrap;
using Propnex.Poster.Dtos;
using Propnex.Poster.IProperty;
using Propnex.Poster.Share;
using PropnexPoster.NetCoreWinForm;
using RestSharp;
using Serilog;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using Extensions = Propnex.Poster.IProperty.Extensions;
using HtmlElement = CefSharp.Dom.HtmlElement;

namespace Propnex.Poster.NetCoreWinForm
{
    public partial class IPropertyCefPoster : CefFrom, ITransientDependency, IPosterStart
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IPropnexTaskProvider _propnexTaskProvider;
        private ILogger? _logger;

        private PropnexTasks propnexTasks;
        private PropnexTask propnexTask;
        private List<Listing> Listings = new List<Listing>();

        private RequestData<Variables<AddListingMutationDto>>? addListingMutationDto;
        private RequestData<Variables<LocationDto>> locationDto;
        private RequestData<Variables<PropertyDetailsDto>> propertyDetailsDto;

        public Task<DevToolsContext> DevToolsContext
        {
            get
            {
                return chromiumWebBrowser.CreateDevToolsContextAsync();
            }
        }


        CancellationToken cancellationToken = new CancellationToken();


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

        PnTaskDto PnTaskDto;

        public override async Task StartAsync()
        {
            await Delay(5);
            var toLoginResult = await ToLoginAsync();
#if DEBUG
            chromiumWebBrowser.ShowDevTools();
#endif
            try
            {
                if (toLoginResult.Status == PosterActionResultStatus.Error)
                {
                    await PublishMessageAsync(toLoginResult.Message);
                    await Delay(60 * 5);
                    Close();
                    return;
                }
                await PublishMessageAsync("Start a new task");
                await GetTaskAsync();
                while (PnTaskDto == null)
                {
                    await PublishMessageAsync("Not find task, delay 5 min");
                    await Delay(60 * 5);
                    await GetTaskAsync();
                }
                _logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.Async(c => c.File($"{Directory.GetDirectoryRoot(System.AppDomain.CurrentDomain.BaseDirectory)}\\logs\\task\\{PnTaskDto.Number}.MyIP.txt"))
                            .CreateLogger();

                foreach (var item in propnexTasks.Tasks)
                {
                    propnexTask = item;
                    var loginResult = await LoginAsync();
                    if (loginResult.Status != PosterActionResultStatus.Success)
                    {
                        await PublishMessageAsync(loginResult.Message);
                        foreach (var task in propnexTask.Listings.Listings)
                        {
                            await xwebItemAsync(loginResult, task);
                        }
                        await XwebEndAsync(loginResult.Message);
                        break;
                    }
                    var listingResult = await GetListings();
                    if (listingResult.Status != PosterActionResultStatus.Success)
                    {
                        //await PublishMessageAsync(listingResult.Message);
                        foreach (var task in propnexTask.Listings.Listings)
                        {
                            await xwebItemAsync(listingResult.ToPosterActionResult(), task);
                        }
                        await XwebEndAsync();
                        break;
                    }
                    else
                    {
                        Listings = listingResult.Data;
                    }
                    if (item.TaskType == "Post Only")
                    {
                        await PostOnlyAsync();
                    }
                    if (item.TaskType.ToLower() == "repost")
                    {
                        await RepostAsync();
                    }
                    if (item.TaskType.ToLower() == "update")
                    {
                        await UpdateAsync();
                    }
                    if (item.TaskType.ToLower() == "remove from portals")
                    {
                        await DeleteAsync();
                    }
                    if (item.TaskType.ToLower().IndexOf("retrieve") > -1)
                    {
                        await RetrieveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await PublishMessageAsync(ex.Message);
            }
            finally
            {
                (_logger as Serilog.Core.Logger)?.Dispose();
                Close();
            }
        }

        public async Task PostOnlyAsync()
        {
            foreach (var task in propnexTask.Listings.Listings)
            {
                var result = await CreateListingAsync(task);
                await xwebItemAsync(result, task);
            }
            await XwebEndAsync();
        }

        public async Task UpdateAsync()
        {
            foreach (var task in propnexTask.Listings.Listings)
            {
                var result = await UpdateListingAsync(task);
                await xwebItemAsync(result, task);
            }
            await XwebEndAsync();
        }

        public async Task DeleteAsync()
        {
            foreach (var task in propnexTask.Listings.Listings)
            {
                var result = await DeleteListing(task);
                await xwebItemAsync(result, task);
            }
            await XwebEndAsync();
        }

        public async Task RepostAsync()
        {
            foreach (var task in propnexTask.Listings.Listings)
            {
                var result = await ReportListing(task);
                await xwebItemAsync(result, task);
            }
            await XwebEndAsync();
        }

        public async Task RetrieveAsync()
        {
            await Task.CompletedTask;
        }



        public async Task<PosterActionResult> CreateListingAsync(PropnexListing propnexListing)
        {
            string addListingMutationUrl = "https://www.iproperty.com.my/pro/rasor/graphql/addListingMutation";
            string? listingId = "";

            var resultListingDetails = await listingDetails();
            if (resultListingDetails.Status == PosterActionResultStatus.Success)
                listingId = resultListingDetails.Data;
            else
            {
                await PublishMessageAsync(resultListingDetails.Message);
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultListingDetails.Message,
                    Status = PosterActionResultStatus.Error
                };

            }

            var resultLocation = await location(propnexListing, listingId);
            if (resultLocation.Status == PosterActionResultStatus.Error)
            {
                await PublishMessageAsync(resultLocation.Message);
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultLocation.Message,
                    Status = PosterActionResultStatus.Error
                };
            }

            var resultProperytDetails = await propertyDetails(propnexListing, listingId);
            if (resultProperytDetails.Status == PosterActionResultStatus.Error)
            {
                await PublishMessageAsync(resultProperytDetails.Message);
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultProperytDetails.Message,
                    Status = PosterActionResultStatus.Error
                };
            }


            var resultDescriptionMedial = await descriptionMedial(propnexListing, listingId);
            if (resultDescriptionMedial.Status == PosterActionResultStatus.Error)
            {
                await PublishMessageAsync(resultDescriptionMedial.Message);
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultDescriptionMedial.Message,
                    Status = PosterActionResultStatus.Error
                };
            }


            var resultUpgradePublish = await upgradePublish(propnexListing, listingId);

            return new PosterActionResult()
            {
                Data = listingId,
                Message = resultUpgradePublish.Message
            };

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
                    if (result.Contains("errors"))
                    {
                        throw new Exception(result);
                    }
                    //_logger.Information(result);
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
        }

        public async Task<PosterActionResult> UpdateListingAsync(PropnexListing propnexListing)
        {
            var listingId = "";

            var listing = matchListing(propnexListing);
            if (listing == null)
            {
                return new PosterActionResult()
                {
                    Data = "",
                    Message = "Oops, we can��t find and match the above listing to perform any action. Please check your guru direct as you could have modified previously."
                };
            }
            else
            {
                listingId = listing.Id;
            }

            var refUrl = $"https://www.iproperty.com.my/pro/edit-listing/listing-details/{listingId}";
            var resultListingDetails = await listingDetails();
            if (resultListingDetails.Status == PosterActionResultStatus.Success)
                listingId = resultListingDetails.Data;
            else
            {
                await PublishMessageAsync(resultListingDetails.Message);
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultListingDetails.Message,
                    Status = PosterActionResultStatus.Error
                };

            }

            var resultLocation = await location(propnexListing, listingId, $"https://www.iproperty.com.my/pro/edit-listing/location/{listingId}");
            if (resultLocation.Status == PosterActionResultStatus.Error)
            {
                await PublishMessageAsync(resultLocation.Message);
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultLocation.Message,
                    Status = PosterActionResultStatus.Error
                };
            }

            var resultProperytDetails = await propertyDetails(propnexListing, listingId, $"https://www.iproperty.com.my/pro/edit-listing/property-details/{listingId}");
            if (resultProperytDetails.Status == PosterActionResultStatus.Error)
            {
                await PublishMessageAsync(resultProperytDetails.Message);
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultProperytDetails.Message,
                    Status = PosterActionResultStatus.Error
                };
            }


            var resultDescriptionMedial = await descriptionMedial(propnexListing, listingId, $"https://www.iproperty.com.my/pro/edit-listing/description-and-media/{listingId}");
            if (resultDescriptionMedial.Status == PosterActionResultStatus.Error)
            {
                await PublishMessageAsync(resultDescriptionMedial.Message);
                return new PosterActionResult()
                {
                    Data = listingId,
                    Message = resultDescriptionMedial.Message,
                    Status = PosterActionResultStatus.Error
                };
            }
            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success,
                Message = resultDescriptionMedial.Message
            };
            async Task<PosterActionResult<string>> listingDetails()
            {
                propnexListing.Details["data_step1"] = propnexListing.Details["data_step1"].Replace("\"location\":[]", "\"location\":{}");
                addListingMutationDto = JsonConvert.DeserializeObject<RequestData<Variables<AddListingMutationDto>>>(propnexListing.Details["data_step1"]);
                var updateListingMutationDto = JsonConvert.DeserializeObject<RequestData<Variables<UpdateListingMutationDto>>>(propnexListing.Details["data_step1"]);
                updateListingMutationDto.variables.input.id = listingId;
                updateListingMutationDto.OperationName = "updateListingInfo";
                updateListingMutationDto.variables.shouldExtendsFields = true;
                updateListingMutationDto.extensions.persistedQuery.Sha256Hash = "338f5cae02e4a1a1514a145cf448909fbe092045dc524f11db5d41572990dcdf";
                string jsonData = JsonConvert.SerializeObject(updateListingMutationDto, jsonSerialzerSettings);


                return await GetPolicy<string>().ExecuteAsync(async (ctx) =>
                {
                    var result = await AjaxJsonPostAsync("https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo", "refUrl", data: jsonData);
                    if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                    {
                        await PublishMessageAsync(result);
                        throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                    }
                    if (result.Contains("errors"))
                    {
                        throw new Exception(result);
                    }
                    _logger.Information(result);
                    var data = JsonConvert.DeserializeObject<ResponseData<UpdateListingPayload>>(result);
                    listingId = data?.Data.updateListing.listing.Id;
                    await PublishMessageAsync($"update listing details success,listing is is {listingId}");
                    return new PosterActionResult<string>()
                    {
                        Data = listingId,
                        Status = PosterActionResultStatus.Success
                    };

                }, new Context("listingDetails"));
            }
        }


        public async Task<PosterActionResult> ReportListing(PropnexListing propnexListing)
        {
            var listingId = "";

            var listing = matchListing(propnexListing);
            if (listing == null)
            {
                return new PosterActionResult()
                {
                    Data = "",
                    Message = "Oops, we can��t find and match the above listing to perform any action. Please check your guru direct as you could have modified previously."
                };
            }
            else
            {
                listingId = listing.Id;
            }

            var updateResult = await UpdateListingAsync(propnexListing);

            if (updateResult.Status == PosterActionResultStatus.Error)
            {
                return updateResult;
            }

            var listingQuoteQueryResult = await listingQuoteQueryReAdvertise(listingId);

            if (listingQuoteQueryResult.Status == PosterActionResultStatus.Error)
            {
                return listingQuoteQueryResult.ToPosterActionResult();
            }

            var quoteId = listingQuoteQueryResult.Data;
            await GetPolicy<string>().ExecuteAsync(async (ctx) =>
            {
                var url = "https://www.iproperty.com.my/pro/rasor/graphql/ReAdvertiseListing";
                var body = "{\"operationName\":\"ReAdvertiseListing\",\"variables\":{\"input\":{\"id\":\"" + listingId + "\",\"quoteId\":\"" + quoteId + "\"}},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"ca45ee989a023f060ca9f77a01a61cbe126d645f267c5fa589f7878dcf2c86e7\"}}}";

                var result = await AjaxJsonPostAsync(url, "https://www.iproperty.com.my/pro/listings", data: body);

                if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                {
                    await PublishMessageAsync(result);
                    throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
                _logger.Information(result);
                var data = JsonConvert.DeserializeObject<ResponseData<UpdateListingPayload>>(result);
                await PublishMessageAsync($"update listing details success,listing is is {listingId}");
                return new PosterActionResult<string>()
                {
                    Status = PosterActionResultStatus.Success
                };

            }, new Context("ReportListing"));


            async Task<PosterActionResult<string>> listingQuoteQueryReAdvertise(string listingId)
            {
                var url = $"https://www.iproperty.com.my/pro/rasor/graphql/ListingQuoteQueryReAdvertise?" +
                    "operationName=ListingQuoteQueryReAdvertise&" +
                    $"variables=%7B%22ids%22%3A%5B%22{listingId}%22%5D%7D&" +
                    "extensions=%7B%22" +
                    "persistedQuery%22%3A%7B%22" +
                    "version%22%3A1%2C%22sha256" +
                    "Hash%22%3A%22830aa402b183a13726b1ae434d16d8df0010a6df3f5024ff17fb7a9a33c95fc4%22%7D%7D";

                return await GetPolicy<string>().ExecuteAsync(async (ctx) =>
                {
                    var result = await AjaxJsonGetAsync(url);
                    if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                    {
                        await PublishMessageAsync(result);
                        throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                    }
                    if (result.Contains("errors"))
                    {
                        throw new Exception(result);
                    }

                    JObject jsonObjext = JObject.Parse(result);
                    var data = jsonObjext["data"]["listingQuotes"]["reAdvertise"] as JArray;

                    if (data.Count > 0)
                    {

                    }
                    return new PosterActionResult<string>()
                    {
                        Data = data[0]["quoteId"].ToString(),
                        Status = PosterActionResultStatus.Success
                    };

                }, new Context("listingQuoteQueryReAdvertise"));
            }
            return new PosterActionResult();
        }

        public async Task<PosterActionResult> DeleteListing(PropnexListing propnexListing)
        {
            var listingId = "";

            var listing = matchListing(propnexListing);
            if (listing == null)
            {
                return new PosterActionResult()
                {
                    Data = "",
                    Message = "Oops, we can��t find and match the above listing to perform any action. Please check your guru direct as you could have modified previously."
                };
            }
            else
            {
                listingId = listing.Id;
            }

            var url = "https://www.iproperty.com.my/pro/rasor/graphql/offlineListing";
            var body = "{\"operationName\":\"offlineListing\",\"variables\":{\"input\":{\"id\":\"" + listingId + "\"}},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"fbcd4ad7faece70b109e1bdc728762bafedf8c1db7c39478c62c31a9001dab5e\"}}}";

            await GetPolicy<string>().ExecuteAsync(async (ctx) =>
            {
                var result = await AjaxJsonPostAsync(url, $"https://www.iproperty.com.my/pro/listing/overview/{listingId}", data: body);
                if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                {
                    await PublishMessageAsync(result);
                    throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
                //_logger?.Information(result);
                return new PosterActionResult<string>()
                {
                    Data = result,
                    Message = $"offline listing success {listingId}"
                };

            }, new Context("DeleteListing"));

            return new PosterActionResult();
        }


        public async Task<PosterActionResult<Listing>> propertyDetails(PropnexListing propnexListing, string listingId, string action = "")
        {
            var refUrl = $"https://www.iproperty.com.my/pro/add-listing/location/{listingId}";
            if (string.IsNullOrEmpty(action) == false)
            {
                refUrl = action;
            }

            var result = await GetPolicy<Listing>().ExecuteAsync(async (ctx) =>
            {
                propnexListing.Details["data_details"] = propnexListing.Details["data_details"].Replace("\"location\":[]", "\"location\":{}");
                propertyDetailsDto = JsonConvert.DeserializeObject<RequestData<Variables<PropertyDetailsDto>>>(propnexListing.Details["data_details"]);
                var input = propertyDetailsDto.variables.input;
                input.id = listingId;
                propertyDetailsDto.extensions.persistedQuery.Sha256Hash = "338f5cae02e4a1a1514a145cf448909fbe092045dc524f11db5d41572990dcdf";
                if (input.photo360s.Count == 0)
                {
                    input.photo360s = null;
                }
                if (input.images.Count == 0)
                {
                    input.images = null;
                }
                if (input.floorPlans.Count == 0)
                {
                    input.floorPlans = null;
                }
                var result = await AjaxJsonPostAsync("https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo", refUrl, data: JsonConvert.SerializeObject(propertyDetailsDto, jsonSerialzerSettings));

                if (result.Contains("PersistedQueryNotFound"))
                {
                    throw new Exception("PersistedQueryNotFound");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
                var listing = JsonConvert.DeserializeObject<ResponseData<UpdateListingPayload>>(result);
                await PublishMessageAsync($"propertyDetails success,{listingId}");
                return new PosterActionResult<Listing>()
                {
                    Data = listing.Data.updateListing.listing,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("propertyDetails"));

            return result;

        }

        public async Task<PosterActionResult<Listing>> location(PropnexListing propnexListing, string listingId, string action = "")
        {
            var refUrl = $"https://www.iproperty.com.my/pro/add-listing/location/{listingId}";
            if (string.IsNullOrEmpty(action) == false)
            {
                refUrl = action;
            }
            //1.����location����
            locationDto = JsonConvert.DeserializeObject<RequestData<Variables<LocationDto>>>(propnexListing.Details["data_location"].Replace("\"extension\":[]", "\"extension\":{}"));
            locationDto.variables.input.id = listingId;
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
                if (string.IsNullOrEmpty(propnexListing.Basic["txtPostCode"]))
                {
                    return new PosterActionResult<Listing>()
                    {
                        Status = PosterActionResultStatus.Error,
                        Message = $"PostCode is null"
                    };
                }
                var buildingsResult = await BuildingQuery(buildingText, listingId);
                if (buildingsResult.Status == PosterActionResultStatus.Success)
                {
                    var buildings = buildingsResult.Data;
                    place = buildings.FirstOrDefault(q => q.postCode == propnexListing.Basic["txtPostCode"] || q.Level5.Text.en_GB == buildingText);
                    if (place == null)
                    {
                        //    buildingsResult = await level2Query(locationDto.variables.input.location.Level2.Text.en_GB, locationDto.variables.input.location.Level1.Id);
                        //    if (buildingsResult.Status == PosterActionResultStatus.Success)
                        //    {
                        //        buildings = buildingsResult.Data;
                        //        if (buildings.Count == 0)
                        //        {
                        //            return new PosterActionResult<Listing>()
                        //            {
                        //                Status = PosterActionResultStatus.Error,
                        //                Message = $"Not find building,data :{locationDto.variables.input.location.Level2.Text.en_GB},{locationDto.variables.input.location.Level1.Id}"
                        //            };
                        //        }
                        //        place = buildings.FirstOrDefault();
                        //        if(string.IsNullOrEmpty(place.Level2.Text.en_GB)==false)
                        //        {
                        //            buildingsResult = await BuildingQuery(place.Level2.Text.en_GB);
                        //            buildings = buildingsResult.Data;
                        //            place = buildings.FirstOrDefault(q => q.postCode == propnexListing.Basic["txtPostCode"]);
                        //            if(place==null)
                        //            {
                        //                return new PosterActionResult<Listing>()
                        //                {
                        //                    Status = PosterActionResultStatus.Error,
                        //                    Message = $"Not find building,data:{buildingText}"
                        //                };
                        //            }
                        //        }
                        //    }
                        //    else
                        //{
                        return new PosterActionResult<Listing>()
                        {
                            Status = PosterActionResultStatus.Error,
                            Message = $"Not find building,data:{buildingText}"
                        };
                    }
                }

                var input = locationDto.variables.input;
                var loc = locationDto.variables.input.location;
                loc.Address.en_GB = place.Address.en_GB;
                input.propertyGroupTypeCode = place.propertyGroupType.Code;
                input.propertyTypeCode = place.propertyType.Code;
                loc.postalCode = place.postCode;
                loc.longitude = place.longitude;
                loc.latitude = place.latitude;
                loc.Level1.Id = place.Level1.Id;
                loc.Level2.Id = place.Level2.Id;
                loc.Level3.Text.en_GB = place.Level3.Text.en_GB;
                loc.Level5.Id = place.Level5.Id;
            }
            else
            {
                //get level2 id 
                var buildingsResult = await level2Query(locationDto.variables.input.location.Level2.Text.en_GB, locationDto.variables.input.location.Level1.Id);
                if (buildingsResult.Status == PosterActionResultStatus.Success)
                {
                    var buildings = buildingsResult.Data;
                    if (buildings.Count == 0)
                    {
                        return new PosterActionResult<Listing>()
                        {
                            Status = PosterActionResultStatus.Error,
                            Message = $"Not find building,data :{locationDto.variables.input.location.Level2.Text.en_GB},{locationDto.variables.input.location.Level1.Id}"
                        };
                    }

                    place = buildings.FirstOrDefault();

                    var input = locationDto.variables.input;
                    var loc = locationDto.variables.input.location;
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
                        Message = $"find building error ,data :{locationDto.variables.input.location.Level2.Text.en_GB},{locationDto.variables.input.location.Level1.Id},message :{buildingsResult.Message}"
                    };
                }
            }

            if (place.buildingFacilities != null && place.buildingFacilities.Count > 1)
            {
                locationDto.variables.input.buildingFacilityCodes = place.buildingFacilities.Select(q => q.Code).ToList();
                locationDto.variables.shouldExtendsFields = true;
            }
            locationDto.variables.input.buildingFacilityCodes = null;
            locationDto.extensions.persistedQuery.Sha256Hash = "338f5cae02e4a1a1514a145cf448909fbe092045dc524f11db5d41572990dcdf";
            if (locationDto.variables.input.location.block != null && locationDto.variables.input.location.block.Length > 29)
                locationDto.variables.input.location.block = locationDto.variables.input.location.block.Substring(0, 29);
            return await GetPolicy<Listing>().ExecuteAsync(async (ctx) =>
            {
                locationDto.variables.input.location.Level5.Text = null;
                var jsonstr = JsonConvert.SerializeObject(locationDto, jsonSerialzerSettings);
                var result = await AjaxJsonPostAsync("https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo", refUrl, data: jsonstr);

                if (result.Contains("PersistedQueryNotFound"))
                {
                    throw new Exception("PersistedQueryNotFound");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
                var listing = JsonConvert.DeserializeObject<ResponseData<UpdateListingPayload>>(result);
                await PublishMessageAsync($"location success,{listingId}");
                return new PosterActionResult<Listing>()
                {
                    Data = listing.Data.updateListing.listing,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("location"));
        }

        public async Task<PosterActionResult<Listing>> descriptionMedial(PropnexListing propnexListing, string listingId, string action = "")
        {
            var refUrl = $"https://www.iproperty.com.my/pro/add-listing/location/{listingId}";
            if (string.IsNullOrEmpty(action) == false)
            {
                refUrl = action;
            }
            await (await DevToolsContext).EvaluateExpressionAsync(@"window.base64ToFile=function (dataurl, filename) { 
	                var arr = dataurl.split(','),
	                    mime = arr[0].match(/:(.*?);/)[1],
	                    bstr = atob(arr[1]),
	                    n = bstr.length,
	                    u8arr = new Uint8Array(n);
	                while (n--) {
	                    u8arr[n] = bstr.charCodeAt(n);
	                }
	                return new File([u8arr], filename, { type: mime });
	            }");

            propnexListing.Details["data_media"] = propnexListing.Details["data_media"].Replace("\"location\":[]", "\"location\":{}").Replace("\"extension\":[]", "\"extension\":{}");
            var descriptionMedialDto = JsonConvert.DeserializeObject<RequestData<Variables<DescriptionMedialDto>>>(propnexListing.Details["data_media"]);

            if (propnexListing.Details.ContainsKey("listing_title"))
            {
                descriptionMedialDto.variables.input.title = new ListingMultiLangText()
                {
                    en_GB = propnexListing.Details["listing_title"]
                };
                if (descriptionMedialDto.variables.input.title.en_GB == "")
                {
                    descriptionMedialDto.variables.input.title.en_GB = "Please call me";
                }
            }
            else
            {
                descriptionMedialDto.variables.input.title = new ListingMultiLangText()
                {
                    en_GB = "Please call me"
                };
            }
            descriptionMedialDto.variables.input.title.en_GB = descriptionMedialDto.variables.input.title.en_GB.Trim('.');
            if (descriptionMedialDto.variables.input.title.en_GB.Length > 30)
            {
                descriptionMedialDto.variables.input.title.en_GB = descriptionMedialDto.variables.input.title.en_GB.Substring(0, 29);
            }

            var input = descriptionMedialDto.variables.input;

            for (int i = 0; i < propnexListing.Photos.Count; i++)
            {
                string[] ss = propnexListing.Photos[i].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                await PublishMessageAsync($"uploadimage {i + 1}");
                var image = await uploadImage(ss[0].Trim());
                if (image != null)
                {
                    input.images.Add(new DescriptionMedialPhotoDto()
                    {
                        path = image.storage.Key,
                        fullPath = image.storage.Location,
                        width = image.image.width,
                        height = image.image.height,
                        id = $"rc-upload-{DateTime.Now.Ticks}-{i + 1}"
                    });
                }

                if (i == 39)
                    break;
            }

            for (int i = 0; i < propnexListing.FloorPlan.Count; i++)
            {
                string[] ss = propnexListing.FloorPlan[i].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                await PublishMessageAsync($"uploadfloorplan {i + 1}");
                var image = await uploadImage(ss[0].Trim());
                if (image != null)
                {
                    input.floorPlans.Add(new DescriptionMedialPhotoDto()
                    {
                        path = image.storage.Key,
                        fullPath = image.storage.Location,
                        width = image.image.width,
                        height = image.image.height,
                        id = $"rc-upload-{DateTime.Now.Ticks}-{i}"
                    });
                }

                if (i == 39)
                    break;
            }

            for (int i = 0; i < propnexListing.Videos.Count; i++)
            {

                input.videos.Add(new { url = propnexListing.Videos[i] });
                if (i == 39)
                    break;
            }

            return await GetPolicy<Listing>().ExecuteAsync(async (ctx) =>
             {
                 var input = descriptionMedialDto.variables.input;
                 input.id = listingId;
                 descriptionMedialDto.extensions.persistedQuery.Sha256Hash = "338f5cae02e4a1a1514a145cf448909fbe092045dc524f11db5d41572990dcdf";
                 var result = await AjaxJsonPostAsync("https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo", refUrl, data: JsonConvert.SerializeObject(descriptionMedialDto, jsonSerialzerSettings));

                 if (result.Contains("PersistedQueryNotFound"))
                 {
                     throw new Exception("PersistedQueryNotFound");
                 }
                 if (result.Contains("errors"))
                 {
                     throw new Exception(result);
                 }
                 var listing = JsonConvert.DeserializeObject<ResponseData<UpdateListingPayload>>(result);
                 await PublishMessageAsync($"descriptionMedial success,{listingId}");
                 return new PosterActionResult<Listing>()
                 {
                     Data = listing.Data.updateListing.listing,
                     Status = PosterActionResultStatus.Success
                 };
             }, new Context("descriptionMedial"));

            async Task<ResponseImageDto> uploadImage(string url)
            {

                var path = $"{System.IO.Path.Combine(Directory.GetDirectoryRoot(System.AppDomain.CurrentDomain.BaseDirectory), "task", PnTaskDto.Number)}";
                if (Directory.Exists($"{path}") == false)
                    Directory.CreateDirectory(path);
                var guid = Guid.NewGuid().ToString().Replace("-", "");
                var fileName = $"{guid}.jpg";
                var filePath = $"{Path.Combine(path, fileName)}";
                byte[]? file = null;
                using (RestClient client = new RestClient())
                {
                    RestRequest request = new RestRequest();
                    request.Timeout = 6000;
                    request.Method = Method.Get;
                    request.Resource = url;
                    file = await client.DownloadDataAsync(request).ConfigureAwait(false);
                }
                if (file != null)
                {
                    try
                    {
                        string dataString = "data:image/jpeg;base64," + Convert.ToBase64String(file);
                        await (await DevToolsContext).EvaluateFunctionAsync($"(value)=>{{ window.file_{guid}=window.base64ToFile(value,'{fileName}')}}", dataString);
                        StringBuilder sb = new StringBuilder();
                        sb.Append("var fd= new FormData();");
                        sb.Append($"fd.append('photo',window.file_{guid});");

                        sb.Append($"fetch(\"https://www.iproperty.com.my/pro/api/image\", {{ method: \"POST\", \"mode\": \"cors\",\"credentials\": \"include\",body: fd}}).then(response => response.text())");
                        var result = await (await DevToolsContext).EvaluateExpressionAsync<string>(sb.ToString());
                        if (result.Contains("errors") == false)
                        {
                            return JsonConvert.DeserializeObject<ResponseImageDto>(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        await PublishMessageAsync($"upload file error ,{url}" + ex.Message);
                    }
                }
                return null;
            }
        }

        async Task<PosterActionResult<Listing>> upgradePublish(PropnexListing propnexListing, string listingId)
        {

            var listingQuote = await listingQuoteQueryAsync(listingId);
            var quote = listingQuote.Data.quotes.FirstOrDefault(q => q.listingProduct.Id != null && q.listingProduct.Label == "Standard");
            if (quote != null)
            {
                var publishListingInfoDto = new RequestData<Variables<PublishListingInfoDto>>();
                publishListingInfoDto.extensions = new Extensions()
                {
                    persistedQuery = new PersistedQuery() { Sha256Hash = "3ecb2bca14887a97ee2cbd986baf6ba5455605248583f0a66afc7daee4120dc9", Version = 1 }
                };
                publishListingInfoDto.OperationName = "publishListingInfo";
                publishListingInfoDto.variables = new Variables<PublishListingInfoDto>()
                {
                    input = new PublishListingInfoDto()
                    {
                        id = listingId,
                    }
                };
                publishListingInfoDto.variables.input.quotes.Add(new PublishListingInfoQuote()
                {
                    channelCode = addListingMutationDto.variables.input.channelCode.Value,
                    quoteIds = new List<string>() { quote.quoteId }
                });

                await GetPolicy<Listing>().ExecuteAsync(async (ctx) =>
                {
                    publishListingInfoDto.variables.shouldExtendsFields = true;

                    var result = await AjaxJsonPostAsync($"https://www.iproperty.com.my/pro/rasor/graphql/publishListingInfo",
                         $"https://www.iproperty.com.my/pro/add-listing/upgrade-and-review/{listingId}",
                         data: JsonConvert.SerializeObject(publishListingInfoDto, jsonSerialzerSettings)
                         );
                    if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                    {
                        await PublishMessageAsync(result);
                        throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                    }
                    if (result.Contains("errors"))
                    {
                        throw new Exception(result);
                    }
                    await PublishMessageAsync($"create listing details success,listing is is {listingId}");
                    var listing = JsonConvert.DeserializeObject<ResponseData<PublishListingPayload>>(result);
                    return new PosterActionResult<Listing>()
                    {
                        Data = listing.Data.publishListing.listings[0],
                        Status = PosterActionResultStatus.Success
                    };

                }, new Context());
            }

            return new PosterActionResult<Listing>();

            async Task<PosterActionResult<ListingQuoteDto>> listingQuoteQueryAsync(string listingId)
            {
                var url = $"https://www.iproperty.com.my/pro/rasor/graphql/ListingQuoteQuery/{listingId}?" +
                    $"operationName=ListingQuoteQuery&variables=%7B%22id%22%3A%22{listingId}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%2228b8639e7d95e8ba5dce1e3176397c3a56a34f3fd29204fc6c8c5b08729e5720%22%7D%7D";

                return await GetPolicy<ListingQuoteDto>()
                     .ExecuteAsync(async (ctx) =>
                     {
                         var result = await AjaxJsonGetAsync(url);
                         var responseData = JsonConvert.DeserializeObject<ResponseData<Payload<ListingQuoteDto>>>(result);
                         if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                         {
                             await PublishMessageAsync(result);
                             throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                         }
                         if (result.Contains("errors"))
                         {
                             throw new Exception(result);
                         }
                         return new PosterActionResult<ListingQuoteDto>()
                         {
                             Data = responseData.Data.listing,
                             Status = PosterActionResultStatus.Success
                         };
                     }, new Context("ListingQuoteQuery"));
            }
        }

        public Listing matchListing(PropnexListing propnexListing)
        {
            Listing listing = null;
            if (propnexListing.Details.ContainsKey("iproperty_listing_id"))
            {
                listing = Listings.Where(q => q.Id == propnexListing.Details["iproperty_listing_id"]).FirstOrDefault();
            }
            var listingType = "";
            if (propnexListing.ListingType.ToLower() == "sale")
            {
                listingType = "buy";
            }
            if (listing == null)
            {
                listing = Listings.FirstOrDefault(q => (q.Channel.Label.ToLower() == propnexListing.ListingType.ToLower() || q.Channel.Label.ToLower() == listingType) &&
                    (q.Location.Level5.Text.en_GB.Trim().ToLower().StartsWith(propnexListing.ListingName.Trim().ToLower().Replace(".", "")) ||
                     (propnexListing.ListingName.Trim().ToLower().StartsWith(q.Location.Level5.Text.en_GB.Trim().ToLower().Replace(",", "")) &&
                     propnexListing.FloorArea == q.SaleableArea.Value)
                    ));
            }
            return listing;
        }

        public async Task<PosterActionResult> ToLoginAsync()
        {
            int retry = 0;
            while (retry < 5)
            {
                await Delay(10);
                var loginUrl = "https://www.iproperty.com.my/pro/listings?lang=en-GB";
                await chromiumWebBrowser.LoadUrlAsync("https://www.baidu.com");
                await PublishMessageAsync("DeleteCookie");
                var cookieManager = chromiumWebBrowser.GetCookieManager();
                await cookieManager.DeleteCookiesAsync();
                await Delay();
                await PublishMessageAsync($"LoadUrl:{loginUrl}");
                await chromiumWebBrowser.LoadUrlAsync(loginUrl);
                await watiForIsLoading();
                //DevToolsContext = await chromiumWebBrowser.CreateDevToolsContextAsync();
                var loginForm = await (await DevToolsContext).QuerySelectorAsync("#login-form");
                if (loginForm != null)
                    break;
            }
            if (retry == 5)
            {
                var loginForm = await (await DevToolsContext).QuerySelectorAsync("#login-form");
                if (loginForm == null)
                {
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Error,
                        Message = "Not find login form"
                    };
                }
            }
            var checkPageResult = await CheckPage();
            if (checkPageResult.Status == PosterActionResultStatus.Error)
            {
                return checkPageResult;
            }
            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success
            };
        }

        public async Task<PosterActionResult> LoginAsync()
        {
            //var loginUrl = "https://www.iproperty.com.my/pro/listings?lang=en-GB";
            //await chromiumWebBrowser.LoadUrlAsync("https://www.baidu.com");
            //await PublishMessageAsync("DeleteCookie");
            //var cookieManager = chromiumWebBrowser.GetCookieManager();
            //await cookieManager.DeleteCookiesAsync();
            //await PublishMessageAsync($"LoadUrl:{loginUrl}");
            //await chromiumWebBrowser.LoadUrlAsync(loginUrl);
            //await watiForIsLoading();
            //DevToolsContext = await chromiumWebBrowser.CreateDevToolsContextAsync();
            //chromiumWebBrowser.ShowDevTools();

            //await Delay(60);

            //check page
            var checkPageResult = await CheckPage();
            if (checkPageResult.Status != PosterActionResultStatus.Success)
                return checkPageResult;
            //input login user name
            var userNameInput = await (await DevToolsContext).QuerySelectorAsync<CefSharp.Dom.HtmlElement>("#login-userid");
            await Delay();
            await userNameInput.SetAttributeAsync("value", propnexTask.Account);
            //input password
            var userPasswordInput = await (await DevToolsContext).QuerySelectorAsync<CefSharp.Dom.HtmlElement>("#login-password");
            await Delay();
            await userPasswordInput.SetAttributeAsync("value", propnexTask.Password);
            //login button
            var loginButton = await (await DevToolsContext).QuerySelectorAsync<HtmlElement>("#btn_login");
            await Delay(1);
            await loginButton.ClickAsync();
            await chromiumWebBrowser.WaitForNavigationAsync(new TimeSpan(0, 5, 0));
            await watiForIsLoading();
            await CheckPage();
            var warning = await (await DevToolsContext).QuerySelectorAsync<HtmlElement>("div.login-body > div.warning-container > div");
            if (warning != null)
            {
                return new PosterActionResult()
                {
                    Status = PosterActionResultStatus.Error,
                    Message = await warning.GetInnerTextAsync()
                };
            }
            await PublishMessageAsync("Login success");
            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success
            };
        }

        public async Task<PosterActionResult<List<Listing>>> GetListings()
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/listingsQuery?" +
                $"operationName=listingsQuery&variables=%7B%22shouldExtendsFields%22%3Atrue%2C%22statusCode%22%3A{2}%2C%22isExcludeChild%22%3Afalse%2C%22sortBy%22%3A%22new-to-old%22%2C%22limit%22%3A500%2C%22page%22%3A%221%22%2C%22includeReAdvertiseJob%22%3Atrue%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%227b6a11e4f1b523a1308f9f5274b6f7d46683849dbbaec77233f66ba21fbef25c%22%7D%7D";

            return await GetPolicy<List<Listing>>().ExecuteAsync(async (ctx) =>
            {
                string jscode = $@"()=> {{return fetch(""{url}"", {{
                                    ""headers"": {{
                                        ""accept"": ""application/json, text/plain, *"",
                                        ""if-none-match"": ""W/\""42c3f-i6z2s6ipfF/j1sd6HDcrj3E+{new Random(_lock.GetHashCode()).Next(100, 999)}\"""",
                                    }},
                                    ""method"": ""GET"",
                                    ""mode"": ""cors"",
                                    ""credentials"": ""include""
                                }}).then(res=>{{
                                      return res.text()
                                }})}}";
                var result = await (await DevToolsContext).EvaluateFunctionAsync<string>(jscode);
                if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                {
                    throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                }
                if (result.Contains("PersistedQueryNotFound"))
                {
                    throw new Exception("PersistedQueryNotFound");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
                //_logger?.Information(result);
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
           .WaitAndRetryAsync(10, retryNumber => TimeSpan.FromSeconds(30), async (exception, timeSpan, retryCount, context) =>
           {
               if (retryCount == 1)
                   await PublishMessageAsync($"retry count {retryCount}, exctption {exception.Exception.Message}");
               context["Message"] = exception.Exception.Message;
           });

            var fallbackPolicy = Policy<PosterActionResult<T>>
           .Handle<Exception>()
           .FallbackAsync(
                async (res, context, cancellationToken) =>
                   {
                       return await Task.Factory.StartNew(() =>
                       {
                           return new PosterActionResult<T>()
                           {
                               Status = PosterActionResultStatus.Error,
                               Message = $"{context.OperationKey}:{context["Message"].ToString()}"
                           };
                       }, cancellationToken);
                   },
                async (res, c) =>
                   {
                       await PublishMessageAsync($"onFallbackAsync called:{res?.Exception?.Message}");
                   });
            return Policy.WrapAsync(fallbackPolicy, retryPolicy);
        }

        public async Task<PosterActionResult<List<PlaceDto>>> BuildingQuery(string key, string listingId)
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/buildingQuery";
            url = $"https://www.iproperty.com.my/pro/rasor/graphql/buildingQuery?" +
               $"operationName=buildingQuery&" +
               $"variables=%7B%22q%22%3A%22level5%22%2C%22shouldExtendsFields%22%3Atrue%2C%22includeBuildingFacilityCodes%22%3Atrue%2C%22keyword%22%3A%22{key.ToLower()}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%229adf6bae0454ed8c07e24e43e1f2b622d0649a9fb96a056659ab644ee2b06e63%22%7D%7D";
            return await GetPolicy<List<PlaceDto>>().ExecuteAsync(async (ctx) =>
            {
                //while (true)
                //{
                //    await Task.Delay(1000 * 10);
                //}
                //var query = new RequestData<BuildingQueryVariablesDto>()
                //{
                //    extensions = new Extensions()
                //    {
                //        persistedQuery = new PersistedQuery()
                //        {
                //            Sha256Hash = "9adf6bae0454ed8c07e24e43e1f2b622d0649a9fb96a056659ab644ee2b06e63",
                //            Version = 1
                //        }
                //    },
                //    variables = new BuildingQueryVariablesDto()
                //    {
                //        keyword = key
                //    },
                //    query = "query buildingQuery($keyword: String, $q: String = \"level3\", $shouldExtendsFields: Boolean = false, $includeBuildingFacilityCodes: Boolean = false) {\r\n  places(searchAddress: true, keyword: $keyword, q: $q, limit: 50, includeBuildingFacilityCodes: $includeBuildingFacilityCodes) {\r\n    data {\r\n      latitude\r\n      longitude\r\n      address {\r\n        en_GB\r\n        zh_HK\r\n        zh_CN\r\n        __typename\r\n      }\r\n      postCode\r\n      level1 {\r\n        id\r\n        text {\r\n          en_GB\r\n          zh_HK\r\n          zh_CN\r\n          __typename\r\n        }\r\n        __typename\r\n      }\r\n      level2 {\r\n        id\r\n        text {\r\n          en_GB\r\n          zh_HK\r\n          zh_CN\r\n          __typename\r\n        }\r\n        __typename\r\n      }\r\n      level3 {\r\n        id @skip(if: $shouldExtendsFields)\r\n        text {\r\n          en_GB\r\n          zh_HK\r\n          zh_CN\r\n          __typename\r\n        }\r\n        __typename\r\n      }\r\n      level5 @include(if: $shouldExtendsFields) {\r\n        id\r\n        text {\r\n          en_GB\r\n          __typename\r\n        }\r\n        __typename\r\n      }\r\n      propertyType {\r\n        id\r\n        code\r\n        description\r\n        label\r\n        __typename\r\n      }\r\n      propertyGroupType {\r\n        id\r\n        code\r\n        description\r\n        label\r\n        __typename\r\n      }\r\n      buildingFacilities @include(if: $includeBuildingFacilityCodes) {\r\n        id\r\n        code\r\n        description\r\n        __typename\r\n      }\r\n      __typename\r\n    }\r\n    __typename\r\n  }\r\n}\r\n",
                //    OperationName = "buildingQuery"
                //};
                //var result = await AjaxJsonPostAsync(url, $"https://www.iproperty.com.my/pro/add-listing/location/{listingId}", data: JsonConvert.SerializeObject(query, jsonSerialzerSettings));
                var result = await AjaxJsonGetAsync(url);
                if (result.Contains("PersistedQueryNotFound"))
                {
                    throw new Exception("PersistedQueryNotFound");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
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
                if (result.Contains("PersistedQueryNotFound"))
                {
                    throw new Exception("PersistedQueryNotFound");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
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
            await Delay(10);
            var gRecaptcha = await (await DevToolsContext).QuerySelectorAsync(".g-recaptcha");
            if (gRecaptcha != null)
            {
                return new PosterActionResult()
                {
                    Status = PosterActionResultStatus.Error,
                    Message = "g-recaptcha"
                };
            }
            var challengeForm = await (await DevToolsContext).QuerySelectorAsync("#challenge-form");
            if (challengeForm != null)
            {
                var checkBox = await (await DevToolsContext).QuerySelectorAsync("#cf-stage > div.ctp-checkbox-container > label > input[type=checkbox]");
                try
                {
                    await (await DevToolsContext).EvaluateFunctionAsync("()=> {document.querySelector(\"iframe\").contentWindow.document.querySelector(\"#cf-stage > div.ctp-checkbox-container > label > input[type=checkbox]\").click();}");
                    await Delay(10);
                }
                catch (Exception ex)
                {

                }
                challengeForm = await (await DevToolsContext).QuerySelectorAsync("#challenge-form");
                if (challengeForm != null)
                {
                    return new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Error,
                        Message = "challenge recaptcha"
                    };
                }
            }
            var warning = await (await DevToolsContext).QuerySelectorAsync<Element>("div.warning-container > div.warning");
            if (warning != null)
            {
                return new PosterActionResult()
                {
                    Status = PosterActionResultStatus.Error,
                    Message = await warning.GetInnerHtmlAsync()
                };
            }



            //await Delay(60);
            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success
            };
        }

        /// <summary>
        /// ��ȡpost ����
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
                result = await (await DevToolsContext).EvaluateFunctionAsync<string>(jscode);
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }

        /// <summary>
        /// ��ȡjson����
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
                                    ""accept"": ""application/json, text/plain, *"",
                                  }},
                                  ""method"": ""GET"",
                                  ""mode"": ""cors""
                                }}).then(res=>{{
                                      return res.text()
                                }})}}";

                var result = await (await DevToolsContext).EvaluateFunctionAsync<string>(jscode);

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
                await PublishMessageAsync($"Waiting loading {chromiumWebBrowser.IsLoading}");
                await Delay();
            }
        }

        public async Task Delay(int delay = 5)
        {
            await PublishMessageAsync($"Waiting {delay} sec");
            await Task.Delay(delay * 1000);
        }

        public async Task GetTaskAsync()
        {
            //PnTaskDto = new PnTaskDto()
            //{
            //    Number = "23885.cef.tsk"
            //};
            //propnexTasks = _propnexTaskProvider.Get(System.IO.File.ReadAllText("E:\\23885.cef.tsk"));
            //if (propnexTasks == null)
            //{
            //    await PublishMessageAsync("Not find tasks ,dealy 1 min");
            //    await Task.Delay(1000 * 60);
            //    Close();
            //}
            //return;
            var context = "";
            PnTaskDto = await WebServer.GetTask();
            if (PnTaskDto != null)
            {
                context = await WebServer.GetTaskContent(PnTaskDto);
                propnexTasks = _propnexTaskProvider.Get(context);
                if (propnexTasks == null)
                {
                    await PublishMessageAsync("Not find tasks ,dealy 1 min");
                    await Task.Delay(1000 * 60);
                    return;
                }
            }
            else
            {
                await PublishMessageAsync("Not find tasks ,dealy 1 min");
                await Task.Delay(1000 * 60);
                return;
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

        private async Task xwebItemAsync(PosterActionResult posterActionResult, PropnexListing propnexListing)
        {
            await PublishMessageAsync($"upload result XwebItem:status={(posterActionResult.Status == PosterActionResultStatus.Success ? "Done" : "Faile")}.task_id={propnexTask.Id},taskitem_id={propnexListing.Details["taskitem_id"]},taskitem_note={posterActionResult.Message}");
            using (RestClient client = new RestClient("https://franchise-prod.propnex.net/index.php/tasks/updateStatus"))
            {
                var request = new RestRequest();
                request.AddParameter("account_name", propnexTask.Account);
                request.AddParameter("account_password", propnexTask.Password);
                request.AddParameter("task_id", propnexTask.Id);
                request.AddParameter("taskitem_id", propnexListing.Details["taskitem_id"]);
                request.AddParameter("status", (posterActionResult.Status == PosterActionResultStatus.Success ? "Done" : "Failed"));
                request.AddParameter("time_cost", "0");
                request.AddParameter("taskitem_note", posterActionResult.Message);
                if (PosterActionResultStatus.Success == posterActionResult.Status)
                {
                    request.AddParameter($"portal_link", $"https://www.iproperty.com.my/property/sepang/sale-{posterActionResult.Data}/");
                }
                else
                {
                    request.AddParameter("portal_link", "");
                }
                request.AddParameter("poster", "cef");
                request.Method = Method.Post;
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                var response = await Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 (response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted) || !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(5, retryNumber => TimeSpan.FromSeconds(30), async (ex, retry) =>
                {
                    await PublishMessageAsync($"xwebItem error ,{propnexTask.Id},{propnexListing.Details["taskitem_id"]} - {ex.Result.Request.Resource}");
                }).ExecuteAsync(async () =>
                {
                    return await client.ExecutePostAsync(request);
                });
            }
        }

        private async Task XwebEndAsync(string note = "")
        {
            await PublishMessageAsync($"upload result XwebEnd:task_id={propnexTask.Id}");
            using (RestClient client = new RestClient("https://franchise-prod.propnex.net/index.php/tasks/updateStatus"))
            {
                var request = new RestRequest();
                request.AddParameter("account_name", propnexTask.Account);
                request.AddParameter("account_password", propnexTask.Password);
                request.AddParameter("task_id", propnexTask.Id);
                request.AddParameter("status", "Done");
                request.AddParameter("time_cost", "0");
                request.AddParameter("note", note);
                request.AddParameter("poster", "cef");
                request.Method = Method.Post;
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                var response = await Policy
                .Handle<Exception>()
                 .OrResult<RestResponse>(response =>
                 (response.ResponseStatus == ResponseStatus.TimedOut ||
                 response.ResponseStatus == ResponseStatus.Aborted) || !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(5, retryNumber => TimeSpan.FromSeconds(30), async (ex, retry) =>
                {
                    await PublishMessageAsync($"XwebEnd error ,{propnexTask.Id}- {ex.Result.Request.Resource}");
                }).ExecuteAsync(async () =>
                {
                    return await client.ExecutePostAsync(request);
                });
            }
        }
    }
}