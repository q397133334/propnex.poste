using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropnexPoster.Console
{
    public class HostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public HostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var run = _serviceProvider.GetService<PosterRun>();//  new PosterRun();
                run.MessageEvent = (message, s) =>
                {
                    System.Console.WriteLine(message);
                };
                run.TaskInfoEvent = (a) => { };
                await run.Run();
                run = null;
                System.Console.WriteLine("complage");
                await Task.Delay(1000);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
