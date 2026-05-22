//using GD1.Application.Interfaces;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;

//namespace GD1.Api.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UploadController : ControllerBase
//    {
//        private readonly IFileService _fileService;

//        public UploadController(IFileService fileService)
//        {
//            _fileService = fileService;
//        }

//        /// <summary>
//        /// Uploads a file (camera capture or selected file) and returns the URL.
//        /// </summary>
//        [HttpPost("upload-file")]
//        [Consumes("multipart/form-data")]
//        public async Task<IActionResult> Upload(IFormFile file)
//        {
//            if (file == null) return BadRequest("No file uploaded.");

//            var url = await _fileService.SaveFileAsync(file, "uploads");
            
//            return Ok(new { 
//                url,
//                message = "File uploaded successfully. Use this URL in the AddVehicle API."
//            });
//        }
//    }
//}
