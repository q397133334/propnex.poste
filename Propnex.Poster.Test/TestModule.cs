using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Propnex.Poster.PropertyGuru.Mobile;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Propnex.Poster.Test
{
    [DependsOn(
    typeof(AbpAutofacModule)
)]
    public class TestModule : Volo.Abp.Modularity.AbpModule
    {
        public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<TestModule>>();
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            logger.LogInformation($"MySettingName => {configuration["MySettingName"]}");

            var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
            logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");

          
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            //注册一个singleton实例
            context.Services.AddTransient<Auth>();
            context.Services.AddTransient<Mobile>();
            context.Services.AddTransient<Api>();
            context.Services.AddTransient<ProjectsApi>();
            context.Services.AddTransient<IPropnexTaskProvider,DefaultPropnexTaskProvider>();
        }
    }
}
