using Aschott.Anchor.Domain.Events;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Domain.Tests.Events;

public sealed class DomainEventTests
{
    private sealed record SampleEvent(string Name) : DomainEvent;

    [Fact]
    public void Domain_event_has_unique_id_per_instance()
    {
        var a = new SampleEvent("a");
        var b = new SampleEvent("a");

        a.Id.ShouldNotBe(b.Id);
    }

    [Fact]
    public void Domain_event_records_occurred_at_in_utc()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var evt = new SampleEvent("x");
        var after = DateTime.UtcNow.AddSeconds(1);

        evt.OccurredAtUtc.ShouldBeInRange(before, after);
        evt.OccurredAtUtc.Kind.ShouldBe(DateTimeKind.Utc);
    }
}
