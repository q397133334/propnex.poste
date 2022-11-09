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
            var fileStrem = System.IO.File.ReadAllBytes(Path.Combine(_webHostEnvironment.WebRootPath, "taskxml", fileName));
            var fileResult = File(fileStrem, "text/plain", fileName);

            System.IO.File.Move(Path.Combine(_webHostEnvironment.WebRootPath, "taskxml", fileName), Path.Combine(_webHostEnvironment.WebRootPath, "usetaskxml", fileName));

            return fileResult;
        }
    }
}
