using attempt1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using System.Security.Claims;

namespace attempt1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IDownstreamApi downstreamApi)
        {
            _logger = logger;
            _downstreamApi = downstreamApi; ;
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Index()
        { 
            var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            _logger.LogInformation($"User ID: {userId}");

            // Construct the endpoint URL by appending the path segment
            string relativePath = $"PrayerCard/{userId}";

            // using var response = await _downstreamApi.CallApiForUserAsync("DownstreamApi").ConfigureAwait(false);

            using var response = await _downstreamApi.CallApiForUserAsync("DownstreamApi", options =>
            {
                options.RelativePath = relativePath; // Append the relative path to the base URL
            }).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var apiResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                ViewData["ApiResult"] = apiResult;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}: {error}");
            };
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
