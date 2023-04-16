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

        public PosterActionResult ToPosterActionResult()
        {
            return new PosterActionResult()
            {
                Data = Data,
                Message = Message
            };
        }
    }

    public class PosterActionResult : PosterActionResult<object>
    {
    }


    public enum PosterActionResultStatus
    {
        Success,
        Error,
        Expection
    }
}
