using Propnex.Poster.WebServer.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Nito.AsyncEx;
using Flurl.Http;
using System.Text;
using Propnex.Poster.Dtos;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Volo.Abp.Uow;
using Polly;

namespace Propnex.Poster.WebServer.Services
{

    public interface IPnTaskAppService : ICrudAppService< //Defines CRUD methods
            Dtos.PnTaskDto, //Used to show books
            Guid, //Primary key of the book entity
            Dtos.PnTaskListInput, //Used for paging/sorting/filtering
            Dtos.CreateUpdatePnTaskDto> //Used
    {
        Task<Dtos.PnTaskDto> GetPnTaskAsync(Dtos.InputGetTaskInfoDto inputDto);

        Task PnTaskRetry(Guid? Machineid, Guid pnTaskId, string message = "");

        Task PnTaskXmlRetry(Guid pnTaskId, string message = "");

        Task<List<PnTaskDto>> GetWaitPnTaskAsync();

        Task CreatePropertyTasks(CreatePropertyTaskDto input);

        Task ResetPnTask(Guid machineId, Guid pnTaskId, string message = "");

        Task<List<PnTaskLogDto>> GetLogsAsync(Guid pnTaskId);
    }

    public class PnTaskAppService : CrudAppService<
            Entities.PnTask, //The Book entity
            Dtos.PnTaskDto, //Used to show books
            Guid, //Primary key of the book entity
            Dtos.PnTaskListInput, //Used for paging/sorting/filtering
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

        protected override async Task<IQueryable<Entities.PnTask>> CreateFilteredQueryAsync(Dtos.PnTaskListInput input)
        {
            var query = await base.CreateFilteredQueryAsync(input);
            if (!string.IsNullOrWhiteSpace(input.NumberFilter))
            {
                query = query.Where(t => t.Number.Contains(input.NumberFilter));
            }
            return query;
        }

        public async Task CreatePropertyTasks(CreatePropertyTaskDto input)
        {
            if (input.MaxId < input.MinId)
            {
                throw new Volo.Abp.UserFriendlyException("StartId can not max EndId");
            }
            List<PnTask> list = new List<PnTask>();
            for (int i = input.MinId; i <= input.MaxId; i++)
            {
                list.Add(new PnTask()
                {
                    Number = i.ToString(),
                    ClientId = "1",
                    Status = Share.TaskStatus.Wait,
                    TargetPortal = "PropertyData"
                }); ;
            }
            await Repository.InsertManyAsync(list);
        }

