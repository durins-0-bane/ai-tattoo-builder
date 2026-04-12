using Microsoft.AspNetCore.Mvc;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;

namespace TattooShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // potentially add auth here later, e.g. [Authorize]
public class AppointmentsController(IAppointmentRepository repository) : ControllerBase
{
    private readonly IAppointmentRepository _repository = repository;

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
    public async Task<ActionResult<Appointment>> Create(Appointment appointment)
    {
        // Possibly add date/time validation here
        await _repository.AddAppointmentAsync(appointment);
        return Ok(appointment);
    }
}