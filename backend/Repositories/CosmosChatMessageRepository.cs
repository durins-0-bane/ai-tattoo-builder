using Microsoft.Azure.Cosmos;
using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public class CosmosChatMessageRepository : IChatMessageRepository
{
    private readonly Container _container;

    public CosmosChatMessageRepository(CosmosClient client, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        _container = client.GetContainer(databaseName, "ChatMessages");
    }

    public async Task AddAsync(ChatMessage message)
    {
        await _container.CreateItemAsync(message, new PartitionKey(message.SessionId));
    }

    public async Task<IReadOnlyList<ChatMessage>> GetBySessionAsync(string sessionId)
    {
        var query = _container.GetItemQueryIterator<ChatMessage>(
            new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @sessionId ORDER BY c.createdAt ASC")
                .WithParameter("@sessionId", sessionId));

        var results = new List<ChatMessage>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }
}
