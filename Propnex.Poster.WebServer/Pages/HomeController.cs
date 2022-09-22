using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Propnex.Poster.WebServer.Pages
{
    [Route("/")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("/api/downloadtask")]
        [HttpGet]
        public FileResult DownloadTask(Guid taskId, string fileName)
        {
            var fileStrem =  System.IO.File.OpenRead(Path.Combine(_webHostEnvironment.WebRootPath, "taskxml",fileName));
            return File(fileStrem, "text/plain", fileName);
        }
    }
}
