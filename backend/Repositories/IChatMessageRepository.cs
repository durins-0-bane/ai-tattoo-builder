using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public interface IChatMessageRepository
{
    Task AddAsync(ChatMessage message);
    Task<IReadOnlyList<ChatMessage>> GetBySessionAsync(string sessionId);
}
