using System.ComponentModel;
using MediatR;
using Microsoft.SemanticKernel;
using TattooShop.Api.Features;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;
using TattooShop.Api.Services;

namespace TattooShop.Api.Plugins;

public class AppointmentPlugin(IMediator mediator, IAppointmentRepository repository, ChatExecutionContext executionContext)
{
    private readonly ChatExecutionContext _executionContext = executionContext;

    [KernelFunction("book_appointment")]
    [Description("Books a new appointment for the signed-in customer with a specific artist.")]
    public async Task<string> BookAppointmentAsync(
        string artistId,
        [Description("Appointment start time in UTC (e.g. 2026-05-01T14:00:00Z).")]
        string startTimeUtc,
        int appointmentDurationInMinutes,
        [Description("Service type, e.g. 'New Tattoo', 'Touch-up', 'Consultation'.")]
        string serviceType)
    {
        if (string.IsNullOrWhiteSpace(_executionContext.UserId))
            return "Error: You must be signed in before booking an appointment.";

        if (!DateTime.TryParse(startTimeUtc, out var startTime))
            return "Error: Invalid start time format. Use UTC.";

        var appointment = new Appointment(
            Id: Guid.NewGuid().ToString(),
            ArtistId: artistId,
            CustomerUserId: _executionContext.UserId,
            CustomerName: _executionContext.UserDisplayName,
            CustomerEmail: _executionContext.UserEmail,
            StartTime: startTime.ToUniversalTime(),
            DurationMinutes: appointmentDurationInMinutes,
            ServiceType: serviceType,
            Status: "Pending"
        );

        await mediator.Send(new BookAppointmentCommand(appointment));
        return $"Appointment {appointment.Id} booked for {_executionContext.UserDisplayName} on {startTime:f} UTC.";
    }

    [KernelFunction("get_artist_appointments")]
    [Description("Retrieves all appointments for a given artist.")]
    public async Task<string> GetAppointmentsByArtistAsync(string artistId)
    {
        var appointments = (await repository.GetAppointmentsByArtistAsync(artistId)).ToList();

        if (appointments.Count == 0)
            return $"No appointments found for artist {artistId}.";

        var summary = string.Join("\n", appointments.Select(a =>
            $"- [{a.Id}] {a.CustomerName} on {a.StartTime:f} UTC — {a.ServiceType} ({a.DurationMinutes} min) [{a.Status}]"));

        return $"Appointments for artist {artistId}:\n{summary}";
    }
}
