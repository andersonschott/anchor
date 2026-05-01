using Aschott.Anchor.Domain.Events;

namespace Aschott.Anchor.Domain.Entities;

public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected AggregateRoot() { }
    protected AggregateRoot(TKey id) : base(id) { }

    public IReadOnlyList<DomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(DomainEvent @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
