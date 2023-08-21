using Propnex.Poster.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.NetCoreWinForm
{
    public class CefFrom : Form, IPosterStart 
    {
        public virtual Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
