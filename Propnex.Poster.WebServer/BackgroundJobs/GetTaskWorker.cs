using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Propnex.Poster.WebServer.Entities;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskStatus = Propnex.Poster.Share.TaskStatus;
using Propnex.Poster.WebServer.Data;
using Pomelo.EntityFrameworkCore.MySql.Query.Internal;

namespace Propnex.Poster.WebServer.BackgroundJobs
{
    public class GetTaskWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public GetTaskWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 60 * 1000; //1 minutes
        }


        [Volo.Abp.Uow.UnitOfWork(false)]
        protected async override Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            #if DEBUG
            return;
            #endif
            var _repositoryPntask = workerContext.ServiceProvider.GetService<IRepository<PnTask>>();
            IPnTaskLogRepository _pnTaskLogRepository = workerContext.ServiceProvider.GetService<IPnTaskLogRepository>();

            var pn = await (WebServerConsts.PnBaseUrl + WebServerConsts.PnfetchGuruTasks).GetStringAsync();
            var list = pn.Split('\n');
            foreach (var item in list)
            {
                var tsk = item.Split('\t');
                if (tsk.Length == 2)
                {
                    var waitTask = (await _repositoryPntask.GetQueryableAsync()).Where(q => q.Number == tsk[0]).OrderByDescending(q => q.CreationTime).FirstOrDefault();
                    if (waitTask == null)
                    {
                        waitTask = await _repositoryPntask.InsertAsync(new PnTask()
                        {
                            Number = tsk[0],
                            ClientId = tsk[1],
                            Status = TaskStatus.Wait
                        });
                        await _pnTaskLogRepository.InsertAsync(waitTask.Id, Guid.Empty, "Init Task", "");
                    }
                    else
                    {
                        if (waitTask.Status != TaskStatus.Wait)
                        {
                            if (waitTask.LastModificationTime.HasValue && waitTask.LastModificationTime < DateTime.Now.AddDays(-1))
                            {
                                waitTask.Status = TaskStatus.Wait;
                                await _repositoryPntask.UpdateAsync(waitTask);
                                await _pnTaskLogRepository.InsertAsync(waitTask.Id, Guid.Empty, "Reset Task", "");
                            }
                        }
                    }
                }
            }

            pn = await (WebServerConsts.PnBaseUrl + WebServerConsts.PnfetchGuruTasks + "?xweb=1").GetStringAsync();
            list = pn.Split('\n');
            foreach (var item in list)
            {
                var tsk = item.Split('\t');
                if (tsk.Length == 2)
                {
                    var waitTask = (await _repositoryPntask.GetQueryableAsync()).Where(q => q.Number == tsk[0]).OrderByDescending(q => q.CreationTime).FirstOrDefault();
                    if (waitTask == null)
                    {
                        waitTask = await _repositoryPntask.InsertAsync(new PnTask()
                        {
                            Number = tsk[0],
                            ClientId = tsk[1],
                            Status = TaskStatus.Wait
                        });
                        await _pnTaskLogRepository.InsertAsync(waitTask.Id, Guid.Empty, "Init Task", "");
                    }
                    else
                    {
                        if (waitTask.Status != TaskStatus.Wait)
                        {
                            if (waitTask.LastModificationTime.HasValue && waitTask.LastModificationTime < DateTime.Now.AddDays(-1))
                            {
                                waitTask.Status = TaskStatus.Wait;
                                await _repositoryPntask.UpdateAsync(waitTask);
                                await _pnTaskLogRepository.InsertAsync(waitTask.Id, Guid.Empty, "Reset Task", "");
                            }
                        }
                    }
                }
            }
        }
    }
}
