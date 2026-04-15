using Newtonsoft.Json;

namespace TattooShop.Api.Models;

public record ChatSession(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("partitionKey")] string UserId,
    string ArtistId,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
