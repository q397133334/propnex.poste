using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Autofac;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace Propnex.Poster.NetCoreWinForm
{
    [DependsOn(typeof(AbpAutofacModule))]
    [DependsOn(typeof(AbpEventBusModule))]
    public class NetCoreWinFormModule : AbpModule
    {

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IPropnexTaskProvider, DefaultPropnexTaskProvider>();
        }
    }
}
