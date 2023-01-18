using Propnex.Poster.WebServer.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Nito.AsyncEx;
using Flurl.Http;
using System.Text;
using System.Linq.Expressions;

namespace Propnex.Poster.WebServer.Services
{
    public interface IMachineAppService : Volo.Abp.Application.Services.IApplicationService
    {
        Task<Guid> GetIdAsync(string anyDesk);

        Task<List<Dtos.MachineDto>> GetMachines();

        Task UpdateOnline(Guid id);
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

        public async Task<List<Dtos.MachineDto>> GetMachines()
        {
            var machines = await AsyncExecuter.ToListAsync((await _repositoryMachine.GetQueryableAsync()).Where(q => q.IsEnable));

            var machinesDto = new List<Dtos.MachineDto>();

            foreach (var machine in machines)
            {

                machinesDto.Add(new Dtos.MachineDto()
                {
                    Number = machine.Number,
                    OnlineTime = machine.OnlineTime,
                });
            }
            return machinesDto;
        }

        public async Task UpdateOnline(Guid id)
        {
            var machine = await _repositoryMachine.GetAsync(q => q.Id == id);
            if (machine != null)
            {
                machine.OnlineTime = DateTime.Now;
                await _repositoryMachine.UpdateAsync(machine);
            }
        }
    }
}
