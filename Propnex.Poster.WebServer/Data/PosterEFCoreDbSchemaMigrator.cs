using Microsoft.EntityFrameworkCore;
using Propnex.Poster.WebServer.Data;
using Volo.Abp.DependencyInjection;

namespace Propnex.Poster.Data;

public class PosterEFCoreDbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public PosterEFCoreDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the PosterDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<PosterDbContext>()
            .Database
            .MigrateAsync();
    }
}
