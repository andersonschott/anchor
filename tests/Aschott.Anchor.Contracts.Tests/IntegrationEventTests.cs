using Aschott.Anchor.Contracts;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Contracts.Tests;

public sealed class IntegrationEventTests
{
    private sealed record OrderShipped(Guid OrderId) : IntegrationEvent;

    [Fact]
    public void Records_with_same_state_are_equal()
    {
        var id = Guid.NewGuid();
        var time = DateTime.UtcNow;
        var tenant = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var a = new OrderShipped(orderId) { Id = id, OccurredAtUtc = time, TenantId = tenant };
        var b = new OrderShipped(orderId) { Id = id, OccurredAtUtc = time, TenantId = tenant };

        a.ShouldBe(b);
    }

    [Fact]
    public void Default_values_populate_id_and_timestamp()
    {
        var evt = new OrderShipped(Guid.NewGuid());

        evt.Id.ShouldNotBe(Guid.Empty);
        evt.OccurredAtUtc.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void TenantId_is_optional_for_host_level_events()
    {
        var evt = new OrderShipped(Guid.NewGuid());

        evt.TenantId.ShouldBeNull();
    }
}
