using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace PropnexPoster.WPF;

[DependsOn(typeof(AbpAutofacModule))]
public class WPFModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<MainWindow>();
        context.Services.AddTransient<PosterRun>();
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var configuration = context.ServiceProvider.GetService<IConfiguration>();
        WebServer.BaseUrl = configuration["BaseUrl"];
        WebServer.MachindNumber = (await WebServer.GetMachineIdAsync(configuration["MachineNumber"])).Trim('\"');
        await WebServer.PingAsync();
           
    }
}
