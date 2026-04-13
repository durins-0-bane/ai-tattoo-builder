using MediatR;
using Microsoft.AspNetCore.Mvc;
using TattooShop.Api.Features;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;

namespace TattooShop.Api.Controllers;

[ApiController]
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
    public async Task<ActionResult<TattooDesign>> Create(TattooDesign design)
    {
        var result = await _mediator.Send(new SaveTattooDesignCommand(design));
        return CreatedAtAction(nameof(GetById), new { id = result.Id, customerId = result.CustomerId }, result);
    }

    /// <summary>
    /// Retrieves a tattoo design by its ID and the customer's ID.
    /// </summary>
    /// <param name="id">The ID of the tattoo design.</param>
    /// <param name="customerId">The ID of the customer who owns the tattoo design.</param>
    [HttpGet("{id}/{customerId}")]
    public async Task<ActionResult<TattooDesign>> GetById(string id, string customerId)
    {
        var design = await _repository.GetDesignByIdAsync(id, customerId);
        
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
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<TattooDesign>>> GetByCustomer(string customerId)
    {
        var designs = await _repository.GetDesignsByCustomerAsync(customerId);
        return Ok(designs);
    }
}