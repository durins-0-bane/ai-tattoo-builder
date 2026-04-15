using MediatR;
using Microsoft.Extensions.Logging;

namespace TattooShop.Api.Features.Behaviors;

public class RetryBehavior<TRequest, TResponse>(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private const int MaxRetries = 3;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await next();
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                logger.LogWarning(ex, "Attempt {Attempt} failed for {RequestType}. Retrying...", attempt, typeof(TRequest).Name);
                await Task.Delay(200 * attempt, cancellationToken);
            }
        }

        return await next(); // final attempt — let exceptions propagate
    }
}
