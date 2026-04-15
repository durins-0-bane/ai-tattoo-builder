using Newtonsoft.Json;

namespace TattooShop.Api.Models;

public record User(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("partitionKey")] string GoogleSubject,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    DateTime CreatedAt,
    DateTime LastLoginAt
);
