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
public class AppointmentsController(IAppointmentRepository repository, IMediator mediator) : ControllerBase
{
    private readonly IAppointmentRepository _repository = repository;
    private readonly IMediator _mediator = mediator;

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetMine()
    {
        var appointments = await _repository.GetAppointmentsByCustomerAsync(GetUserId());
        return Ok(appointments);
    }

    /// <summary>
    /// Gets all appointments for a specific artist.
    /// </summary>
    /// <param name="artistId">The ID of the artist whose appointments are being requested.</param>
    [HttpGet("artist/{artistId}")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetByArtist(string artistId)
    {
        var appointments = await _repository.GetAppointmentsByArtistAsync(artistId);
        return Ok(appointments);
    }

    /// <summary>
    /// Gets a specific appointment by its ID and the artist's ID.
    /// </summary>
    /// <param name="id">The ID of the appointment.</param>
    /// <param name="artistId">The ID of the artist associated with the appointment.</param>
    [HttpGet("{id}/{artistId}")]
    public async Task<ActionResult<Appointment>> GetById(string id, string artistId)
    {
        var appointment = await _repository.GetAppointmentAsync(id, artistId);
        
        if (appointment == null)
        {
            return NotFound(new { message = $"Appointment {id} not found." });
        }

        return Ok(appointment);
    }

    /// <summary>
    /// Creates a new appointment.
    /// </summary>
    /// <param name="appointment">The appointment to create. Artist's ID is required.</param>
    [HttpPost]
    public async Task<ActionResult<Appointment>> Create(CreateAppointmentRequest request)
    {
        var appointment = new Appointment(
            Id: Guid.NewGuid().ToString(),
            ArtistId: request.ArtistId,
            CustomerUserId: GetUserId(),
            CustomerName: GetUserDisplayName(),
            CustomerEmail: GetUserEmail(),
            StartTime: request.StartTimeUtc.ToUniversalTime(),
            DurationMinutes: request.DurationMinutes,
            ServiceType: request.ServiceType,
            Status: "Pending");

        var result = await _mediator.Send(new BookAppointmentCommand(appointment));
        return Ok(result);
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User ID claim is missing.");

    private string GetUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email")
        ?? string.Empty;

    private string GetUserDisplayName() =>
        User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue("name")
        ?? "Tattoo Collector";
}

public record CreateAppointmentRequest(string ArtistId, DateTime StartTimeUtc, int DurationMinutes, string ServiceType);