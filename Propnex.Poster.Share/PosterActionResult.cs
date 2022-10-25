using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.Share
{
    public class PosterActionResult<T>
    {
        public string Message { get; set; }

        public PosterActionResultStatus Status { get; set; }

        public T Data { get; set; }
    }

    public class PosterActionResult
    {
        public string Message { get; set; }

        public PosterActionResultStatus Status { get; set; }

        public Object Data { get; set; }
    }


    public enum PosterActionResultStatus
    {
        Success,
        Error,
        Expection
    }
}
