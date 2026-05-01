using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Aschott.Anchor.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ActivitySource Activity = new("Aschott.Anchor.Application");

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        using var activity = Activity.StartActivity($"Mediator: {name}");
        activity?.SetTag("command.name", name);

        var sw = Stopwatch.StartNew();
        logger.LogInformation("Handling {Name}", name);
        try
        {
            var response = await next(message, cancellationToken);
            sw.Stop();
            logger.LogInformation("Handled {Name} in {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Handling {Name} failed", name);
            throw;
        }
    }
}
