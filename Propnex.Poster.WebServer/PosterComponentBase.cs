using Propnex.Poster.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Propnex.Poster;

public abstract class PosterComponentBase : AbpComponentBase
{
    protected PosterComponentBase()
    {
        LocalizationResource = typeof(PosterResource);
    }
}
