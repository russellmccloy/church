using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace church_ui.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ITokenAcquisition tokenAcquisition, IHttpClientFactory httpClientFactory)
        {
            _tokenAcquisition = tokenAcquisition;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var apiUrl = "https://your-api-url/api/endpoint"; // Replace with your API endpoint URL

            // Get the token to call the API
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "api-scope" }); // Replace "api-scope" with the correct scope for your API

            // Call the Web API
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                // Process the response
                var data = await response.Content.ReadAsStringAsync();
                // You can return the data to the view or use it elsewhere
            }
            else
            {
                // Handle error
            }

            return View();
        }
    }
}
