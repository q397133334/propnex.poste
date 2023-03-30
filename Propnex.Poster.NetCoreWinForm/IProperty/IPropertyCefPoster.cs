using CefSharp;
using CefSharp.Dom;
using CefSharp.WinForms;
using Propnex.Poster.Share;
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
            }
            Close();
        }

        public async Task Login()
        {
            var loginUrl = "https://www.iproperty.com.my/pro/listings?lang=en-GB";
            await chromiumWebBrowser.LoadUrlAsync("https://www.baidu.com");
            await PublishMessageAsync("DeleteCookie");
            var cookieManager = chromiumWebBrowser.GetCookieManager();
            await cookieManager.DeleteCookiesAsync();
            await PublishMessageAsync($"LoadUrl:{loginUrl}");
            await chromiumWebBrowser.LoadUrlAsync(loginUrl);
            await watiForIsLoading();
            DevToolsContext = await chromiumWebBrowser.CreateDevToolsContextAsync();
            chromiumWebBrowser.ShowDevTools();

            await Delay(60);

            //check page
            await CheckPage();
            //input login user name
            var userNameInput = await DevToolsContext.QuerySelectorAsync<HtmlElement>("#login-userid");
            await Delay(60);
            await userNameInput.SetAttributeAsync("value",propnexTask.Account);
            //input password
            var userPasswordInput = await DevToolsContext.QuerySelectorAsync<HtmlElement>("#login-password");
            await Delay();
            await userPasswordInput.SetAttributeAsync("value",propnexTask.Password);
            //login button
            var loginButton = await DevToolsContext.QuerySelectorAsync<HtmlElement>("#btn_login");
            await Delay(1);
            await loginButton.ClickAsync();
            await Delay();

            await watiForIsLoading();
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
            propnexTasks = _propnexTaskProvider.GetTasks(System.IO.File.ReadAllText("C:\\Users\\worker_fg\\Downloads\\2504.tsk"));
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