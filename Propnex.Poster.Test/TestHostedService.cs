using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Volo.Abp;


namespace Propnex.Poster.Test
{
    public class TestHostedService : IHostedService
    {
        private readonly IAbpApplicationWithExternalServiceProvider _abpApplication;
        private readonly TestService _testService;

        public TestHostedService(TestService testService, IAbpApplicationWithExternalServiceProvider abpApplication)
        {
            _abpApplication = abpApplication;
            _testService = testService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _testService.SayHelloAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _abpApplication.ShutdownAsync();
        }
    }
}
