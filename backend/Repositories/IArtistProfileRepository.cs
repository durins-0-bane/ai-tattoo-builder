using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public interface IArtistProfileRepository
{
    Task EnsureSeedDataAsync();
    Task<IReadOnlyList<ArtistProfile>> GetActiveArtistsAsync();
    Task<ArtistProfile?> GetByIdAsync(string id);
}
