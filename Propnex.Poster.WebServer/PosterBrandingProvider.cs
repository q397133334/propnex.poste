using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Propnex.Poster;

[Dependency(ReplaceServices = true)]
public class PosterBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Poster";
}
