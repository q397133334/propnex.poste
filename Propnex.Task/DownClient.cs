using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Propnex
{
    public class DownClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = 1000 * 60 * 3;
            return request;
        }
    }
}
