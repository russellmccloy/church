using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Controllers
{
    [Authorize(Policy = "AdminsOnly")] 
    public class UploadCardsController : Controller
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public UploadCardsController(IConfiguration configuration)
        {
            // Retrieve the connection string and container name from appsettings
            _connectionString = configuration["ChurchStorage:ConnectionString"];
            _containerName = configuration["ChurchStorage:ContainerName"];
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImages(int adultId, IFormFile[] files)
        {
            if (files == null || files.Length == 0)
            {
                TempData["Error"] = "No files selected!";
                return RedirectToAction("Index");
            }

            try
            {
                // Create a BlobServiceClient to interact with Azure Blob Storage
                BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                // Ensure the container exists
                await containerClient.CreateIfNotExistsAsync();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{adultId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"; // Create unique file name
                        var blobClient = containerClient.GetBlobClient(fileName);

                        // Upload the file to Azure Blob Storage
                        using (var stream = file.OpenReadStream())
                        {
                            await blobClient.UploadAsync(stream, overwrite: true);
                        }
                    }
                }

                TempData["Success"] = "Images uploaded successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error uploading images: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
