using church_api.Models;
using Microsoft.Azure.Cosmos;

namespace church_api.Services.Abstractions
{
    public interface ICosmosDbClient<T>
    {
        Task<T?> GetItemAsync(string id, string partitionKey, CancellationToken cancellationToken);
        //Task<IEnumerable<T>> QueryItemsAsync(string query, string partitionKey);
        Task<T?> CreateItemAsync<T>(T item, CancellationToken cancellationToken) where T : DocumentBase;
        Task<List<T>> ExecuteQueryAsync<T>(QueryDefinition query, string partitionKey, CancellationToken cancellationToken);
        //Task UpdateItemAsync(string id, T item, string partitionKey);
        //Task DeleteItemAsync(string id, string partitionKey);
    }
}
