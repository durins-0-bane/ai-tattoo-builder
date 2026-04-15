using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TattooShop.Api.Features;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;

namespace TattooShop.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TattooDesignsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITattooDesignRepository _repository;

    public TattooDesignsController(IMediator mediator, ITattooDesignRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    /// <summary>
    /// Creates a new tattoo design.
    /// </summary>
    /// <param name="design">The tattoo design to create.</param>
    [HttpPost]
    public async Task<ActionResult<TattooDesign>> Create(CreateTattooDesignRequest request)
    {
        var design = new TattooDesign(
            Id: Guid.NewGuid().ToString(),
            CustomerId: GetUserId(),
            ImageUrl: request.ImageUrl,
            Prompt: request.Prompt,
            RefinedPrompt: request.RefinedPrompt,
            Style: request.Style,
            ArtistId: request.ArtistId,
            SessionId: request.SessionId,
            CreatedAt: DateTime.UtcNow,
            IsFavorite: false);

        var result = await _mediator.Send(new SaveTattooDesignCommand(design));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a tattoo design by its ID and the customer's ID.
    /// </summary>
    /// <param name="id">The ID of the tattoo design.</param>
    /// <param name="customerId">The ID of the customer who owns the tattoo design.</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<TattooDesign>> GetById(string id)
    {
        var design = await _repository.GetDesignByIdAsync(id, GetUserId());
        
        if (design == null)
        {
            return NotFound(new { message = "Design not found." });
        }

        return Ok(design);
    }

    /// <summary>
    /// Retrieves all tattoo designs for a specific customer.
    /// </summary>
    /// <param name="customerId">The ID of the customer.</param>
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<TattooDesign>>> GetMine()
    {
        var designs = await _repository.GetDesignsByCustomerAsync(GetUserId());
        return Ok(designs);
    }

    [HttpPatch("{id}/favorite")]
    public async Task<ActionResult<TattooDesign>> ToggleFavorite(string id)
    {
        var customerId = GetUserId();
        var design = await _repository.GetDesignByIdAsync(id, customerId);
        if (design is null)
        {
            return NotFound(new { message = "Design not found." });
        }

        var updated = design with { IsFavorite = !design.IsFavorite };
        await _repository.UpdateDesignAsync(updated);
        return Ok(updated);
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User ID claim is missing.");
}

public record CreateTattooDesignRequest(
    string ImageUrl,
    string Prompt,
    string RefinedPrompt,
    string Style,
    string? ArtistId,
    string? SessionId);