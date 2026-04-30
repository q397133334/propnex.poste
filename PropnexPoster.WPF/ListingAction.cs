using Propnex.Poster.Dtos;
using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using Propnex.Poster.PropertyGuru.Tasks;
using Propnex.Poster.Share;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PropnexPoster.WPF
{
    public class ListingAction : IDisposable
    {
        public Token Token { get; set; } = new Token();

        public Auth Auth { get; set; }

        public Api Api { get; set; }

        public AdsProduct AdsProduct { get; set; }

        public Mobile Mobile { get; set; }

        public ProjectsApi ProjectsApi { get; set; }

        public Agent Agent { get; set; }

        public Action<string, bool, bool> ActionLog;

        public GuruTask GuruTask { get; set; }

        public ListingAction(GuruTask guruTask, string proxyIp = "", Action<string, bool, bool> log = null)
        {
            GuruTask = guruTask;
            if (string.IsNullOrEmpty(proxyIp))
            {
                Auth = new Auth() { Log = Log };
                Api = new Api(Token) { Log = Log };
                AdsProduct = new AdsProduct(Token) { Log = Log };
                Mobile = new Mobile(Token) { Log = Log };
                ProjectsApi = new ProjectsApi(Token) { Log = Log };
                Agent = new Agent(Token) { Log = Log };
            }
            else
            {
                Auth = new Auth(proxyIp) { Log = Log };
                Api = new Api(Token, proxyIp) { Log = Log };
                AdsProduct = new AdsProduct(Token, proxyIp) { Log = Log };
                Mobile = new Mobile(Token, proxyIp) { Log = Log };
                ProjectsApi = new ProjectsApi(Token, proxyIp) { Log = Log };
                Agent = new Agent(Token, proxyIp) { Log = Log };
            }
            ActionLog = log;
        }

        public async Task<bool> LoginAsync()
        {
            ClientBase.PhoneModel = PhoneModelList.GetPhoneModel();
            LogSave("Get User Login Information ...", false, true);
            var pnUser = await WebServer.GetUser(GuruTask.Account);
            if (pnUser == null || pnUser.Id == Guid.Empty)
            {
                LogSave("Not Find User Logini Information ,Meed To Login", false, true);
                pnUser = new PnUserDto();
                pnUser.Account = GuruTask.Account;
                pnUser.Password = GuruTask.Password;
                pnUser.PhoneModel = ClientBase.PhoneModel;
                if (pnUser.PhoneModel.Length < 20)
                {
                    pnUser.PhoneModel += $";{Guid.NewGuid()}";
                }
                await WebServer.PnUser(pnUser);
                pnUser = await WebServer.GetUser(GuruTask.Account);
                await authAsync();
                await GetListingsAsync();
            }
            else
            {
                Token = Newtonsoft.Json.JsonConvert.DeserializeObject<Propnex.Poster.PropertyGuru.Mobile.Dto.Token>(pnUser.TokenJson);
                if (DateTime.Parse(Token.accessTokenExpiresAt).AddHours(-1) < DateTime.Now)
                {
                    await authAsync();
                }
            }

            if (Token == null)
            {
                return false;
            }

            async Task authAsync()
            {
                LogSave("Login ...", false, true);
                var loginResult = await Auth.LoginAsync(new AuthLogin()
                {
                    UserName = GuruTask.Account,
                    Password = GuruTask.Password
                });
                if (loginResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    LogSave("Login Success", false, true);
                    Token = loginResult.Data;
                    pnUser.TokenJson = Newtonsoft.Json.JsonConvert.SerializeObject(Token);
                    LogSave("Token :" + Token.accessToken, false, true);
                    await WebServer.UpdatePnUserToken(pnUser);
                }
                else
                {
                    LogSave("Login Failed" + loginResult.Message);
                    Token = null;
                }
            }
            return true;
        }

        public async Task<bool> RefreshTokenAsync(GuruTaskListing listing)
        {
            return true;
        }

        public async Task<bool> CreateListingV2Async(GuruTaskListing listing)
        {
            return true;
        }

        public async Task<bool> CreateListingV3Async(GuruTaskListing listing)
        {
            return true;
        }

        public async Task<bool> UpdateListingV2Async(GuruTaskListing listing)
        {
            return true;
        }

        public async Task<bool> UpdateListingV3Async(GuruTaskListing listing)
        {
            return true;
        }

        public async Task<bool> RepostV2Async(GuruTaskListing listing)
        {
            return true;
        }

        public async Task<bool> RepostV3Async(GuruTaskListing listing)
        {
            return true;
        }

        public void Dispose()
        {
            Token = null;
            Auth.Dispose();
            Api.Dispose();
            AdsProduct.Dispose();
            Mobile.Dispose();
            ProjectsApi.Dispose();
            Agent.Dispose();
        }


        public List<ListingsListing> Listings { get; set; } = new List<ListingsListing>();
        public List<ListingInfo> ListingInfos { get; set; }

        public async Task GetListingsAsync()
        {
            Listings = new List<ListingsListing>();
            ListingInfos = new List<ListingInfo>();
            var result = await Mobile.ListingManagementAsync(new QueryListingManagement(Token.User.AgentId.ToString()));
            try
            {
                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    addListing(result.Data.listings);
                    while (result.Data.page < result.Data.totalPages)
                    {
                        result = await Mobile.ListingManagementAsync(new QueryListingManagement(Token.User.AgentId.ToString())
                        {
                            Page = (result.Data.page + 1).ToString()
                        });
                        if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            addListing(result.Data.listings);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSave("Get Listings Error :" + ex.Message);
            }

            void addListing(List<ListingsListing> lists)
            {
                foreach (var item in lists)
                {
                    var info = new ListingInfo();
                    info.Id = item.id.Value;
                    info.Title = item.localizedTitle;
                    info.Score = item.qualityScore.ToString();
                    info.TypeCode = item.typeCode;
                    info.StatusCode = item.statusCode;
                    info.PropertyTypeCode = item.property.typeCode;
                    info.Prece = item.price.value.ToString();
                    info.StreetNumber = item.location.streetNumber;
                    info.StreetName = item.location.streetName1;
                    info.PostCode = item.location.postalCode;
                    if (item.products != null && item.products.Count > 0)
                    {
                        info.IsBoosted = item.products[0].productType == "boost-v2";
                    }
                    //turbo
                    if (item.products != null && item.products.Count > 0)
                    {
                        info.IsTurbo = item.products[0].productType == "turbo";
                    }
                    if (item.charges != null)
                    {
                        info.RepostCharge = item.charges.repost;
                    }
                    try
                    {
                        info.Sqft = Convert.ToInt32(item.sizes.floorArea[0].value).ToString();
                    }
                    catch
                    {
                        info.Sqft = Convert.ToInt32(item.sizes.landArea[0].value).ToString();
                    }
                    ListingInfos.Add(info);
                    Listings.Add(item);
                }
            }
        }

        private void LogSave(string message, bool isRef = false, bool isSave = true)
        {
            ActionLog?.Invoke(message, isRef, isSave);
        }

        private void Log(string message, bool isRef = false)
        {
            ActionLog?.Invoke(message, true, false);
        }

    }
}
