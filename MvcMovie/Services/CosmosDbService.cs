using Microsoft.Azure.Cosmos;
using MvcMovie.Models;
using Newtonsoft.Json;

namespace MvcMovie.Services
{
    public class CosmosDbService//<T> where T : class
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly string _partitionKey = "";

        public CosmosDbService(CosmosClient client, IConfiguration config)
        {
            string dbName = config["CosmosDb:DatabaseName"];
            string container = config["CosmosDb:ContainerName"];

            _cosmosClient = client;
            _container = _cosmosClient.GetContainer(dbName, container);
            _partitionKey = config["CosmosDb:PartitionKey"];
        }

        public async Task<T> GetItemAsync<T>(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(id, new PartitionKey(_partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        public async Task<ItemResponse<Movie>> CreateItemAsync(Movie item)
        {
            try
            {

                item.id = Guid.NewGuid().ToString();
                item.partition1 = _partitionKey;
                var response = await _container.CreateItemAsync(item, new PartitionKey(item.partition1));
                return response;

            }catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw;
            }

        }

        public async Task<ItemResponse<Movie>> UpdateItemAsync(Movie item)
        {
            try
            {
                item.partition1 = _partitionKey;
                var response = await _container.ReplaceItemAsync(item, item.id, new PartitionKey(item.partition1));
                return response;
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw;
            }

        }




        /*test purpose*/
        public async Task<List<T>> GetAllItemsAsync<T>()
        {
            try
            {
                var query = _container.GetItemQueryIterator<T>("SELECT * from c");
                var results = new List<T>();
                while(query.HasMoreResults)
                {
                    try
                    {
                        var response = await query.ReadNextAsync();
                        results.AddRange(response.Resource);   
                    }
                    catch(JsonReaderException ex)
                    {
                        continue;
                    }
                }
                return results;    
            }
            catch(CosmosException ex)
            {
                return default;
            }
            
        }
    }    
}
