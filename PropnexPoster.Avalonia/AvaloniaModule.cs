using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Propnex.Poster.Share;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace PropnexPoster.Avalonia;

[DependsOn(typeof(AbpAutofacModule))]
public class AvaloniaModule : AbpModule
{
    public static AppConfiguration AppConfiguration { get; set; }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<MainWindow>();
        context.Services.AddSingleton<AppConfiguration>();
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var configuration = context.ServiceProvider.GetService<IConfiguration>();
        WebServer.BaseUrl = configuration["BaseUrl"];
        WebServer.MachindNumber = (await WebServer.GetMachineIdAsync(configuration["MachineNumber"])).Trim('\"');
        AppConfiguration = configuration.Get<AppConfiguration>();
    }
}
