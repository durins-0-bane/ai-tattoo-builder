using Newtonsoft.Json;

namespace TattooShop.Api.Models;

public record ChatMessage(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("partitionKey")] string SessionId,
    string UserId,
    string Role,
    string Content,
    string? ImageUrl,
    DateTime CreatedAt
);
