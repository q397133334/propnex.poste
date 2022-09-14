using Propnex.Poster.WebServer.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Propnex.Poster.WebServer.Data
{
    public class PnTaskLogRepository : EfCoreRepository<PosterDbContext, Entities.PnTaskLog, Guid>, Entities.IPnTaskLogRepository
    {
        public PnTaskLogRepository(IDbContextProvider<PosterDbContext> dbContextProvider) : base(dbContextProvider)
        {

        }

        public async Task InsertAsync(Guid pnTaskId, Guid machineId, string message, string ip)
        {
            await InsertAsync(new PnTaskLog()
            {
                CreateTime = DateTime.Now,
                MachineId = machineId,
                PntaskId = pnTaskId,
                Ip = ip,
                Message = message
            });
        }
    }
}
