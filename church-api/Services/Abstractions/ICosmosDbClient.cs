using church_api.Controllers;

namespace church_api.Services.Abstractions
{
    public interface ICosmosDbClient<T>
    {
        Task<T?> GetItemAsync(string id, string partitionKey);
        //Task<IEnumerable<T>> QueryItemsAsync(string query, string partitionKey);
        Task AddItemAsync<T>(T item) where T : DocumentBase;
        //Task UpdateItemAsync(string id, T item, string partitionKey);
        //Task DeleteItemAsync(string id, string partitionKey);
    }
}
