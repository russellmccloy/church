using church_api.Models;
using church_api.Repositories.Abstractions;
using church_api.Services;
using church_api.Services.Abstractions;
using Microsoft.Azure.Cosmos;

namespace church_api.Repositories;

public class PrayerCardRepository : IPrayerCardRepository
{
    private readonly ICosmosDbClient<PrayerCard> _cosmosDbClient;

    public PrayerCardRepository(ICosmosDbClient<PrayerCard> cosmosDbClient)
    {
        _cosmosDbClient = cosmosDbClient;
    }

    public async Task<IList<PrayerCard>> GetPrayCardsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sql = "SELECT * FROM c where c.dataType = 'PrayerCard' and c.userId = @userId";

        var query = new QueryDefinition(sql)
        .WithParameter("@userId", userId.ToString());

        var prayerCards = await _cosmosDbClient.ExecuteQueryAsync<PrayerCard>(query, nameof(PrayerCard), cancellationToken);
        return prayerCards;
    }

    // Add a new PrayerCard to the database
    public async Task<PrayerCard?> AddPrayerCardAsync(PrayerCard prayerCard, CancellationToken cancellationToken)
    {
        // Set the ID and dataType for the new prayer card (if required)
        prayerCard.Id = Guid.NewGuid().ToString(); // Assign a new GUID as ID

        // Use CosmosDbClient to create the new prayer card in the database
        var createdPrayerCard = await _cosmosDbClient.CreateItemAsync(prayerCard, cancellationToken);

        return createdPrayerCard; // Return the created prayer card with its new ID
    }
}
