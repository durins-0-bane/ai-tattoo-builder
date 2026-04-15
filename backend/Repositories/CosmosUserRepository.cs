using Microsoft.Azure.Cosmos;
using TattooUser = TattooShop.Api.Models.User;

namespace TattooShop.Api.Repositories;

public class CosmosUserRepository : IUserRepository
{
    private readonly Container _container;

    public CosmosUserRepository(CosmosClient client, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        _container = client.GetContainer(databaseName, "Users");
    }

    public async Task<TattooUser?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<TattooUser>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<TattooUser?> GetByGoogleSubjectAsync(string googleSubject)
    {
        var query = _container.GetItemQueryIterator<TattooUser>(
            new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @subject")
                .WithParameter("@subject", googleSubject));

        var results = new List<TattooUser>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results.Count > 0 ? results[0] : null;
    }

    public async Task UpsertAsync(TattooUser user)
    {
        await _container.UpsertItemAsync(user, new PartitionKey(user.GoogleSubject));
    }
}
