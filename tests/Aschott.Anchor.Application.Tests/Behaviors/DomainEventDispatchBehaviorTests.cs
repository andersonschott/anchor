using Aschott.Anchor.Application.Behaviors;
using Aschott.Anchor.Application.UnitOfWork;
using Aschott.Anchor.Domain.Entities;
using Aschott.Anchor.Domain.Events;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Application.Tests.Behaviors;

public sealed record DispatchPing(string Msg) : IRequest<string>;

public sealed record CustomerCreated(Guid CustomerId) : DomainEvent;
public sealed record CustomerRenamed(Guid CustomerId, string NewName) : DomainEvent;

public sealed class TestAggregate(Guid id) : AggregateRoot<Guid>(id)
{
    public void Raise(DomainEvent @event) => RaiseDomainEvent(@event);
}

public sealed class DomainEventDispatchBehaviorTests
{
    [Fact]
    public async Task Dispatches_collected_events_in_order_after_next()
    {
        var agg1 = new TestAggregate(Guid.NewGuid());
        agg1.Raise(new CustomerCreated(agg1.Id));
        var agg2 = new TestAggregate(Guid.NewGuid());
        agg2.Raise(new CustomerRenamed(agg2.Id, "Acme"));

        var collector = Substitute.For<IDomainEventCollector>();
        collector.GetTrackedAggregatesWithEvents().Returns([agg1, agg2]);

        IEnumerable<DomainEvent>? dispatched = null;
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        dispatcher
            .When(d => d.DispatchAsync(Arg.Any<IEnumerable<DomainEvent>>(), Arg.Any<CancellationToken>()))
            .Do(call => dispatched = ((IEnumerable<DomainEvent>)call.Args()[0]).ToList());

        var behavior = new DomainEventDispatchBehavior<DispatchPing, string>(collector, dispatcher);
        MessageHandlerDelegate<DispatchPing, string> next = (_, _) => ValueTask.FromResult("ok");

        await behavior.Handle(new DispatchPing("x"), next, CancellationToken.None);

        dispatched.ShouldNotBeNull();
        dispatched!.Count().ShouldBe(2);
        dispatched.OfType<CustomerCreated>().ShouldHaveSingleItem();
        dispatched.OfType<CustomerRenamed>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Clears_aggregates_after_dispatch()
    {
        var agg = new TestAggregate(Guid.NewGuid());
        agg.Raise(new CustomerCreated(agg.Id));
        var collector = Substitute.For<IDomainEventCollector>();
        collector.GetTrackedAggregatesWithEvents().Returns([agg]);
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        var behavior = new DomainEventDispatchBehavior<DispatchPing, string>(collector, dispatcher);
        MessageHandlerDelegate<DispatchPing, string> next = (_, _) => ValueTask.FromResult("ok");

        await behavior.Handle(new DispatchPing("x"), next, CancellationToken.None);

        agg.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_not_call_dispatcher_when_no_aggregates_have_events()
    {
        var collector = Substitute.For<IDomainEventCollector>();
        collector.GetTrackedAggregatesWithEvents().Returns([]);
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        var behavior = new DomainEventDispatchBehavior<DispatchPing, string>(collector, dispatcher);
        MessageHandlerDelegate<DispatchPing, string> next = (_, _) => ValueTask.FromResult("ok");

        await behavior.Handle(new DispatchPing("x"), next, CancellationToken.None);

        await dispatcher.DidNotReceive().DispatchAsync(
            Arg.Any<IEnumerable<DomainEvent>>(),
            Arg.Any<CancellationToken>());
    }
}
