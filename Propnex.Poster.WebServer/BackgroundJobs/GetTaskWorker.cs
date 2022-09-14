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

namespace Propnex.Poster.WebServer.BackgroundJobs
{
    public class GetTaskWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public GetTaskWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 60 * 1000; //10 minutes
        }

        protected async override Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var _repositoryPntask = workerContext.ServiceProvider.GetService<IRepository<PnTask>>();

            var pn = await "https://pa-production.propnex.net/index.php/tasks/fetchGuruTasks?xweb=1".GetStringAsync();
            var list = pn.Split('\n');
            foreach (var item in list)
            {
                var tsk = item.Split('\t');
                if (tsk.Length == 2)
                {
                    if ((await _repositoryPntask.FindAsync(q => q.Number == tsk[0])) == null)
                    {
                        await _repositoryPntask.InsertAsync(new PnTask()
                        {
                            Number = tsk[0],
                            Status = TaskStatus.Wait
                        });
                    }
                }
            }
        }
    }
}
