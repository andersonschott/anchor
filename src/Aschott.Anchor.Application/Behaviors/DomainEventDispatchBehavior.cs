using Aschott.Anchor.Application.UnitOfWork;
using Aschott.Anchor.Domain.Events;
using Mediator;

namespace Aschott.Anchor.Application.Behaviors;

/// <summary>
/// After <c>next()</c> returns, collects domain events from all aggregates tracked by the
/// current unit of work and forwards them to <see cref="IDomainEventDispatcher"/>.
/// Aggregates are cleared after their events are captured to prevent re-publishing on
/// subsequent SaveChanges in the same scope.
/// </summary>
public sealed class DomainEventDispatchBehavior<TRequest, TResponse>(
    IDomainEventCollector collector,
    IDomainEventDispatcher dispatcher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);

        var aggregates = collector.GetTrackedAggregatesWithEvents();
        if (aggregates.Count == 0)
            return response;

        var events = new List<DomainEvent>();
        foreach (var aggregate in aggregates)
        {
            events.AddRange(aggregate.GetDomainEvents());
            aggregate.ClearDomainEvents();
        }

        if (events.Count > 0)
            await dispatcher.DispatchAsync(events, cancellationToken);

        return response;
    }
}
