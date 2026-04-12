using Newtonsoft.Json;

namespace TattooShop.Api.Models;

public record TattooDesign(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("partitionKey")] string CustomerId,
    string ImageUrl,
    string Prompt,
    string RefinedPrompt,
    string Style,
    DateTime CreatedAt,
    bool IsFavorite
);