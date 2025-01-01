using church_api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace church_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IFileUploader _imageUploader;

        public StorageController(IFileUploader imageUploader)
        {
            _imageUploader = imageUploader;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] string description, [FromForm] DateTime clientDate, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var filePath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var blobName = file.FileName;

            await _imageUploader.UploadFileAsync(filePath, blobName);

            System.IO.File.Delete(filePath);

            return Ok(new { Message = "Image uploaded successfully.", BlobName = blobName });
        }
    }
}
