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
        public async Task<List<CreateOrUpdateListing>> GetListingJson(string listingId)
        {
            if (string.IsNullOrEmpty(listingId))
                throw new UserFriendlyException("listingId can not be empty");
            var listsContext = await $"https://pa-production.propnex.net/index.php/tasks/getListingFile?lid={listingId}".GetStringAsync();

            try
            {
                var listings = new GuruTaskListings(listsContext);

                var createOrUpdateListings = new List<CreateOrUpdateListing>();

                foreach (var listing in listings.Listings)
                {
                    var createOrUpdateListing = new CreateOrUpdateListing();
                    createOrUpdateListing.Create(listing.Listing);
                    createOrUpdateListings.Add(createOrUpdateListing);
                }

                return createOrUpdateListings;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message, innerException: ex);
            }

        }
    }
}
