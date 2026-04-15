using Microsoft.Azure.Cosmos;
using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public class CosmosChatSessionRepository : IChatSessionRepository
{
    private readonly Container _container;

    public CosmosChatSessionRepository(CosmosClient client, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        _container = client.GetContainer(databaseName, "ChatSessions");
    }

    public async Task<ChatSession> CreateAsync(ChatSession session)
    {
        var response = await _container.CreateItemAsync(session, new PartitionKey(session.UserId));
        return response.Resource;
    }

    public async Task<ChatSession?> GetByIdAsync(string userId, string sessionId)
    {
        try
        {
            var response = await _container.ReadItemAsync<ChatSession>(sessionId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<ChatSession>> GetByUserAsync(string userId)
    {
        var query = _container.GetItemQueryIterator<ChatSession>(
            new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @userId ORDER BY c.updatedAt DESC")
                .WithParameter("@userId", userId));

        var results = new List<ChatSession>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task UpsertAsync(ChatSession session)
    {
        await _container.UpsertItemAsync(session, new PartitionKey(session.UserId));
    }
}
