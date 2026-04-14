namespace TattooShop.Api.Services;

public interface ITattooAgentService
{
    IAsyncEnumerable<string> ChatStreamAsync(string userMessage);
}