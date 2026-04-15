using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public interface IChatSessionRepository
{
    Task<ChatSession> CreateAsync(ChatSession session);
    Task<ChatSession?> GetByIdAsync(string userId, string sessionId);
    Task<IReadOnlyList<ChatSession>> GetByUserAsync(string userId);
    Task UpsertAsync(ChatSession session);
}
