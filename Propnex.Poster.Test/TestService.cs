using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Propnex.Poster.Test
{
    public class TestService : ITransientDependency
    {
        public ILogger<TestService> Logger { get; set; }

        private readonly IPropnexTaskProvider _propnexTaskProvider;
        public TestService(IPropnexTaskProvider propnexTaskProvider)
        {
            Logger = NullLogger<TestService>.Instance;
            _propnexTaskProvider = propnexTaskProvider;
        }

        public async Task SayHelloAsync()
        {
            _propnexTaskProvider.Get(File.ReadAllText("C:\\Users\\worker_fg\\Downloads\\2504.tsk"));
        }
    }
}
