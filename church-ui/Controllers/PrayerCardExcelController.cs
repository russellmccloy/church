using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Models;

namespace Controllers;

public class PrayerCardExcelController : Controller
{
    private readonly GoogleSheetsService _googleSheetsService;
    private readonly string _connectionString;
    private readonly string _containerName;
    
    public PrayerCardExcelController(IConfiguration configuration)
    {
        _googleSheetsService = new GoogleSheetsService();
        _connectionString = configuration["ChurchStorage:ConnectionString"];
        _containerName = configuration["ChurchStorage:ContainerName"];
    }
    
    public async Task<IActionResult> Index()
    {
        List<Adult> adults = await _googleSheetsService.ReadDataAsync();

        // Filter out invalid entries (adult names and youth age != 0)
        var filteredAdults = adults.Where(a =>
            !string.IsNullOrWhiteSpace(a.AdultName) && a.AdultName != "0" &&
            a.YouthList.Any(y => y.Age != 0)).ToList(); // Ensure Youth.Age is not 0

        return View(filteredAdults); // Pass the list of adults
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var credential = new DefaultAzureCredential();
        // Create BlobServiceClient to interact with Azure Blob Storage
        BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri("https://churchdevst01.blob.core.windows.net"), credential);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        // List all blobs for this AdultId (container/AdultId/ image.jpg)
        var images = new List<string>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: $"{id}/"))
        {
            images.Add(blobItem.Name); // Collect all image names for the specific AdultId
        }

        // Return the view and pass the list of images
        return View(images);
    }
}
