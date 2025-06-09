using CefSharp;
using CefSharp.Dom;
using Flurl;
using Flurl.Http;
using Flurl.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Wrap;
using Propnex.Poster.Dtos;
using Propnex.Poster.IProperty;
using Propnex.Poster.IProperty.V1;
using Propnex.Poster.Share;
using PropnexPoster.NetCoreWinForm;
using RestSharp;
using Serilog;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Windows.Forms;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using static System.Net.WebRequestMethods;
using Extensions = Propnex.Poster.IProperty.V1.ExtensionsV1;
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
        private List<Propnex.Poster.IProperty.V1.Listing> Listings = new List<IProperty.V1.Listing>();

        private IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.AddListingMutationDto>>? addListingMutationDto;
        private IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.LocationDto>> locationDto;
        private IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.PropertyDetailsDto>> propertyDetailsDto;

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
            string? listingId = "";

            propnexListing.Details["data_step1"] = propnexListing.Details["data_step1"].Replace("\"location\":[]", "\"location\":{}");
            addListingMutationDto = JsonConvert.DeserializeObject<IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.AddListingMutationDto>>>(propnexListing.Details["data_step1"]);

            locationDto = JsonConvert.DeserializeObject<IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.LocationDto>>>(propnexListing.Details["data_location"].Replace("\"extension\":[]", "\"extension\":{}"));

            propnexListing.Details["data_details"] = propnexListing.Details["data_details"].Replace("\"location\":[]", "\"location\":{}");
            propertyDetailsDto = JsonConvert.DeserializeObject<IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.PropertyDetailsDto>>>(propnexListing.Details["data_details"]);

            var resultLocation = await location(propnexListing);

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
            listingId = resultLocation.Data.Id;
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
                Message = resultUpgradePublish.Message
            };
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
            propnexListing.Details["data_step1"] = propnexListing.Details["data_step1"].Replace("\"location\":[]", "\"location\":{}");
            addListingMutationDto = JsonConvert.DeserializeObject<IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.AddListingMutationDto>>>(propnexListing.Details["data_step1"]);

            locationDto = JsonConvert.DeserializeObject<IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.LocationDto>>>(propnexListing.Details["data_location"].Replace("\"extension\":[]", "\"extension\":{}"));

            propnexListing.Details["data_details"] = propnexListing.Details["data_details"].Replace("\"location\":[]", "\"location\":{}");
            propertyDetailsDto = JsonConvert.DeserializeObject<IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.PropertyDetailsDto>>>(propnexListing.Details["data_details"]);

            //var refUrl = $"https://www.iproperty.com.my/pro/edit-listing/listing-details/{listingId}";
            //var resultListingDetails = await listingDetails();
            //if (resultListingDetails.Status == PosterActionResultStatus.Success)
            //    listingId = resultListingDetails.Data;
            //else
            //{
            //    await PublishMessageAsync(resultListingDetails.Message);
            //    return new PosterActionResult()
            //    {
            //        Data = listingId,
            //        Message = resultListingDetails.Message,
            //        Status = PosterActionResultStatus.Error
            //    };

            //}

            var resultLocation = await updatelocation(propnexListing, int.Parse(listingId), $"https://www.iproperty.com.my/pro/edit-listing/location/{listingId}");
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

            listingId = resultLocation.Data.Id;
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

            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success,
                Message = resultDescriptionMedial.Message

            };
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
                var data = JsonConvert.DeserializeObject<IProperty.V1.ResponseDataV1<UpdateListingPayloadV1>>(result);
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


        public async Task<PosterActionResult<AddListingDto>> location(PropnexListing propnexListing)
        {

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

            var listProperty = await complete(buildingText);

            if (listProperty.Data.Count == 0 || listProperty.Data == null)
            {
                return new PosterActionResult<AddListingDto>()
                {
                    Data = null,
                    Status = PosterActionResultStatus.Error,
                    Message = $"not find {buildingText}"
                };
            }

            var property = listProperty.Data.FirstOrDefault();

            var stupeLocationDto = new StupeLocationDto();
            stupeLocationDto.buildingFacilityCodes = property.buildingFacilities.Select(q => q.Code).ToList();
            stupeLocationDto.Location = new IProperty.LocationDto()
            {
                Address = null,
                latitude = property.latitude.Value,
                longitude = property.longitude.Value,
                level1 = property.level1,
                level2 = property.level2,
                level3 = new Level3() { NanoId = property.level3?.NanoId }, // property.level3,
                level4 = property.level4,
                level5 = property.level5,
                postalCode = property.PostCode,
                hasNoTownship = property.hasTownship
            };

            stupeLocationDto.Location.level1.Text = null;
            stupeLocationDto.Location.level2.Text = null;
            stupeLocationDto.Location.level3.Text = null;// = new Level3();
            stupeLocationDto.Location.level4.NanoId = null;
            stupeLocationDto.Location.level5.Text = null;


            var query = "mutation addListingMutation($input: AddListingInput!) {\n  addListing(input: $input) {\n    listing {\n      id\n      channel {\n        id\n        code\n        description\n        label\n        __typename\n      }\n      propertyType {\n        id\n        code\n        description\n        label\n        __typename\n      }\n      lister {\n        id\n        firstName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        lastName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        fullName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        email\n        __typename\n      }\n      representationLister {\n        id\n        firstName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        lastName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        fullName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        email\n        __typename\n      }\n      listingRefNo\n      extension {\n        isCoAgency\n        listingExclusivity {\n          id\n          code\n          description\n          label\n          __typename\n        }\n        __typename\n      }\n      propertyCategoryType {\n        id\n        code\n        description\n        label\n        __typename\n      }\n      isAuction\n      auctionDate\n      postedDate\n      __typename\n    }\n    __typename\n  }\n}\n";
            stupeLocationDto.propertyTypeCode = property.propertyType.Code;
            stupeLocationDto.propertyGroupTypeCode = property.propertyGroupType.Code;
            stupeLocationDto.propertyCategoryTypeCode = addListingMutationDto.variables.input.propertyCategoryTypeCode;
            stupeLocationDto.saleableAreaMeasurementCode = addListingMutationDto.variables.input.saleableAreaMeasurementCode;
            stupeLocationDto.storeyCode = addListingMutationDto.variables.input.storeyCode;

            var addListingMutationInput = new RequestData<InputDto<StupeLocationDto>>()
            {
                extensions = new IProperty.Extensions()
                {
                    persistedQuery = new IProperty.PersistedQuery
                    {
                        Sha256Hash = "d6797c24744e9c772977451f0dd7270ab0e622a483ffc8676d6accdd997036ae",
                        Version = 1
                    }
                },
                OperationName = "addListingMutation",
                query = query,
                variables = new InputDto<StupeLocationDto>() { Input = stupeLocationDto }
            };




            var listing = await GetPolicy<AddListingDto>().ExecuteAsync(async (ctx) =>
            {
                string addListingMutationUrl = "https://www.iproperty.com.my/pro/rasor/graphql/addListingMutation";

                var result = await AjaxJsonPostAsync(addListingMutationUrl, "https://www.iproperty.com.my/pro/add-listing/location", data: JsonConvert.SerializeObject(addListingMutationInput, jsonSerialzerSettings));
                if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                {
                    await PublishMessageAsync(result);
                    throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                }
                // await Task.Delay(60 * 1000 * 2);
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }

                try
                {
                    var listing = JsonConvert.DeserializeObject<ResponseData<AddListingPayload>>(result);
                    await PublishMessageAsync($"create listing details success,listing is is {listing.Data.addListing.listing.Id}");
                    return new PosterActionResult<AddListingDto>()
                    {
                        Data = listing.Data.addListing.listing,
                        Status = PosterActionResultStatus.Success
                    };
                }
                catch (Exception ex)
                {

                }



                return new PosterActionResult<AddListingDto>();
            }, new Context("addListingMutation"));


            return listing;
            async Task<PosterActionResult<List<PropertyDto>>> complete(string key)
            {
                string completeUrl = $"https://www.iproperty.com.my/pro/rasor/graphql/autocomplete?operationName=autocomplete&variables=%7B%22resolveLocation%22%3Atrue%2C%22includeBuildingFacility%22%3Atrue%2C%22keyword%22%3A%22{Url.Encode(key)}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22154be742795bc943bd5c3b9c85b43b5d072ffb26fc11229e58b9599935aabcc3%22%7D%7D";

                return await GetPolicy<List<PropertyDto>>()
                     .ExecuteAsync(async (ctx) =>
                     {
                         var result = await AjaxJsonGetAsync(completeUrl);
                         var responseData = JsonConvert.DeserializeObject<ResponseData<AutocompleteResult>>(result);
                         if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                         {

                             await PublishMessageAsync(result);
                             throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                         }
                         if (result.Contains("errors"))
                         {
                             throw new Exception(result);
                         }
                         return new PosterActionResult<List<PropertyDto>>()
                         {
                             Data = responseData.Data.autocomplete.Data,
                             Status = PosterActionResultStatus.Success
                         };
                     }, new Context("complete"));

            }
        }

        public async Task<PosterActionResult<AddListingDto>> updatelocation(PropnexListing propnexListing, int id, string action = "")
        {

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

            var listProperty = await complete(buildingText);

            if (listProperty.Data.Count == 0 || listProperty.Data == null)
            {
                return new PosterActionResult<AddListingDto>()
                {
                    Data = null,
                    Status = PosterActionResultStatus.Error,
                    Message = $"not find {buildingText}"
                };
            }

            var property = listProperty.Data.FirstOrDefault();

            var stupeLocationDto = new UpdateStupeLocationDto();
            stupeLocationDto.Id = id;
            stupeLocationDto.buildingFacilityCodes = property.buildingFacilities.Select(q => q.Code).ToList();
            stupeLocationDto.Location = new IProperty.LocationDto()
            {
                Address = null,
                latitude = property.latitude.Value,
                longitude = property.longitude.Value,
                level1 = property.level1,
                level2 = property.level2,
                level3 = new Level3() { NanoId = property.level3?.NanoId }, // property.level3,
                level4 = property.level4,
                level5 = property.level5,
                postalCode = property.PostCode,
                hasNoTownship = property.hasTownship
            };

            stupeLocationDto.Location.level1.Text = null;
            stupeLocationDto.Location.level2.Text = null;
            stupeLocationDto.Location.level3.Text = null;// = new Level3();
            stupeLocationDto.Location.level4.NanoId = null;
            stupeLocationDto.Location.level5.Text = null;


            var query = "mutation addListingMutation($input: AddListingInput!) {\n  addListing(input: $input) {\n    listing {\n      id\n      channel {\n        id\n        code\n        description\n        label\n        __typename\n      }\n      propertyType {\n        id\n        code\n        description\n        label\n        __typename\n      }\n      lister {\n        id\n        firstName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        lastName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        fullName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        email\n        __typename\n      }\n      representationLister {\n        id\n        firstName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        lastName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        fullName {\n          en_GB\n          zh_HK\n          zh_CN\n          __typename\n        }\n        email\n        __typename\n      }\n      listingRefNo\n      extension {\n        isCoAgency\n        listingExclusivity {\n          id\n          code\n          description\n          label\n          __typename\n        }\n        __typename\n      }\n      propertyCategoryType {\n        id\n        code\n        description\n        label\n        __typename\n      }\n      isAuction\n      auctionDate\n      postedDate\n      __typename\n    }\n    __typename\n  }\n}\n";
            stupeLocationDto.propertyTypeCode = property.propertyType.Code;
            stupeLocationDto.propertyGroupTypeCode = property.propertyGroupType.Code;
            stupeLocationDto.propertyCategoryTypeCode = addListingMutationDto.variables.input.propertyCategoryTypeCode;
            stupeLocationDto.saleableAreaMeasurementCode = addListingMutationDto.variables.input.saleableAreaMeasurementCode;
            stupeLocationDto.storeyCode = addListingMutationDto.variables.input.storeyCode;

            var addListingMutationInput = new RequestData<InputDto<UpdateStupeLocationDto>>()
            {
                extensions = new IProperty.Extensions()
                {
                    persistedQuery = new IProperty.PersistedQuery
                    {
                        Sha256Hash = "696299734d38d07dac5e4571217cf4ecd35b8147c72822e8cf7b0b92f8e06e96",
                        Version = 1
                    }
                },
                OperationName = "updateListingInfo",
                //query = query,
                variables = new InputDto<UpdateStupeLocationDto>() { Input = stupeLocationDto }
            };




            var listing = await GetPolicy<AddListingDto>().ExecuteAsync(async (ctx) =>
            {
                string addListingMutationUrl = "https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo";

                var result = await AjaxJsonPostAsync(addListingMutationUrl, action, data: JsonConvert.SerializeObject(addListingMutationInput, jsonSerialzerSettings));
                if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                {
                    await PublishMessageAsync(result);
                    throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                }
                if (result.Contains("provided sha does not match query"))
                {
                    await PublishMessageAsync(result);
                    throw new Exception("provided sha does not match query");
                }
                // await Task.Delay(60 * 1000 * 2);
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }

                try
                {
                    var listing = JsonConvert.DeserializeObject<ResponseData<UpdateListingPayload>>(result);
                    await PublishMessageAsync($"create listing details success,listing is is {listing.Data.updateListing.listing.Id}");
                    return new PosterActionResult<AddListingDto>()
                    {
                        Data = listing.Data.updateListing.listing,
                        Status = PosterActionResultStatus.Success
                    };
                }
                catch (Exception ex)
                {

                }



                return new PosterActionResult<AddListingDto>();
            }, new Context("addListingMutation"));


            return listing;
            async Task<PosterActionResult<List<PropertyDto>>> complete(string key)
            {
                string completeUrl = $"https://www.iproperty.com.my/pro/rasor/graphql/autocomplete?operationName=autocomplete&variables=%7B%22resolveLocation%22%3Atrue%2C%22includeBuildingFacility%22%3Atrue%2C%22keyword%22%3A%22{Url.Encode(key)}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22154be742795bc943bd5c3b9c85b43b5d072ffb26fc11229e58b9599935aabcc3%22%7D%7D";

                return await GetPolicy<List<PropertyDto>>()
                     .ExecuteAsync(async (ctx) =>
                     {
                         var result = await AjaxJsonGetAsync(completeUrl);
                         var responseData = JsonConvert.DeserializeObject<ResponseData<AutocompleteResult>>(result);
                         if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                         {

                             await PublishMessageAsync(result);
                             throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                         }
                         if (result.Contains("errors"))
                         {
                             throw new Exception(result);
                         }
                         return new PosterActionResult<List<PropertyDto>>()
                         {
                             Data = responseData.Data.autocomplete.Data,
                             Status = PosterActionResultStatus.Success
                         };
                     }, new Context("complete"));

            }
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
                //propnexListing.Details["data_details"] = propnexListing.Details["data_details"].Replace("\"location\":[]", "\"location\":{}");
                var propertyDetailsDtov2 = JsonConvert.DeserializeObject<RequestData<Variables<IProperty.PropertyDetailsDto>>>(propnexListing.Details["data_details"]);
                var input = propertyDetailsDtov2.variables.input;
                input.id = listingId;
                propertyDetailsDtov2.extensions.persistedQuery.Sha256Hash = "696299734d38d07dac5e4571217cf4ecd35b8147c72822e8cf7b0b92f8e06e96";
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
                if (locationDto.variables.input.buildingFacilityCodes != null)
                {
                    input.buildingFacilityCodes = locationDto.variables.input.buildingFacilityCodes;
                }
                input.saleableAreaMeasurementCode = locationDto.variables.input.saleableAreaMeasurementCode;

                input.extension.isCoAgency = addListingMutationDto.variables.input.extension.isCoAgency;

                input.channelCode = addListingMutationDto.variables.input.channelCode;
                input.isAuction = addListingMutationDto.variables.input.isAuction;
                input.auctionDate = addListingMutationDto.variables.input.auctionDate;
                input.listingRefNo = addListingMutationDto.variables.input.listingRefNo;
                input.isPostCrossListing = false;

                var result = await AjaxJsonPostAsync("https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo", refUrl, data: JsonConvert.SerializeObject(propertyDetailsDtov2, jsonSerialzerSettings));

                await Delay(60);
               if (result.Contains("PersistedQueryNotFound"))
                {
                    throw new Exception("PersistedQueryNotFound");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
                var listing = JsonConvert.DeserializeObject<ResponseDataV1<UpdateListingPayloadV1>>(result);
                await PublishMessageAsync($"propertyDetails success,{listingId}");
                return new PosterActionResult<Listing>()
                {
                    Data = listing.Data.updateListing.listing,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("propertyDetails"));

            return result;

        }

        public async Task<PosterActionResult<Listing>> updatePropertyDetails(PropnexListing propnexListing, string listingId, string action = "")
        {
            var refUrl = $"https://www.iproperty.com.my/pro/add-listing/location/{listingId}";
            if (string.IsNullOrEmpty(action) == false)
            {
                refUrl = action;
            }

            var result = await GetPolicy<Listing>().ExecuteAsync(async (ctx) =>
            {
                //propnexListing.Details["data_details"] = propnexListing.Details["data_details"].Replace("\"location\":[]", "\"location\":{}");
                var propertyDetailsDtov2 = JsonConvert.DeserializeObject<RequestData<Variables<IProperty.PropertyDetailsDto>>>(propnexListing.Details["data_details"]);
                var input = propertyDetailsDtov2.variables.input;
                input.id = listingId;
                propertyDetailsDtov2.extensions.persistedQuery.Sha256Hash = "696299734d38d07dac5e4571217cf4ecd35b8147c72822e8cf7b0b92f8e06e96";
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
                if (propertyDetailsDto.variables.input.buildingFacilityCodes != null)
                {
                    input.buildingFacilityCodes = propertyDetailsDto.variables.input.buildingFacilityCodes;
                }
                input.saleableAreaMeasurementCode = propertyDetailsDto.variables.input.saleableAreaMeasurementCode;

                input.extension.isCoAgency = addListingMutationDto.variables.input.extension.isCoAgency;

                input.channelCode = addListingMutationDto.variables.input.channelCode;
                input.isAuction = addListingMutationDto.variables.input.isAuction;
                input.auctionDate = addListingMutationDto.variables.input.auctionDate;
                input.listingRefNo = addListingMutationDto.variables.input.listingRefNo;
                input.isPostCrossListing = false;

                var result = await AjaxJsonPostAsync("https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo", refUrl, data: JsonConvert.SerializeObject(propertyDetailsDtov2, jsonSerialzerSettings));

                await Delay(60);
                if (result.Contains("PersistedQueryNotFound"))
                {
                    throw new Exception("PersistedQueryNotFound");
                }
                if (result.Contains("errors"))
                {
                    throw new Exception(result);
                }
                var listing = JsonConvert.DeserializeObject<ResponseDataV1<UpdateListingPayloadV1>>(result);
                await PublishMessageAsync($"propertyDetails success,{listingId}");
                return new PosterActionResult<Listing>()
                {
                    Data = listing.Data.updateListing.listing,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("propertyDetails"));

            return result;

        }

        public async Task<PosterActionResult<IProperty.V1.Listing>> descriptionMedial(PropnexListing propnexListing, string listingId, string action = "")
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
                descriptionMedialDto.variables.input.title = new ListingMultiLangTextV1()
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
                descriptionMedialDto.variables.input.title = new ListingMultiLangTextV1()
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
                 descriptionMedialDto.extensions.persistedQuery.Sha256Hash = "696299734d38d07dac5e4571217cf4ecd35b8147c72822e8cf7b0b92f8e06e96";
                 var result = await AjaxJsonPostAsync("https://www.iproperty.com.my/pro/rasor/graphql/updateListingInfo", refUrl, data: JsonConvert.SerializeObject(descriptionMedialDto, jsonSerialzerSettings));

                 if (result.Contains("PersistedQueryNotFound"))
                 {
                     throw new Exception("PersistedQueryNotFound");
                 }
                 if (result.Contains("errors"))
                 {
                     throw new Exception(result);
                 }
                 var listing = JsonConvert.DeserializeObject<ResponseData<UpdateListingPayloadV1>>(result);
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
                    request.Timeout = 60 * 1000;
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

        async Task<PosterActionResult<IProperty.V1.Listing>> upgradePublish(PropnexListing propnexListing, string listingId)
        {

            var listingQuote = await listingQuoteQueryAsync(listingId);
            var quote = listingQuote.Data.quotes.FirstOrDefault(q => q.listingProduct.Id != null && q.listingProduct.Label == "Standard");
            if (quote != null)
            {
                var publishListingInfoDto = new IProperty.V1.RequestDataV1<IProperty.V1.Variables<IProperty.V1.PublishListingInfoDto>>();
                publishListingInfoDto.extensions = new Extensions()
                {
                    persistedQuery = new IProperty.V1.PersistedQueryV1() { Sha256Hash = "29cfe80f371c1feff5c97456cd1d4b19e3622ccfd657aad0918d5f98b968ec26", Version = 1 }
                };
                publishListingInfoDto.OperationName = "publishListingInfo";
                publishListingInfoDto.variables = new IProperty.V1.Variables<IProperty.V1.PublishListingInfoDto>()
                {
                    input = new IProperty.V1.PublishListingInfoDto()
                    {
                        id = listingId,
                    }
                };
                publishListingInfoDto.variables.input.quotes.Add(new IProperty.V1.PublishListingInfoQuote()
                {
                    channelCode = addListingMutationDto.variables.input.channelCode.Value,
                    quoteIds = new List<string>() { quote.quoteId }
                });

                await GetPolicy<IProperty.V1.Listing>().ExecuteAsync(async (ctx) =>
                {
                    publishListingInfoDto.variables.shouldExtendsFields = true;

                    var result = await AjaxJsonPostAsync($"https://www.iproperty.com.my/pro/rasor/graphql/publishListingInfo",
                         $"https://www.iproperty.com.my/pro/add-listing/upgrade-and-review/{listingId}",
                         data: JsonConvert.SerializeObject(publishListingInfoDto, jsonSerialzerSettings)
                         );

                    await Delay(60);
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
                    var listing = JsonConvert.DeserializeObject<IProperty.V1.ResponseDataV1<IProperty.V1.PublishListingPayloadV1>>(result);
                    return new PosterActionResult<IProperty.V1.Listing>()
                    {
                        Data = listing.Data.publishListing.listings[0],
                        Status = PosterActionResultStatus.Success
                    };

                }, new Context());
            }

            return new PosterActionResult<IProperty.V1.Listing>();

            async Task<PosterActionResult<IProperty.V1.ListingQuoteDto>> listingQuoteQueryAsync(string listingId)
            {
                var url = $"https://www.iproperty.com.my/pro/rasor/graphql/ListingQuoteQuery/{listingId}?" +
                    $"operationName=ListingQuoteQuery&variables=%7B%22id%22%3A%22{listingId}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%222ac313f70c209c63edf06866222674a14f18c8e0e77017d05708230726ba3bf3%22%7D%7D";

                return await GetPolicy<IProperty.V1.ListingQuoteDto>()
                     .ExecuteAsync(async (ctx) =>
                     {
                         var result = await AjaxJsonGetAsync(url);
                         await Delay(1);
                         var responseData = JsonConvert.DeserializeObject<IProperty.V1.ResponseDataV1<PayloadV1<IProperty.V1.ListingQuoteDto>>>(result);
                         if (result.Contains("PERSISTED_QUERY_NOT_FOUND"))
                         {
                             await PublishMessageAsync(result);
                             throw new Exception("PERSISTED_QUERY_NOT_FOUND");
                         }
                         if (result.Contains("errors"))
                         {
                             throw new Exception(result);
                         }
                         return new PosterActionResult<IProperty.V1.ListingQuoteDto>()
                         {
                             Data = responseData.Data.listing,
                             Status = PosterActionResultStatus.Success
                         };
                     }, new Context("ListingQuoteQuery"));
            }
        }

        public IProperty.V1.Listing matchListing(PropnexListing propnexListing)
        {
            IProperty.V1.Listing listing = null;
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
                listing = Listings.Where(q => q.Location.Level5.Text != null).FirstOrDefault(q => (q.Channel.Label.ToLower() == propnexListing.ListingType.ToLower() || q.Channel.Label.ToLower() == listingType) &&
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
            var expired = await (await DevToolsContext).QuerySelectorAsync<HtmlElement>(".expiry-title");
            if (expired != null)
            {
                return new PosterActionResult()
                {
                    Status = PosterActionResultStatus.Error,
                    Message = await expired.GetInnerTextAsync()
                };
            }
            await PublishMessageAsync("Login success");
            return new PosterActionResult()
            {
                Status = PosterActionResultStatus.Success
            };
        }

        public async Task<PosterActionResult<List<IProperty.V1.Listing>>> GetListings()
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/listingsQuery?" +
                  //$"operationName=listingsQuery&variables=%7B%22shouldExtendsFields%22%3Atrue%2C%22statusCode%22%3A{2}%2C%22isExcludeChild%22%3Afalse%2C%22sortBy%22%3A%22new-to-old%22%2C%22limit%22%3A500%2C%22page%22%3A%221%22%2C%22includeReAdvertiseJob%22%3Atrue%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%227b6a11e4f1b523a1308f9f5274b6f7d46683849dbbaec77233f66ba21fbef25c%22%7D%7D";
                  $"operationName=listingsQuery&variables=%7B%22shouldExtendsFields%22%3Atrue%2C%22includeSummary%22%3Afalse%2C%22statusCode%22%3A2%2C%22isExcludeChild%22%3Afalse%2C%22sortBy%22%3A%22new-to-old%22%2C%22limit%22%3A50%2C%22page%22%3A%221%22%2C%22includeReAdvertiseJob%22%3Atrue%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22cb8b645832495633bfe9658ad205e60e23e2f93e8fbe09cfd12936fa5f4a4484%22%7D%7D";
            return await GetPolicy<List<IProperty.V1.Listing>>().ExecuteAsync(async (ctx) =>
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
                //await Delay(60);
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
                var jsonResult = JsonConvert.DeserializeObject<IProperty.V1.ResponseDataV1<IProperty.V1.ListingsData>>(result);
                //await Delay(60);
                return new PosterActionResult<List<IProperty.V1.Listing>>()
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

        public async Task<PosterActionResult<List<IProperty.V1.PlaceDto>>> Autocomplete(string key)
        {

            string url = "\r\nhttps://www.iproperty.com.my/pro/rasor/graphql/autocomplete";
            url += $"operationName=autocomplete&" +
                $"variables=%7B%22resolveLocation%22%3Atrue%2C%22includeBuildingFacility%22%3Atrue%2C%22keyword%22%3A%22{key}%22%7D" +
                $"&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22154be742795bc943bd5c3b9c85b43b5d072ffb26fc11229e58b9599935aabcc3%22%7D%7D";

            return await GetPolicy<List<IProperty.V1.PlaceDto>>().ExecuteAsync(async (ctx) =>
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
                var resultData = JsonConvert.DeserializeObject<IProperty.V1.ResponseDataV1<IProperty.V1.BuildingRequestData>>(result);
                return new PosterActionResult<List<IProperty.V1.PlaceDto>>()
                {
                    Data = resultData.Data.places.Data,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("BuildingQuery"));
        }

        public async Task<PosterActionResult<List<IProperty.V1.PlaceDto>>> BuildingQuery(string key, string listingId)
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/buildingQuery";
            url = $"https://www.iproperty.com.my/pro/rasor/graphql/buildingQuery?" +
               $"operationName=buildingQuery&" +
               $"variables=%7B%22q%22%3A%22level5%22%2C%22shouldExtendsFields%22%3Atrue%2C%22includeBuildingFacilityCodes%22%3Atrue%2C%22keyword%22%3A%22{key.ToLower()}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%229adf6bae0454ed8c07e24e43e1f2b622d0649a9fb96a056659ab644ee2b06e63%22%7D%7D";
            return await GetPolicy<List<IProperty.V1.PlaceDto>>().ExecuteAsync(async (ctx) =>
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
                var resultData = JsonConvert.DeserializeObject<IProperty.V1.ResponseDataV1<IProperty.V1.BuildingRequestData>>(result);
                return new PosterActionResult<List<IProperty.V1.PlaceDto>>()
                {
                    Data = resultData.Data.places.Data,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("BuildingQuery"));
        }

        public async Task<PosterActionResult<List<IProperty.V1.PlaceDto>>> level2Query(string name, string id)
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/level2Query?operationName=level2Query&variables=%7B%22keyword%22%3A%22{System.Web.HttpUtility.UrlEncode(name)}%22%2C%22level1Id%22%3A%22{id}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%2250af390e2fb55a7d6b9d8497c7062017baa2976c23106a571b08125ed31b7242%22%7D%7D";

            return await GetPolicy<List<IProperty.V1.PlaceDto>>().ExecuteAsync(async (ctx) =>
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
                var resultData = JsonConvert.DeserializeObject<IProperty.V1.ResponseDataV1<IProperty.V1.BuildingRequestData>>(result);
                return new PosterActionResult<List<IProperty.V1.PlaceDto>>()
                {
                    Data = resultData.Data.places.Data,
                    Status = PosterActionResultStatus.Success
                };
            }, new Context("level2Query"));


        }

        public async Task<PosterActionResult> CheckPage()
        {
            await Delay(20);
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
            data = data.Replace('\n', '"');
            string jscode = $"()=> fetch('{url}',{{ method:\"{type}\",referrer:'{referrerUrl}',headers:{{'content-type': 'application/json'}},body:{(data == "" ? "''" : "JSON.stringify(" + data + ")")}}}) .then(response => response.text())";
            string result = "";
            try
            {
                data = data.Replace("\"", "");
                result = await (await DevToolsContext).EvaluateFunctionAsync<string>(jscode);
                //await Delay(60);
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
                //await Delay(60);
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
            PnTaskDto = new PnTaskDto()
            {
                Number = "28984.cef.tsk"
            };
            propnexTasks = _propnexTaskProvider.Get(System.IO.File.ReadAllText($"E:\\{PnTaskDto.Number}"));
            if (propnexTasks == null)
            {
                await PublishMessageAsync("Not find tasks ,dealy 1 min");
                await Task.Delay(1000 * 60);
                Close();
            }
            return;
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