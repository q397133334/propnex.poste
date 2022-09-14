using Volo.Abp.Domain.Repositories;

namespace Propnex.Poster.WebServer.Entities
{
    public interface IMachineRepository : IRepository<Machine, Guid>
    {
    }

    public interface IPnTaskLogRepository : IRepository<PnTaskLog, Guid>
    {
        Task InsertAsync(Guid pnTaskId, Guid machineId, string message, string ip);
    }
}
