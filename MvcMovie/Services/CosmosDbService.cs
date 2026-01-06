using Microsoft.Azure.Cosmos;
using MvcMovie.Models;
using Newtonsoft.Json;
using MvcMovie.Static;

namespace MvcMovie.Services
{
    public class CosmosDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        //private readonly string _partitionKey = "";

        public CosmosDbService(CosmosClient client, IConfiguration config)
        {
            string dbName = config["CosmosDb:DatabaseName"];
            string container = config["CosmosDb:ContainerName"];

            _cosmosClient = client;
            _container = _cosmosClient.GetContainer(dbName, container);
            //_partitionKey = config["CosmosDb:PartitionKey"];
        }

        public async Task<T> GetItemAsync<T>(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(id, new PartitionKey(MvcMovie.Static.Static.PartitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw;
            }
        }

        public async Task<ItemResponse<Movie>> CreateItemAsync(Movie item)
        {
            try
            {
                var response = await _container.CreateItemAsync(item, new PartitionKey(MvcMovie.Static.Static.PartitionKey));
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
                throw;
            }
            
        }

        public async Task<string> DeleteItemAsync(string movieId)
        {            
            try
            {
                var item = new[] {movieId };
                var response = await _container.Scripts.ExecuteStoredProcedureAsync<string>(
                    "deleteMovieById",
                    new PartitionKey(_partitionKey),
                    item
                );
                string result =response.Resource;
                return result;
            }
            catch(CosmosException ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }    
}
