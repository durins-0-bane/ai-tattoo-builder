using Microsoft.Azure.Cosmos;
using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public class CosmosTattooDesignRepository : ITattooDesignRepository
{
    private readonly Container _container;

    public CosmosTattooDesignRepository(CosmosClient client, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        _container = client.GetContainer(databaseName, "Designs");
    }

    public async Task AddDesignAsync(TattooDesign design) => 
        await _container.CreateItemAsync(design, new PartitionKey(design.CustomerId));

    public async Task<IEnumerable<TattooDesign>> GetDesignsByCustomerAsync(string customerId)
    {
        var query = _container.GetItemQueryIterator<TattooDesign>(
            new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @customerId")
                .WithParameter("@customerId", customerId));

        var results = new List<TattooDesign>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }
}