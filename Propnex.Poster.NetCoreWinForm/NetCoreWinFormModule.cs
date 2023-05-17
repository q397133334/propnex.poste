using AutoUpdaterDotNET;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Propnex.Poster.Share;
using PropnexPoster.NetCoreWinForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace Propnex.Poster.NetCoreWinForm
{
    [DependsOn(typeof(AbpAutofacModule))]
    [DependsOn(typeof(AbpEventBusModule))]
    public class NetCoreWinFormModule : AbpModule
    {
        public static AppConfiguration AppConfiguration { get; set; }
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IPropnexTaskProvider, DefaultPropnexTaskProvider>();
        }

        public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var configuration = context.ServiceProvider.GetService<IConfiguration>();
            AppConfiguration = context.ServiceProvider.GetService<AppConfiguration>();
            WebServer.BaseUrl = configuration["BaseUrl"];
            WebServer.MachindNumber = (await WebServer.GetMachineIdAsync(configuration["MachineNumber"])).Trim('\"');
            AppConfiguration = configuration.Get<AppConfiguration>();
            await Task.CompletedTask;
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            AutoUpdater.Start("http://testposter.propnex.net/PropnexPoster.IProperty/PropnexPoster.IProperty.AutoUpdater.xml");
        }
    }
}
