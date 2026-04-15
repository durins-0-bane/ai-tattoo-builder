using MediatR;

namespace TattooShop.Api.Features.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is BookAppointmentCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.Appointment.CustomerUserId))
                throw new ArgumentException("Customer user ID is required.");
            if (string.IsNullOrWhiteSpace(cmd.Appointment.CustomerEmail))
                throw new ArgumentException("Customer email is required.");
            if (cmd.Appointment.DurationMinutes <= 0)
                throw new ArgumentException("Duration must be greater than zero.");
            if (cmd.Appointment.StartTime <= DateTime.UtcNow)
                throw new ArgumentException("Appointment start time must be in the future.");
        }

        if (request is SaveTattooDesignCommand designCmd)
        {
            if (string.IsNullOrWhiteSpace(designCmd.Design.CustomerId))
                throw new ArgumentException("Customer ID is required.");
            if (string.IsNullOrWhiteSpace(designCmd.Design.ImageUrl))
                throw new ArgumentException("Image URL is required.");
            if (string.IsNullOrWhiteSpace(designCmd.Design.Prompt))
                throw new ArgumentException("Prompt is required.");
            if (string.IsNullOrWhiteSpace(designCmd.Design.Style))
                throw new ArgumentException("Style is required.");
        }

        return await next();
    }
}
