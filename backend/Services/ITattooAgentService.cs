using TattooShop.Api.Models;

namespace TattooShop.Api.Services;

public record ChatReplyResponse(string SessionId, string Text, string? ImageBase64);

public interface ITattooAgentService
{
    Task<ChatReplyResponse> ChatAsync(
        string userId,
        string userEmail,
        string userDisplayName,
        string artistId,
        string? sessionId,
        string userMessage);
}