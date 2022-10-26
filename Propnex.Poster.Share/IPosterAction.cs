using Propnex.Poster.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.Share
{
    public interface IPosterAction<T>
    {
        string Source { get; set; }

        Task<PosterActionResult> Login(string userName, string password);

        Task<PosterActionResult> PostOnly(T task);

        Task<PosterActionResult> Post(T task);

        Task<PosterActionResult> Update(T task);

        Task<PosterActionResult> Repost(T task);

        Task<PosterActionResult> Remove(T task);

        Task<PosterActionResult> Retrieve(T task);
    }
}
