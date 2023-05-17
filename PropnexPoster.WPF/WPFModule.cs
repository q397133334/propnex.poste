using AutoUpdaterDotNET;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Propnex.Poster.Share;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace PropnexPoster.WPF;

[DependsOn(typeof(AbpAutofacModule))]
public class WPFModule : AbpModule
{
    public static AppConfiguration AppConfiguration { get; set; }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<MainWindow>();
        //context.Services.AddTransient<PosterRun>();
        context.Services.AddSingleton<Propnex.Poster.Share.AppConfiguration>();
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var configuration = context.ServiceProvider.GetService<IConfiguration>();
        AppConfiguration = context.ServiceProvider.GetService<AppConfiguration>();
        WebServer.BaseUrl = configuration["BaseUrl"];
        WebServer.MachindNumber = (await WebServer.GetMachineIdAsync(configuration["MachineNumber"])).Trim('\"');
        AppConfiguration = configuration.Get<AppConfiguration>();
    }
    public override async Task OnPreApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        AutoUpdater.Start("http://testposter.propnex.net/PropnexPoster.Guru/PropnexPoster.WPF.AutoUpdater.xml");
        await Task.CompletedTask;
    }
}
