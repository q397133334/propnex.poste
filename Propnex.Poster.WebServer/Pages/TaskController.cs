using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Tasks;
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
                var listings = new GuruTaskListings(listsContext);

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
                var listings = new GuruTaskListings(listsContext);

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
