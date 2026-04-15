using MediatR;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;

namespace TattooShop.Api.Features;

public record BookAppointmentCommand(Appointment Appointment) : IRequest<Appointment>;

public class BookAppointmentHandler(IAppointmentRepository repository) : IRequestHandler<BookAppointmentCommand, Appointment>
{
    public async Task<Appointment> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        await repository.AddAppointmentAsync(request.Appointment);
        return request.Appointment;
    }
}
