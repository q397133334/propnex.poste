using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Tasks;
using Propnex.Poster.Share;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace Propnex.Poster.WebServer.Pages
{
    public class TaskController : AbpController
    {
        [Route("/api/task")]
        [HttpGet]
        public async Task<List<TaskListing>> GetListingJson(string listingId)
        {
            if (string.IsNullOrEmpty(listingId))
                throw new UserFriendlyException("listingId can not be empty");
            try
            {
                var listsContext = await $"https://pa-production.propnex.net/index.php/tasks/getListingFile?lid={listingId}".GetStringAsync();
                var listings = new GuruTaskListings(listsContext,"");
                

                var taskListings = new List<TaskListing>();

                foreach (var listing in listings.Listings)
                {
                    var createOrUpdateListing = new TaskListing();
                    createOrUpdateListing.FastRepost = listing.FastRepost;
                    createOrUpdateListing.Listing = new CreateOrUpdateListing();
                    createOrUpdateListing.Listing.Create(listing.Listing);
                    createOrUpdateListing.Photos = listing.Photos;
                    createOrUpdateListing.FloorPlan = listing.FloorPlan;
                    createOrUpdateListing.Tours = listing.Tours;
                    createOrUpdateListing.Videos = listing.Videos;
                    createOrUpdateListing.ListingV3 = listing.ListingV3;
                    createOrUpdateListing.PatchListingV3 = listing.PatchV3;
                    taskListings.Add(createOrUpdateListing);
                    
                }

                return taskListings;
            }
            catch (FlurlHttpException ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }
        }

        [Route("/api/task1")]
        [HttpGet]
        public async Task<List<TaskListing>> GetListing1Json(string listingId)
        {
            if (string.IsNullOrEmpty(listingId))
                throw new UserFriendlyException("listingId can not be empty");
            try
            {
                var listsContext = await $"https://pa-production.propnex.net/index.php/tasks/getListingFile?lid={listingId}".GetStringAsync();
                var listings = new GuruTaskListings(listsContext, "");


                var taskListings = new List<TaskListing>();

                foreach (var listing in listings.Listings)
                {
                    var createOrUpdateListing = new TaskListing();
                    createOrUpdateListing.FastRepost = listing.FastRepost;
                    createOrUpdateListing.Listing = new CreateOrUpdateListing();
                    createOrUpdateListing.Listing.Create(listing.Listing);
                    createOrUpdateListing.Photos = listing.Photos;
                    createOrUpdateListing.FloorPlan = listing.FloorPlan;
                    createOrUpdateListing.Tours = listing.Tours;
                    createOrUpdateListing.Videos = listing.Videos;
                    createOrUpdateListing.ListingV3 = listing.ListingV3;
                    createOrUpdateListing.PatchListingV3 = listing.PatchV3;
                    taskListings.Add(createOrUpdateListing);

                }

                return taskListings;
            }
            catch (FlurlHttpException ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }
        }

        //[Route("/api/taskChope")]
        //[HttpGet]
        //public async Task<List<TaskListing>> GetChopeListingJson(string listingId)
        //{
        //    if (string.IsNullOrEmpty(listingId))
        //        throw new UserFriendlyException("listingId can not be empty");
        //    try
        //    {
        //        var listsContext = await $"https://pa-production.propnex.net/index.php/tasks/getChoperFile?choper_id={listingId}".GetStringAsync();
        //        var listings = new GuruTaskListings(listsContext);

        //        var taskListings = new List<TaskListing>();

        //        foreach (var listing in listings.Listings)
        //        {
        //            var createOrUpdateListing = new TaskListing();
        //            createOrUpdateListing.FastRepost = listing.FastRepost;
        //            createOrUpdateListing.Listing = new CreateOrUpdateListing();
        //            createOrUpdateListing.Listing.Create(listing.Listing);
        //            taskListings.Add(createOrUpdateListing);
        //        }

        //        return taskListings;
        //    }
        //    catch (FlurlHttpException ex)
        //    {
        //        throw new UserFriendlyException(ex.Message, innerException: ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new UserFriendlyException(ex.Message, innerException: ex);
        //    }
        //}

        [Route("/api/taskChope")]
        [HttpGet]
        public async Task<List<TaskListing>> GetChopeListingJson(
            [ModelBinder(BinderType =typeof(ParameterWithAliasModelBinder))]
            string choperId
            )
        {
            if (string.IsNullOrEmpty(choperId))
                throw new UserFriendlyException("choperId can not be empty");
            try
            {
                var listsContext = await $"https://pa-production.propnex.net/index.php/tasks/getChoperFile?choper_id={choperId}".GetStringAsync();
                var listings = new GuruTaskListings(listsContext,"");

                var taskListings = new List<TaskListing>();

                foreach (var listing in listings.Listings)
                {
                    var createOrUpdateListing = new TaskListing();
                    createOrUpdateListing.FastRepost = listing.FastRepost;
                    createOrUpdateListing.Listing = new CreateOrUpdateListing();
                    createOrUpdateListing.Listing.Create(listing.Listing);
                    createOrUpdateListing.ListingV3 = listing.ListingV3;
                    createOrUpdateListing.PatchListingV3 = listing.PatchV3;
                    createOrUpdateListing.Photos = listing.Photos;
                    createOrUpdateListing.FloorPlan = listing.FloorPlan;
                    createOrUpdateListing.Tours = listing.Tours;
                    createOrUpdateListing.Videos = listing.Videos;
                    taskListings.Add(createOrUpdateListing);
                }

                return taskListings;
            }
            catch (FlurlHttpException ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }
        }


        [Route("/api/taskRetrieve")]
        [HttpPost]
        public async Task<List<string>> TaskRetrieve(Retrieve retrieve)
        {
            var results = new List<string>();
            foreach (var listing in retrieve.Listings)
            {
                try
                {
                    var guruListing = listing;
                    var postActionResult = new PosterActionResult()
                    {
                        Status = PosterActionResultStatus.Success
                    };
                    try
                    {
                        var url = "http://3.0.87.74/propnex/index.php/";
                        //var guruListing = await this.getListing(task.Id.ToString());
                        var retrieveListing = await RetrieveListing.Converter(guruListing, retrieve.Account, retrieve.TargetPortal, retrieve.Id);
                        retrieveListing.Account = retrieve.Account;
                        var result = RetrieveListing.GetData(retrieveListing, retrieve.Account, retrieve.Password, retrieve.Id);
                        if (result.Item2)
                        {
                            var data = result.Item1;

                            FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(data);

                            HttpClient httpClient = new HttpClient();
                            var ok = await httpClient.PostAsync($"{url}listings/post", formUrlEncodedContent);
                            var httpResult = await ok.Content.ReadAsStringAsync();
                            string[] ss = httpResult.Split(new char[] { ',' });
                            string xpid = "";
                            if (ss.Length > 1 && ss[0] == "ok")
                            {
                                xpid = ss[1].Trim();
                                retrieveListing.Details["xpressorID"] = xpid;

                                Dictionary<string, string> files = new Dictionary<string, string>();
                                if (string.IsNullOrEmpty(retrieveListing.Photos)) retrieveListing.Photos = "";
                                string[] photos = retrieveListing.Photos.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                //int i = 0;
                                var formName = "ListingAttachments";
                                foreach (string photo in photos)
                                {
                                    string[] vnames = photo.Split(new char[] { '#' });
                                    string p = vnames[0].Trim();
                                    if (!System.IO.File.Exists(p)) continue;
                                    files = new Dictionary<string, string>();
                                    files[formName + "[attachfile]"] = p;
                                    data = new Dictionary<string, string>();
                                    data[formName + "[category]"] = "photo";
                                    data["xpressor"] = "";

                                    data[formName + "[title]"] = "";
                                    if (retrieveListing.UseFileName)
                                    {
                                        string fn = System.IO.Path.GetFileName(p);
                                        string[] parts = fn.Split(new char[] { '.' });
                                        if (parts.Length > 0)
                                        {
                                            data[formName + "[title]"] = parts[0].Replace("-", " ").Replace("_", " ");
                                        }
                                        ;
                                        if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[1].Trim()))
                                        {
                                            data[formName + "[title]"] = vnames[1].Trim();
                                        }
                                    }
                                    try
                                    {
                                        using (var multipartFormDataContent = new MultipartFormDataContent())
                                        {
                                            foreach (var content in data)
                                            {
                                                multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                                            }
                                            using (var client = new HttpClient())
                                            {
                                                using (var stream = new StreamContent(new System.IO.FileStream(p, System.IO.FileMode.Open)))
                                                {
                                                    multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(p));
                                                    ok = await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                                    httpResult = await ok.Content.ReadAsStringAsync();
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //DoProgress("Exception in PostWebPageMultipart", -1, "");
                                        //throw;
                                    }
                                }


                                if (!string.IsNullOrEmpty(retrieveListing.FloorPlan))
                                {
                                    string[] floorplans = retrieveListing.FloorPlan.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string floorplan in floorplans)
                                    {
                                        string[] vnames = floorplan.Split(new char[] { '#' });
                                        string p = vnames[0].Trim();
                                        if (System.IO.File.Exists(p))
                                        {
                                            files = new Dictionary<string, string>();
                                            files[formName + "[attachfile]"] = p;
                                            data = new Dictionary<string, string>();
                                            data[formName + "[category]"] = "floorplan";
                                            data["xpressor"] = "";

                                            data[formName + "[title]"] = "";
                                            if (retrieveListing.UseFileName)
                                            {
                                                string fn = System.IO.Path.GetFileName(p);
                                                string[] parts = fn.Split(new char[] { '.' });
                                                data[formName + "[title]"] = parts[0].Replace("-", " ");
                                            }
                                            ;
                                            if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[1].Trim()))
                                            {
                                                data[formName + "[title]"] = vnames[1].Trim();
                                            }
                                            try
                                            {
                                                using (var multipartFormDataContent = new MultipartFormDataContent())
                                                {
                                                    foreach (var content in data)
                                                    {
                                                        multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                                                    }
                                                    using (var client = new HttpClient())
                                                    {
                                                        using (var stream = new StreamContent(new System.IO.FileStream(p, System.IO.FileMode.Open)))
                                                        {
                                                            multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(p));
                                                            ok = await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                                            httpResult = await ok.Content.ReadAsStringAsync();
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                //DoProgress("Exception in PostWebPageMultipart", -1, "");
                                                //throw;
                                            }

                                        }
                                    }
                                }

                                string[] tours = retrieveListing.Tours.Split(new string[] { "\n" }, StringSplitOptions.None);
                                string[] toursThumbnail = retrieveListing.TourThumbnails.Split(new string[] { "\n" }, StringSplitOptions.None);
                                //i = 0;
                                for (int j = 0; j < tours.Length; j++)
                                {
                                    string tour = tours[j].Trim();
                                    if (string.IsNullOrEmpty(tour)) continue;
                                    string tourThumbnail = toursThumbnail[j].Trim();
                                    data = new Dictionary<string, string>();
                                    data[formName + "[category]"] = "tour";
                                    data[formName + "[thumbnail]"] = tourThumbnail;

                                    string[] vnames = tour.Split(new char[] { '#' });
                                    string p = vnames[0].Trim();
                                    if (vnames.Length > 2)
                                    {
                                        p = string.Join("#", vnames, 0, vnames.Length - 1);
                                    }
                                    files = new Dictionary<string, string>();
                                    data[formName + "[title]"] = "";
                                    if (p.EndsWith(".swf", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".mov", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".flv", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (!System.IO.File.Exists(p)) continue;
                                        files[formName + "[attachfile]"] = p.Trim();
                                        if (retrieveListing.UseFileName)
                                        {
                                            string fn = System.IO.Path.GetFileName(p.Trim());
                                            string[] parts = fn.Split(new char[] { '.' });
                                            data[formName + "[title]"] = parts[0].Replace("-", " ");
                                        }
                                    }
                                    else
                                    {
                                        data[formName + "[embed_code]"] = p;
                                    }

                                    data["xpressor"] = "";

                                    if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[vnames.Length - 1].Trim()))
                                    {
                                        data[formName + "[title]"] = vnames[vnames.Length - 1].Trim();
                                    }
                                    try
                                    {
                                        using (var multipartFormDataContent = new MultipartFormDataContent())
                                        {
                                            foreach (var content in data)
                                            {
                                                multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                                            }
                                            using (var client = new HttpClient())
                                            {
                                                using (var stream = new StreamContent(new System.IO.FileStream(p, System.IO.FileMode.Open)))
                                                {
                                                    multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(p));
                                                    ok = await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                                    httpResult = await ok.Content.ReadAsStringAsync();
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //DoProgress("Exception in PostWebPageMultipart", -1, "");
                                        //throw;
                                    }
                                }

                                string[] videos = retrieveListing.Videos.Split(new string[] { "\n" }, StringSplitOptions.None);
                                string[] videosThumbnail = retrieveListing.VideoThumbnails.Split(new string[] { "\n" }, StringSplitOptions.None);
                                //i = 0;
                                for (int j = 0; j < videos.Length; j++)
                                {
                                    string video = videos[j];
                                    if (string.IsNullOrEmpty(video)) continue;
                                    string videoThumbnail = videosThumbnail.Length > j ? videosThumbnail[j] : "";
                                    data = new Dictionary<string, string>();
                                    data[formName + "[category]"] = "video";
                                    data[formName + "[thumbnail]"] = videoThumbnail;
                                    string[] vnames = video.Split(new char[] { '#' });
                                    string p = vnames[0].Trim();
                                    if (vnames.Length > 2) p = string.Join("#", vnames, 0, vnames.Length - 1);
                                    files = new Dictionary<string, string>();
                                    data[formName + "[title]"] = "";
                                    if (p.EndsWith(".swf", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".mov", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".flv", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (!System.IO.File.Exists(p)) continue;
                                        files[formName + "[attachfile]"] = p.Trim();
                                        if (retrieveListing.UseFileName)
                                        {
                                            string fn = System.IO.Path.GetFileName(p.Trim());
                                            string[] parts = fn.Split(new char[] { '.' });
                                            data[formName + "[title]"] = parts[0].Replace("-", " ");
                                        }
                                        ;
                                    }
                                    else
                                    {
                                        data[formName + "[embed_code]"] = p;
                                    }
                                    ;
                                    data["xpressor"] = "";

                                    if (vnames.Length > 1 && !string.IsNullOrEmpty(vnames[vnames.Length - 1].Trim()))
                                    {
                                        data[formName + "[title]"] = vnames[vnames.Length - 1].Trim();
                                    }
                                    ;

                                    try
                                    {
                                        using (var multipartFormDataContent = new MultipartFormDataContent())
                                        {
                                            foreach (var content in data)
                                            {
                                                multipartFormDataContent.Add(new StringContent(content.Value), content.Key);
                                            }
                                            using (var client = new HttpClient())
                                            {
                                                using (var stream = new StreamContent(new System.IO.FileStream(p, System.IO.FileMode.Open)))
                                                {
                                                    multipartFormDataContent.Add(stream, formName + "[attachfile]", System.IO.Path.GetFileName(p));
                                                    ok = await httpClient.PostAsync($"{url}listingAttachments/create/{xpid}", multipartFormDataContent);
                                                    httpResult = await ok.Content.ReadAsStringAsync();
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //DoProgress("Exception in PostWebPageMultipart", -1, "");
                                        //throw;
                                    }
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add($"Retrieve Error {listing.id}");
                    }
                    results.Add($"Retrieve Success {listing.id}");
                }
                catch (Exception ex)
                {
                    results.Add($"Retrieve Error {listing.id}");
                }
            }

            return results;
        }
    }


    public class ParameterWithAliasModelBinder : IModelBinder
    {

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName); // 获取值提供器的结果，这里应该是自定义解析逻辑的起点
            if (valueProviderResult == ValueProviderResult.None)
            {
                valueProviderResult = bindingContext.ValueProvider.GetValue("listingId");
            }
            if (valueProviderResult != ValueProviderResult.None) // 检查是否有值提供结果，即是否找到了名为"id"的数据
            {
                var value = valueProviderResult.FirstValue; // 获取第一个值
                bindingContext.Result = ModelBindingResult.Success(value); // 设置绑定结果为成功，并传递解析后的值。
            }
            return Task.CompletedTask;
        }
    }
}
