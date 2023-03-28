using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Propnex.Poster.NetCoreWinForm
{
    [DependsOn(typeof(AbpAutofacModule))]
    public class NetCoreWinFormModule : AbpModule
    {
    }
}
