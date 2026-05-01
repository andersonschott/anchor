using Aschott.Anchor.Domain.Entities;
using Aschott.Anchor.Domain.Events;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Domain.Tests.Entities;

public sealed class AggregateRootTests
{
    private sealed record SampleEvent(string What) : DomainEvent;

    private sealed class SampleAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public void DoSomething(string what) => RaiseDomainEvent(new SampleEvent(what));
    }

    [Fact]
    public void Raise_adds_event_to_list()
    {
        var agg = new SampleAggregate(Guid.NewGuid());

        agg.DoSomething("first");

        var events = agg.GetDomainEvents();
        events.Count.ShouldBe(1);
        var sample = events[0].ShouldBeOfType<SampleEvent>();
        sample.What.ShouldBe("first");
    }

    [Fact]
    public void Multiple_raises_preserve_order()
    {
        var agg = new SampleAggregate(Guid.NewGuid());

        agg.DoSomething("a");
        agg.DoSomething("b");
        agg.DoSomething("c");

        var events = agg.GetDomainEvents();
        events.Count.ShouldBe(3);
        events.Select(e => ((SampleEvent)e).What).ShouldBe(["a", "b", "c"]);
    }

    [Fact]
    public void Clear_empties_event_list()
    {
        var agg = new SampleAggregate(Guid.NewGuid());
        agg.DoSomething("x");

        agg.ClearDomainEvents();

        agg.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact]
    public void GetDomainEvents_returns_read_only_collection()
    {
        var agg = new SampleAggregate(Guid.NewGuid());
        agg.DoSomething("x");

        var events = agg.GetDomainEvents();

        events.ShouldBeAssignableTo<IReadOnlyList<DomainEvent>>();
    }
}
