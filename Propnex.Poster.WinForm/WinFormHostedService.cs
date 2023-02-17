using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.WinForm
{
    public class WinFormHostedService : IHostedService
    {

        private readonly Form1 main;

        public WinFormHostedService(Form1 form)
        {
            main = form;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Application.Run(main);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Application.Exit();
        }
    }
}
