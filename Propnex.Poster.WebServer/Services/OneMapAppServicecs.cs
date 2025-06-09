using Propnex.Poster.Dtos.OneMap;
using RestSharp;
using System.Net;
using Volo.Abp.Application.Services;

namespace Propnex.Poster.WebServer.Services
{
    public class OneMapAppService : ApplicationService, IOneMapAppService
    {

        private RestSharp.RestClient oneMapClient;
        private static CookieContainer cookieContainer;

        public OneMapAppService()
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }
            var options = new RestClientOptions("https://www.onemap.gov.sg/")
            {
                CookieContainer = cookieContainer
            };
            oneMapClient = new RestSharp.RestClient(options, useClientFactory: true);

        }

        public async Task<string> GetnearbyPriSchools(InputNearbYPicSchoolDto input)
        {
            await getCookie();

            var request = new RestSharp.RestRequest($"omapp/getnearbyPriSchools?dist={input.Dist}&postalcode={input.Postalcode}&blkno={input.blkno}", Method.Get);

            var resp = await oneMapClient.ExecuteAsync(request);
            if (resp.IsSuccessful)
            {
                return resp.Content;
            }

            return resp.Content;
        }

        private async Task getCookie()
        {
            if (cookieContainer.GetAllCookies().Where(q => q.Name == "OMITN").Count() == 0)
            {
                var request = new RestSharp.RestRequest("", method: Method.Get);
                var resp = await oneMapClient.ExecuteAsync(request);
                if (!resp.IsSuccessful)
                {
                    new Volo.Abp.UserFriendlyException($"get cookie error {resp.ErrorMessage} {resp.StatusCode}");
                }
            }
        }
    }

    public interface IOneMapAppService : IApplicationService
    {
        Task<string> GetnearbyPriSchools(Dtos.OneMap.InputNearbYPicSchoolDto input);

    }
}
