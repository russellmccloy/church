using church_api.Models;
using church_api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

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
        public async Task<IActionResult> GetItem(string id, CancellationToken cancellationToken)
        {
            var item = await _cosmosDbClient.GetItemAsync(id, "PrayerCard", cancellationToken);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] PrayerCard entity, CancellationToken cancellationToken)
        {
            await _cosmosDbClient.CreateItemAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(GetItem), new { id = entity.Id }, entity);
        }
    }
}