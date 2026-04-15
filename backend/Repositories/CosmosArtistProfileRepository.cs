using Microsoft.Azure.Cosmos;
using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public class CosmosArtistProfileRepository : IArtistProfileRepository
{
    private const string SeedScope = "seeded-artists";
    private readonly Container _container;

    public CosmosArtistProfileRepository(CosmosClient client, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        _container = client.GetContainer(databaseName, "Artists");
    }

    public async Task EnsureSeedDataAsync()
    {
        foreach (var artist in GetSeedArtists())
        {
            try
            {
                await _container.CreateItemAsync(artist, new PartitionKey(artist.Scope));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
            }
        }
    }

    public async Task<IReadOnlyList<ArtistProfile>> GetActiveArtistsAsync()
    {
        var query = _container.GetItemQueryIterator<ArtistProfile>(
            new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @scope AND c.isActive = true")
                .WithParameter("@scope", SeedScope));

        var results = new List<ArtistProfile>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<ArtistProfile?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<ArtistProfile>(id, new PartitionKey(SeedScope));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private static IReadOnlyList<ArtistProfile> GetSeedArtists() =>
    [
        new ArtistProfile(
            Id: "artist-onyx",
            Scope: SeedScope,
            Slug: "onyx",
            DisplayName: "Onyx Vale",
            Bio: "A blackwork specialist who pushes dramatic contrast, negative space, and ceremonial geometry.",
            PersonaPrompt: "You are Onyx Vale, a precise blackwork tattoo artist. You speak with calm confidence, emphasize silhouette, contrast, and longevity on skin, and prefer bold compositions over overworked detail.",
            AccentColor: "#BFA36A",
            Specialties: ["Blackwork", "Geometric", "Mythic symbols"],
            PortfolioImageUrls: [
                "https://placehold.co/600x800/111111/BFA36A?text=Onyx+I",
                "https://placehold.co/600x800/1B1B1B/E8D6B0?text=Onyx+II"
            ],
            IsActive: true),
        new ArtistProfile(
            Id: "artist-sable",
            Scope: SeedScope,
            Slug: "sable",
            DisplayName: "Sable Mora",
            Bio: "A neo-traditional illustrator blending botanical subjects, jewel tones, and cinematic framing.",
            PersonaPrompt: "You are Sable Mora, a neo-traditional tattoo artist with a lush, illustrative voice. You love rich color, floral framing, and emotionally resonant symbolism.",
            AccentColor: "#C65D45",
            Specialties: ["Neo-Traditional", "Botanical", "Color storytelling"],
            PortfolioImageUrls: [
                "https://placehold.co/600x800/3A1F1B/F4C095?text=Sable+I",
                "https://placehold.co/600x800/5E2B22/F8E1C4?text=Sable+II"
            ],
            IsActive: true),
        new ArtistProfile(
            Id: "artist-lumen",
            Scope: SeedScope,
            Slug: "lumen",
            DisplayName: "Lumen Reyes",
            Bio: "A fine-line futurist focused on celestial motifs, delicate gradients, and elegant motion.",
            PersonaPrompt: "You are Lumen Reyes, a fine-line tattoo artist with an airy, design-forward sensibility. You prefer elegant linework, celestial imagery, and restrained minimalism.",
            AccentColor: "#6EA8D9",
            Specialties: ["Fine Line", "Celestial", "Minimal surrealism"],
            PortfolioImageUrls: [
                "https://placehold.co/600x800/14212B/8BC6F1?text=Lumen+I",
                "https://placehold.co/600x800/0F1720/D2ECFF?text=Lumen+II"
            ],
            IsActive: true)
    ];
}
