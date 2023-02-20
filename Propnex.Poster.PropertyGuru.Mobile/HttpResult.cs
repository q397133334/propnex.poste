using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public class HttpResult<T>
    {
        public T Data { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string Message { get; set; }
    }
}
