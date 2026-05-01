using Aschott.Anchor.Domain.Events;

namespace Aschott.Anchor.Application.UnitOfWork;

/// <summary>
/// Application-layer abstraction over the underlying messaging infrastructure used
/// to publish <see cref="DomainEvent"/>s after a successful unit of work.
/// Infrastructure provides the concrete adapter (typically over Mediator's IPublisher).
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
