using Newtonsoft.Json;

namespace TattooShop.Api.Models;

public record ArtistProfile(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("partitionKey")] string Scope,
    string Slug,
    string DisplayName,
    string Bio,
    string PersonaPrompt,
    string AccentColor,
    string[] Specialties,
    string[] PortfolioImageUrls,
    bool IsActive
);
