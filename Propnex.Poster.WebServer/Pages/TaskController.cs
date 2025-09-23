using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
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
                    taskListings.Add(createOrUpdateListing);
                }

                return taskListings;
            }
            catch(FlurlHttpException ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }
        }

        [Route("/api/taskChope")]
        [HttpGet]
        public async Task<List<TaskListing>> GetChopeListingJson(string listingId)
        {
            if (string.IsNullOrEmpty(listingId))
                throw new UserFriendlyException("listingId can not be empty");
            try
            {
                var listsContext = await $"https://pa-production.propnex.net/index.php/tasks/getChoperFile?choper_id={listingId}".GetStringAsync();
                var listings = new GuruTaskListings(listsContext);

                var taskListings = new List<TaskListing>();

                foreach (var listing in listings.Listings)
                {
                    var createOrUpdateListing = new TaskListing();
                    createOrUpdateListing.FastRepost = listing.FastRepost;
                    createOrUpdateListing.Listing = new CreateOrUpdateListing();
                    createOrUpdateListing.Listing.Create(listing.Listing);
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
}
