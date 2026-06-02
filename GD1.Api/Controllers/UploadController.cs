using GD1.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public UploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Uploads a file (camera capture or selected file) and returns the URL.
        /// </summary>
        [HttpPost("upload-file")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(104857600)]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        //public async Task<IActionResult> Upload(IFormFile file)
        //{
        //    if (file == null) return BadRequest("No file uploaded.");

        //    var url = await _fileService.SaveFileAsync(file, "uploads");

        //    return Ok(new { 
        //        url,
        //        message = "File uploaded successfully."
        //    });
        //}

        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                Console.WriteLine("API HIT");

                if (file == null)
                {
                    Console.WriteLine("FILE NULL");
                    return BadRequest("No file uploaded.");
                }

                Console.WriteLine($"FILE NAME: {file.FileName}");
                Console.WriteLine($"FILE SIZE: {file.Length}");

                var url = await _fileService.SaveFileAsync(file, "uploads");

                Console.WriteLine("UPLOAD SUCCESS");

                return Ok(new
                {
                    url
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, ex.Message);
            }
        }
    }
}
