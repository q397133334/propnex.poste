using Propnex.Poster.WebServer.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Nito.AsyncEx;
using Flurl.Http;
using System.Text;

namespace Propnex.Poster.WebServer.Services
{
    public interface IMachineAppService : Volo.Abp.Application.Services.IApplicationService
    {
        Task<Guid> GetIdAsync(string anyDesk);
    }

    public class MachineAppService : ApplicationService, IMachineAppService
    {

        private readonly IRepository<Machine> _repositoryMachine;

        public MachineAppService(IRepository<Machine> repositoryMachine)
        {
            _repositoryMachine = repositoryMachine;
        }

        public async Task<Guid> GetIdAsync(string anyDesk)
        {
            var machine = await AsyncExecuter.FirstOrDefaultAsync((await _repositoryMachine.GetQueryableAsync()).Where(q => q.Number == anyDesk));
            if (machine == null)
            {
                machine = new Machine()
                {
                    Number = anyDesk
                };
                machine = await _repositoryMachine.InsertAsync(machine);
            }
            return machine.Id;
        }
    }
}
