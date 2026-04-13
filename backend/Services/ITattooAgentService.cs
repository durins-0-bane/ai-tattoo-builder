namespace TattooShop.Api.Services;

public interface ITattooAgentService
{
    Task<string> ChatAsync(string userMessage);
}