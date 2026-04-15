using TattooShop.Api.Models;

namespace TattooShop.Api.Services;

public interface ITattooAgentService
{
    IAsyncEnumerable<ChatResponseChunk> ChatStreamAsync(string userMessage);
}