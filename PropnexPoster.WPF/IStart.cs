using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropnexPoster.WPF
{
    public interface IStart
    {
        Task StartAsync();

        void Start();
    }
}
