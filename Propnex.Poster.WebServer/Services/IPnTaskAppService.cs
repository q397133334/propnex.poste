using Propnex.Poster.WebServer.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Nito.AsyncEx;
using Flurl.Http;
using System.Text;
using Propnex.Poster.Dtos;

namespace Propnex.Poster.WebServer.Services
{

    public interface IPnTaskAppService : ICrudAppService< //Defines CRUD methods
            Dtos.PnTaskDto, //Used to show books
            Guid, //Primary key of the book entity
            PagedAndSortedResultRequestDto, //Used for paging/sorting
            Dtos.CreateUpdatePnTaskDto> //Used
    {
        Task<Dtos.PnTaskDto> GetPnTaskAsync(Dtos.InputGetTaskInfoDto inputDto);

        Task PnTaskRetry(Guid Machineid, Guid pnTaskId, string message = "");
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PnTaskAppService(IRepository<PnTask, Guid> repository, IPnTaskLogRepository pnTaskLogRepository, IWebHostEnvironment webHostEnvironment) : base(repository)
        {
            _pnTaskLogRepository = pnTaskLogRepository;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<Dtos.PnTaskDto> GetPnTaskAsync(Dtos.InputGetTaskInfoDto inputDto)
        {
            using (await _Mutex.LockAsync())
            {
                var rootPath = Path.Combine(_webHostEnvironment.WebRootPath, "taskxml");
                //1. get waiting pntask
                var pnTask = await AsyncExecuter.FirstOrDefaultAsync((await Repository.GetQueryableAsync()).Where(q => q.Status == Share.TaskStatus.Wait));
                if (pnTask == null)
                    return null;
                //2. check task file
                var downloadUrl = $"{WebServerConsts.PnBaseUrl}{WebServerConsts.PnreadGuruTask}?client_id={pnTask.ClientId}&fileName={pnTask.Number}";
                //3. download task file
                var taskContext = await downloadUrl.GetStringAsync();
                //4. return pntasks
                if (taskContext == "Can't find task file." && File.Exists(Path.Combine(rootPath, pnTask.Number)) == false)
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
                if (Directory.Exists(rootPath) == false)
                {
                    Directory.CreateDirectory(rootPath);
                }
                if (taskContext != "Can't find task file.")
                {
                    await _pnTaskLogRepository.InsertAsync(inputDto.MachineId, pnTask.Id, "Dwonload Success", "");
                    File.WriteAllText(Path.Combine(rootPath, pnTask.Number), taskContext, Encoding.UTF8);
                }
                await _pnTaskLogRepository.InsertAsync(inputDto.MachineId, pnTask.Id, "Get PnTask", "");

                pnTask.Status = Share.TaskStatus.Runing;
                await Repository.UpdateAsync(pnTask);

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

        public async Task PnTaskRetry(Guid machineId, Guid pnTaskId, string message = "")
        {
            var pnTask = await AsyncExecuter.FirstOrDefaultAsync((await Repository.GetQueryableAsync()).Where(q => q.Id == pnTaskId));
            if (pnTask != null && pnTask.RetryCount <= 3)
            {
                pnTask.RetryCount += 1;
                pnTask.Status = Share.TaskStatus.Wait;
                await Repository.UpdateAsync(pnTask);
                await _pnTaskLogRepository.InsertAsync(machineId, pnTask.Id, $"Set Retry {pnTask.RetryCount}, {message}", "");



                var rootPath = Path.Combine(_webHostEnvironment.WebRootPath, "taskxml");
                var usePath = Path.Combine(_webHostEnvironment.WebRootPath, "usetaskxml");
                if (File.Exists(Path.Combine(usePath, pnTask.Number)))
                {
                    System.IO.File.Move(Path.Combine(usePath, pnTask.Number), Path.Combine(rootPath, pnTask.Number));
                }
            }
            else
            {
                pnTask.Status = Share.TaskStatus.Failure;
                await Repository.UpdateAsync(pnTask);
                await _pnTaskLogRepository.InsertAsync(machineId, pnTask.Id, $"set Retry,but retyr max count {pnTask.RetryCount},{message}", "");
            }
        }
    }

}
