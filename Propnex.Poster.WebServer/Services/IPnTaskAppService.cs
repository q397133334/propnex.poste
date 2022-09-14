using Propnex.Poster.WebServer.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Nito.AsyncEx;
using Flurl.Http;
using System.Text;

namespace Propnex.Poster.WebServer.Services
{

    public interface IPnTaskAppService : ICrudAppService< //Defines CRUD methods
            Dtos.PnTaskDto, //Used to show books
            Guid, //Primary key of the book entity
            PagedAndSortedResultRequestDto, //Used for paging/sorting
            Dtos.CreateUpdatePnTaskDto> //Used
    {

    }

    public class PnTaskAppService : CrudAppService<
            Entities.PnTask, //The Book entity
            Dtos.PnTaskDto, //Used to show books
            Guid, //Primary key of the book entity
            PagedAndSortedResultRequestDto, //Used for paging/sorting
            Dtos.CreateUpdatePnTaskDto>, IPnTaskAppService
    {

        private readonly AsyncLock _Mutex = new AsyncLock();//锁

        private readonly IPnTaskLogRepository _pnTaskLogRepository;

        public PnTaskAppService(IRepository<PnTask, Guid> repository, IPnTaskLogRepository pnTaskLogRepository) : base(repository)
        {
            _pnTaskLogRepository = pnTaskLogRepository;
        }


        public async Task<Dtos.PnTaskDto> GetPnTaskAsync(Dtos.InputGetTaskInfoDto inputDto)
        {
            using (await _Mutex.LockAsync())
            {
                //1. get waiting pntask
                var pnTask = await AsyncExecuter.FirstOrDefaultAsync((await Repository.GetQueryableAsync()).Where(q => q.Status == Share.TaskStatus.Wait));
                if (pnTask == null)
                    return null;
                //2. check task file
                var downloadUrl = $"{WebServerConsts.PnBaseUrl}{WebServerConsts.PnreadGuruTask}?client_id={pnTask.ClientId}&fileName={pnTask.Number}";
                //3. download task file
                var taskContext = await downloadUrl.GetStringAsync();
                //4. return pntasks
                if (taskContext == "Can't find task file.")
                {
                    await _pnTaskLogRepository.InsertAsync(inputDto.MachineId, pnTask.Id, "Can't find task file.", "");
                    pnTask.Status = Share.TaskStatus.NotFind;
                    await Repository.UpdateAsync(pnTask);
                    return new Dtos.PnTaskDto()
                    {
                        Status = Share.TaskStatus.NotFind
                    };
                }
                //5. save task
                if (Directory.Exists($"{Environment.CurrentDirectory}\\TaskXml") == false)
                {
                    Directory.CreateDirectory($"{Environment.CurrentDirectory}\\TaskXml");
                }
                File.WriteAllText($"{Environment.CurrentDirectory}\\taskxml\\{pnTask.Number}", taskContext, Encoding.UTF8);
                return new Dtos.PnTaskDto()
                {
                    Id = pnTask.Id,
                    AccountId = pnTask.AccountId,
                    Number = pnTask.Number,
                    Country = pnTask.Country,
                    CreationTime = pnTask.CreationTime,
                    Password = pnTask.Password,
                    Source = pnTask.Source,
                    Status = pnTask.Status
                };
            }
        }
    }

}
