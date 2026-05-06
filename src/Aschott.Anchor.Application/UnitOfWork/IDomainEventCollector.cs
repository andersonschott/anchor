using Aschott.Anchor.Domain.Entities;

namespace Aschott.Anchor.Application.UnitOfWork;

/// <summary>
/// Returns aggregate roots tracked by the current unit of work that have unpublished
/// domain events. Implemented by Infrastructure over the active DbContext.
/// </summary>
public interface IDomainEventCollector
{
    IReadOnlyList<IAggregateRoot> GetTrackedAggregatesWithEvents();
}
