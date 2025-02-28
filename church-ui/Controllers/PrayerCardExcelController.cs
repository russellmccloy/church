using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Controllers;

[Authorize(Roles = "UsersUI,AdminsUI")]
public class PrayerCardExcelController : Controller
{
    private readonly string _connectionString;
    private readonly string _storageAccountName;
    private readonly string _containerName;
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly ILogger<PrayerCardExcelController> _logger;
    private readonly TelemetryClient _telemetryClient;
    
    public PrayerCardExcelController(IConfiguration configuration, 
        IGoogleSheetsService googleSheetsService, 
        ILogger<PrayerCardExcelController> logger, TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
        _googleSheetsService = googleSheetsService;
        _connectionString = configuration["ChurchStorage:ConnectionString"];
        _storageAccountName = configuration["ChurchStorage:AccountName"];
        _containerName = configuration["ChurchStorage:ContainerName"];
        _logger = logger;
    }
    
    public async Task<IActionResult> Index()
    {       
        var userName = User.Identity?.Name ?? "UnknownUser";

        // Create a custom event with custom dimensions
        var telemetry = new TraceTelemetry("User Accessed the Page")
        {
            SeverityLevel = SeverityLevel.Information
        };
        telemetry.Properties["WhoLoggedOn"] = userName;

        // Track the event
        _telemetryClient.TrackTrace(telemetry);
        
        var roleClaims = User.Claims
            .Where(c => c.Type == "roles" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .ToList();

        var logMessage = $"Logged-in User: {userName}, Roles: {string.Join(", ", roleClaims)}";
    
        _telemetryClient.TrackTrace(logMessage);
        _telemetryClient.Flush();
        
        List<Adult> adults = await _googleSheetsService.ReadDataAsync();

        // Filter out invalid entries (adult names and youth age != 0)
        var filteredAdults = adults.Where(a =>
            !string.IsNullOrWhiteSpace(a.AdultName) && a.AdultName != "0" &&
            a.YouthList.Any(y => y.Age != 0)).ToList(); // Ensure Youth.Age is not 0

        return View(filteredAdults); // Pass the list of adults
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var userName = User.Identity?.Name;

        var logMessage = $"Logged-in User: {userName} viewed their Prayer Cards";
        _telemetryClient.TrackTrace(logMessage);
        _telemetryClient.Flush();
        
        _logger.LogInformation(("Using the connection string to create the BlobServiceClient."));
        var blobServiceClient = new BlobServiceClient(_connectionString);
        
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
