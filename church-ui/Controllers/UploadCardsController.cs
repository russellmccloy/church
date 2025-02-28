using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Controllers
{
    [Authorize(Roles = "AdminsUI")]
    public class UploadCardsController : Controller
    {
        private readonly string _connectionString;
        private readonly string _storageAccountName;
        private readonly string _containerName;

        public UploadCardsController(IConfiguration configuration)
        {
            _connectionString = configuration["ChurchStorage:ConnectionString"];
            _storageAccountName = configuration["ChurchStorage:AccountName"];
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
                BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{adultId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var blobClient = containerClient.GetBlobClient(fileName);
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
        
        [HttpGet]
        public async Task<IActionResult> ListImages()
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var images = new List<ImageViewModel>();

                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    string sasUrl = GetSasUri(blobClient, TimeSpan.FromHours(1)).ToString();

                    images.Add(new ImageViewModel { FileName = blobItem.Name, Url = sasUrl });
                }

                return View(images);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error listing images: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(string fileName)
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
                
                TempData["Success"] = "File deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting file: {ex.Message}";
            }
            return RedirectToAction("ListImages");
        }

        private Uri GetSasUri(BlobClient blobClient, TimeSpan expiryTime)
        {
            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("BlobClient is missing permissions to generate SAS.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiryTime)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobClient.GenerateSasUri(sasBuilder);
        }
    }
}