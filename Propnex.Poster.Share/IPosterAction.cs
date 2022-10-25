using Propnex.Poster.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.Share
{
    public interface IPosterAction
    {
        

        Task<PosterActionResult> Login(string userName,string password);

        Task<PosterActionResult> PostOnly();

        Task<PosterActionResult> Post();

        Task<PosterActionResult> Update();

        Task<PosterActionResult> Repost();

        Task<PosterActionResult> Retrieve();
    }
}
