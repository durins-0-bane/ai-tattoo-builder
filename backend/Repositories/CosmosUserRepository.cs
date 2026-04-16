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
        var query = _container.GetItemQueryIterator<TattooUser>(
            new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                .WithParameter("@id", id));

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            if (response.Count > 0)
                return response.First();
        }

        return null;
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
