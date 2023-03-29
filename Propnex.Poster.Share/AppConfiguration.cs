using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.Share
{
    public class AppConfiguration
    {

        public AppConfiguration() { }

        public string BaseUrl { get; set; }

        public string MachineNumber { get; set; }

        public bool IsProxy { get; set; }

        public List<string> ProxyIps { get; set; }
    }
}
