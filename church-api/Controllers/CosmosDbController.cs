using church_api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace church_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CosmosDbController : ControllerBase
    {
        private readonly ICosmosDbClient<PrayerCard> _cosmosDbClient;

        public CosmosDbController(ICosmosDbClient<PrayerCard> cosmosDbClient)
        {
            _cosmosDbClient = cosmosDbClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(string id)
        {
            var item = await _cosmosDbClient.GetItemAsync(id, "PrayerCard");

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] PrayerCard entity)
        {
            await _cosmosDbClient.AddItemAsync(entity);
            return CreatedAtAction(nameof(GetItem), new { id = entity.Id }, entity);
        }
    }
}