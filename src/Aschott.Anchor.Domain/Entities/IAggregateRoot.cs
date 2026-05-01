using Aschott.Anchor.Domain.Events;

namespace Aschott.Anchor.Domain.Entities;

/// <summary>
/// Non-generic marker for aggregate roots, allowing infrastructure (DbContext, UoW)
/// to enumerate tracked aggregates without pinning their TKey at the call site.
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyList<DomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}
