using Microsoft.Azure.Cosmos;
using church_api.Services.Abstractions;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using System.Threading;
using church_api.Controllers;

namespace church_api.Services
{

    public class CosmosDbClient<T> : ICosmosDbClient<T>
    {
        private readonly string _containerName;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CosmosDbClient(CosmosClient cosmosClient, IOptions<CosmosDbSettings> cosmosDbSettings)
        {
            _cosmosClient = cosmosClient;

            // Retrieve database and container names from settings
            var databaseName = cosmosDbSettings.Value.DatabaseName;
            var containerName = cosmosDbSettings.Value.ContainerName;

            // Create the database if it doesn't exist
            var databaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
            var database = databaseResponse.Database;

            // Create the container if it doesn't exist
            var containerResponse = database.CreateContainerIfNotExistsAsync(
                new ContainerProperties
                {
                    Id = containerName,
                    PartitionKeyPath = "/dataType", //cosmosDbSettings.Value.PartitionKeyPath // e.g., "/partitionKey"
                }).GetAwaiter().GetResult();

            _containerName = containerName;
            _container = containerResponse.Container;
        }

        public async Task<T?> GetItemAsync(string id, string partitionKey)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        //public async Task<IEnumerable<T>> QueryItemsAsync(string query, string partitionKey)
        //{
        //    var queryDefinition = new QueryDefinition(query);
        //    var queryRequestOptions = new QueryRequestOptions
        //    {
        //        PartitionKey = new PartitionKey(partitionKey)
        //    };

        //    var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: queryRequestOptions);

        //    var results = new List<T>();
        //    while (queryIterator.HasMoreResults)
        //    {
        //        var response = await queryIterator.ReadNextAsync();
        //        results.AddRange(response);
        //    }

        //    return results;
        //}

        public async Task AddItemAsync<T>(T item) where T : DocumentBase
        {
            await _container.CreateItemAsync(item, new PartitionKey(item.DataType));
            //var response = await _container.UpsertItemAsync(item, new PartitionKey(document.DataType), null, cancellationToken);
        }

        //public async Task UpdateItemAsync(string id, T item, string partitionKey)
        //{
        //    await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));
        //}

        //public async Task DeleteItemAsync(string id, string partitionKey)
        //{
        //    await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        //}
    }
}