        public async Task<Dtos.PnTaskDto> GetPnTaskAsync(Dtos.InputGetTaskInfoDto inputDto)
        {
            if (string.IsNullOrEmpty(inputDto.TargetPortal))
            {
                inputDto.TargetPortal = "GURU";
            }
            using (await _Mutex.LockAsync())
            {
                var rootPath = Path.Combine(_webHostEnvironment.WebRootPath, "taskxml");
                //1. get waiting pntask
                var pnTask = await AsyncExecuter.FirstOrDefaultAsync((await Repository.GetQueryableAsync()).Where(q => q.Status == Share.TaskStatus.Wait && q.TargetPortal == inputDto.TargetPortal).OrderBy(q=>q.CreationTime));
                //PnTask pnTask = await AsyncExecuter.FirstOrDefaultAsync((await Repository.GetQueryableAsync()).Where(q => q.Number == "cp17733240140309.guru.tsk"));
                if (pnTask == null)
                    return null;
                //2. check task file
                var downloadUrl = $"{WebServerConsts.PnBaseUrl}{WebServerConsts.PnreadGuruTask}?client_id={pnTask.ClientId}&fileName={pnTask.Number}";
                //if (inputDto.TargetPortal == "MyIP")
                //{
                //    downloadUrl = $"{WebServerConsts.PnreadMyIpTask}?client_id={pnTask.ClientId}&fileName={pnTask.Number}";
                //}
                //3. download task file
                var taskContext = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                    .ExecuteAsync(() => downloadUrl.GetStringAsync());
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
                    await _pnTaskLogRepository.InsertAsync(pnTask.Id, inputDto.MachineId, "Dwonload Success", "");
                    File.WriteAllText(Path.Combine(rootPath, pnTask.Number), taskContext, Encoding.UTF8);
                }
                await _pnTaskLogRepository.InsertAsync(pnTask.Id, inputDto.MachineId, "Get PnTask", "");

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

        public async Task<List<PnTaskDto>> GetWaitPnTaskAsync()
        {
            var pnTasks = await AsyncExecuter.ToListAsync((await Repository.GetQueryableAsync()).Where(q => q.Status == Share.TaskStatus.Wait));

            var lists = new List<PnTaskDto>();

            foreach (var pnTask in pnTasks)
            {
                lists.Add(new PnTaskDto()
                {
                    //Id = pnTask.Id,
                    //AccountId = pnTask.AccountId,
                    Number = pnTask.Number,
                    //Country = pnTask.Country,
                    //CreationTime = pnTask.CreationTime,
                    //Password = pnTask.Password,
                    //Source = pnTask.Source,
                    //Status = pnTask.Status
                });
            }
            return lists;
        }

        public async Task PnTaskRetry(Guid? machineId, Guid pnTaskId, string message = "")
        {
            if(machineId.HasValue==false)
            {
                machineId = Guid.NewGuid();
            }
            var pnTask = await AsyncExecuter.FirstOrDefaultAsync((await Repository.GetQueryableAsync()).Where(q => q.Id == pnTaskId));
            if (pnTask != null && pnTask.RetryCount <= 5)
            {
                pnTask.RetryCount += 1;
                pnTask.Status = Share.TaskStatus.Wait;
                await Repository.UpdateAsync(pnTask);
                await _pnTaskLogRepository.InsertAsync(machineId.Value, pnTask.Id, $"Set Retry {pnTask.RetryCount}, {message}", "");



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
                await _pnTaskLogRepository.InsertAsync(machineId.Value, pnTask.Id, $"set Retry,but retyr max count {pnTask.RetryCount},{message}", "");
            }
        }

        public async Task ResetPnTask(Guid machineId, Guid pnTaskId, string message = "")
        {
            var pnTask = await AsyncExecuter.FirstOrDefaultAsync(
                (await Repository.GetQueryableAsync()).Where(q => q.Id == pnTaskId));
            if (pnTask == null) return;

            pnTask.Status = Share.TaskStatus.Wait;
            pnTask.RetryCount = 0;
            await Repository.UpdateAsync(pnTask);
            await _pnTaskLogRepository.InsertAsync(machineId, pnTask.Id, $"Reset task, {message}", "");

            var rootPath = Path.Combine(_webHostEnvironment.WebRootPath, "taskxml");
            var usePath = Path.Combine(_webHostEnvironment.WebRootPath, "usetaskxml");
            if (File.Exists(Path.Combine(usePath, pnTask.Number)))
            {
                File.Move(Path.Combine(usePath, pnTask.Number), Path.Combine(rootPath, pnTask.Number));
            }
        }

        public async Task PnTaskXmlRetry(Guid pnTaskId, string message = "")
        {
            var pnTask = await AsyncExecuter.FirstOrDefaultAsync((await Repository.GetQueryableAsync()).Where(q => q.Id == pnTaskId));
            if (pnTask != null)
            {
                await _pnTaskLogRepository.InsertAsync(Guid.Empty, pnTask.Id, $"{message}", "");

                var rootPath = Path.Combine(_webHostEnvironment.WebRootPath, "taskxml");
                var usePath = Path.Combine(_webHostEnvironment.WebRootPath, "usetaskxml");
                if (File.Exists(Path.Combine(usePath, pnTask.Number)))
                {
                    System.IO.File.Move(Path.Combine(usePath, pnTask.Number), Path.Combine(rootPath, pnTask.Number));
                }
            }
        }

        public async Task<List<PnTaskLogDto>> GetLogsAsync(Guid pnTaskId)
        {
            var logs = await AsyncExecuter.ToListAsync(
                (await _pnTaskLogRepository.GetQueryableAsync())
                .Where(l => l.PntaskId == pnTaskId)
                .OrderByDescending(l => l.CreateTime));

            return logs.Select(l => new PnTaskLogDto
            {
                PntaskId = l.PntaskId,
                MachineId = l.MachineId,
                Ip = l.Ip,
                Message = l.Message,
                CreateTime = l.CreateTime
            }).ToList();
        }
    }

}
