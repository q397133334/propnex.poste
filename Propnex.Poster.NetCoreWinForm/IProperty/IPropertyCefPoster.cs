using CefSharp;
using CefSharp.Dom;
using CefSharp.WinForms;
using Propnex.Poster.IProperty;
using Propnex.Poster.Share;
using System.Security.Policy;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using HtmlElement = CefSharp.Dom.HtmlElement;

namespace Propnex.Poster.NetCoreWinForm
{
    public partial class IPropertyCefPoster : Form, ITransientDependency, IPosterStart
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IPropnexTaskProvider _propnexTaskProvider;

        private PropnexTasks propnexTasks;
        private PropnexTask propnexTask;

        private DevToolsContext DevToolsContext;

        private static object _lock = new object();

        public IPropertyCefPoster(ILocalEventBus localEventBus, IPropnexTaskProvider propnexTaskProvider)
        {
            InitializeComponent();
            _localEventBus = localEventBus;
            _propnexTaskProvider = propnexTaskProvider;
        }

        public async Task Start()
        {
            await PublishMessageAsync("Start a new task");
            await GetTask();
            foreach (var item in propnexTasks.Tasks)
            {
                propnexTask = item;
                await Login();
                var listings = await GetListings();
            }
            Close();
        }

        public async Task Login()
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
            await CheckPage();
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
        }

        public async Task<List<Listing>> GetListings()
        {
            string url = $"https://www.iproperty.com.my/pro/rasor/graphql/listingsQuery?" +
                $"operationName=listingsQuery&variables=%7B%22shouldExtendsFields%22%3Atrue%2C%22statusCode%22%3A{2}%2C%22isExcludeChild%22%3Afalse%2C%22sortBy%22%3A%22new-to-old%22%2C%22limit%22%3A500%2C%22page%22%3A%221%22%2C%22includeReAdvertiseJob%22%3Atrue%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%228893c19fbd672297adbdd3bf3eba0c22544d6ef0517a2c3153f36b2c64f86659%22%7D%7D";
            string jscode = $@"()=> {{return fetch(""{url}"", {{
                                    ""headers"": {{
                                        ""accept"": ""application/json, text/plain, */*"",
                                        ""if-none-match"": ""W/\""42c3f-i6z2s6ipfF/j1sd6HDcrj3E+{new Random(_lock.GetHashCode()).Next(1000)}\"""",
                                    }},
                                    ""method"": ""GET"",
                                    ""mode"": ""cors"",
                                    ""credentials"": ""include""
                                }}).then(res=>{{
                                      return res.json()
                                }})}}";
            try
            {
                var javaReposnse = await DevToolsContext.EvaluateFunctionAsync<RequestData<ListingsData>>(jscode);
                return javaReposnse.Data.listings.Data;
            }
            catch (Exception ex)
            {
                await Delay(60);
            }

            return new List<Listing>();
        }

        public async Task CheckPage()
        {
            var gRecaptcha = await DevToolsContext.QuerySelectorAsync(".g-recaptcha");
            if (gRecaptcha != null)
            {

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
            propnexTasks = _propnexTaskProvider.GetTasks(System.IO.File.ReadAllText("E:\\2504.tsk"));
            if (propnexTasks == null)
            {
                await PublishMessageAsync("Not find tasks ,dealy 1 min");
                await Task.Delay(1000 * 60);
                Close();
            }
        }

        public async Task PublishMessageAsync(string message)
        {
            await _localEventBus.PublishAsync(new LogEvent()
            {
                Message = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}-{message}"
            });
        }
    }
}