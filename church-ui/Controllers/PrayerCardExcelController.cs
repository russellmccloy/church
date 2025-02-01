using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Models;

namespace Controllers;

public class PrayerCardExcelController : Controller
{
    private readonly GoogleSheetsService _googleSheetsService;
    private readonly string _connectionString;
    private readonly string _storageAccountName;
    private readonly string _containerName;
    
    public PrayerCardExcelController(IConfiguration configuration)
    {
        _googleSheetsService = new GoogleSheetsService();
        _connectionString = configuration["ChurchStorage:ConnectionString"];
        _storageAccountName = configuration["ChurchStorage:AccountName"];
        _containerName = configuration["ChurchStorage:ContainerName"];
    }
    
    public async Task<IActionResult> Index()
    {
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }
        
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
        BlobServiceClient blobServiceClient;

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            blobServiceClient = new BlobServiceClient(_connectionString);
        }
        else
        {
            blobServiceClient = new BlobServiceClient(
                new Uri($"https://{_storageAccountName}.blob.core.windows.net"), 
                credential);
        }

        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        Console.WriteLine($"Container Name: {_containerName}");

        var images = new List<(string Name, string SasUrl)>(); // Tuple list to store both name & SAS URL

        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: $"{id}/"))
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var sasUri = GetSasUri(blobClient, TimeSpan.FromHours(1)); // Generate SAS URL

            images.Add((blobItem.Name, sasUri.ToString())); // Store name and SAS URL
        }

        return View(images);
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
            Resource = "b", // "b" means blob
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiryTime)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read); // Read-only access

        return blobClient.GenerateSasUri(sasBuilder);
    }
}   
