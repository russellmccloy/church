using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using System.Text.Json.Serialization;
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
    private readonly string _geminiApiUrl;
    private readonly string _geminiApiKey;

    public PrayerCardExcelController(IConfiguration configuration,
        IGoogleSheetsService googleSheetsService,
        ILogger<PrayerCardExcelController> logger, TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
        _googleSheetsService = googleSheetsService;
        _connectionString = configuration["ChurchStorage:ConnectionString"] ?? throw new InvalidOperationException();
        _storageAccountName = configuration["ChurchStorage:AccountName"] ?? throw new InvalidOperationException();
        _containerName = configuration["ChurchStorage:ContainerName"] ?? throw new InvalidOperationException();
        _geminiApiUrl = configuration["Gemini:ApiUrl"] ?? throw new InvalidOperationException();
        _geminiApiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException();
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

    public async Task<IActionResult> AnalyzeImage(string imageUrl, string imageName)
    {
        try
        {
            // 1. Download the image from the SAS URL
            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            var base64Image = Convert.ToBase64String(imageBytes);

            // 2. Call the Google Gemini API
            var analysisResult = await AnalyzeImageWithGemini(base64Image, imageName);

            return Content(analysisResult); // Return the analysis result as plain text
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return Content("Error analyzing the image.");
        }
    }

    private async Task<string> AnalyzeImageWithGemini(string base64Image, string imageName)
    {
        // Replace with your Google Gemini API key
        var apiUrl =  _geminiApiUrl + "?key=" +_geminiApiKey;

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var prompt = $@"
        Analyze the image named '{imageName}' and provide the following information in JSON format:

        {{
            ""sentiment"": ""How the person who wrote the card is feeling."",
            ""prayerSuggestion"": ""What you, the LLM, would suggest prayer for. Only in the context ofJesus Christ and the NIV bible."",
            ""reasoning"": ""How you, the LLM, came up with your results.""
        }}

        Provide the response strictly in JSON format without any additional text.
    ";

        var requestBody = new GeminiRequest
        {
            contents = new[]
            {
                new GeminiContent
                {
                    parts = new[]
                    {
                        new GeminiPart
                        {
                            inlineData = new GeminiInlineData
                            {
                                mimeType = "image/jpeg",
                                data = base64Image
                            }
                        },
                        new GeminiPart
                        {
                            text = prompt
                        }
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(apiUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        try
        {
            // Parse the JSON response
            dynamic responseObject = JsonConvert.DeserializeObject(responseContent) ?? throw new InvalidOperationException();
            string jsonResult = responseObject?.candidates?[0]?.content?.parts?[0]?.text ?? throw new InvalidOperationException();

            if (string.IsNullOrEmpty(jsonResult))
            {
                return
                    "{\"error\": \"Could not analyze the image or the response was not in the correct JSON format.\"}";
            }

            // Trim and clean the string
            string jsonString = jsonResult.Replace("```json", "").Replace("```", "").Replace("\n", "").Trim();

            var analysisResult = JsonConvert.DeserializeObject<SentimentAnalysisResult>(jsonString);

            // Return the parsed JSON as a string
            return JsonConvert.SerializeObject(analysisResult);
        }
        catch (JsonReaderException ex)
        {
            _logger.LogError(ex, "Error parsing JSON response from Gemini API");
            return
                "{\"error\": \"Error parsing JSON response from Gemini API. The API may not have returned the response in the expected JSON format.\"}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return "{\"error\": \"Error analyzing the image.\"}";
        }
    }
    

}
