using CefSharp;

namespace Propnex.Poster.NetCoreWinForm
{
    public partial class IPropertyCefPoster : Form, Share.IPosterStart
    {
        public IPropertyCefPoster()
        {
            InitializeComponent();
        }

        public async void Start()
        {
            await chromiumWebBrowser.LoadUrlAsync("https://www.baidu.com");
            var cookieManager = chromiumWebBrowser.GetCookieManager();
            await cookieManager.DeleteCookiesAsync();
            chromiumWebBrowser.Reload();
        }
    }
}