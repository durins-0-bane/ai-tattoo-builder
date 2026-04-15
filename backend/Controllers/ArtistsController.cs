using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;

namespace TattooShop.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ArtistsController(IArtistProfileRepository artistProfileRepository) : ControllerBase
{
    private readonly IArtistProfileRepository _artistProfileRepository = artistProfileRepository;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ArtistProfile>>> GetAll()
    {
        await _artistProfileRepository.EnsureSeedDataAsync();
        var artists = await _artistProfileRepository.GetActiveArtistsAsync();
        return Ok(artists);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArtistProfile>> GetById(string id)
    {
        await _artistProfileRepository.EnsureSeedDataAsync();
        var artist = await _artistProfileRepository.GetByIdAsync(id);
        if (artist is null)
        {
            return NotFound(new { message = "Artist not found." });
        }

        return Ok(artist);
    }
}
