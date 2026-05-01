using Aschott.Anchor.Infrastructure.MultiTenancy;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Infrastructure.Tests.MultiTenancy;

public sealed class CurrentTenantAccessorTests
{
    [Fact]
    public void Default_id_is_null()
    {
        var accessor = new CurrentTenantAccessor();

        accessor.Id.ShouldBeNull();
    }

    [Fact]
    public void Change_overrides_current_id_for_the_scope()
    {
        var accessor = new CurrentTenantAccessor();
        var tenantA = Guid.NewGuid();

        using (accessor.Change(tenantA))
        {
            accessor.Id.ShouldBe(tenantA);
        }

        accessor.Id.ShouldBeNull();
    }

    [Fact]
    public void Nested_changes_revert_in_lifo_order()
    {
        var accessor = new CurrentTenantAccessor();
        var outer = Guid.NewGuid();
        var inner = Guid.NewGuid();

        using (accessor.Change(outer))
        {
            accessor.Id.ShouldBe(outer);
            using (accessor.Change(inner))
            {
                accessor.Id.ShouldBe(inner);
            }
            accessor.Id.ShouldBe(outer);
        }

        accessor.Id.ShouldBeNull();
    }

    [Fact]
    public async Task Async_local_isolates_concurrent_flows()
    {
        var accessor = new CurrentTenantAccessor();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var taskA = Task.Run(async () =>
        {
            using (accessor.Change(tenantA))
            {
                await Task.Yield();
                return accessor.Id;
            }
        });

        var taskB = Task.Run(async () =>
        {
            using (accessor.Change(tenantB))
            {
                await Task.Yield();
                return accessor.Id;
            }
        });

        var results = await Task.WhenAll(taskA, taskB);

        results[0].ShouldBe(tenantA);
        results[1].ShouldBe(tenantB);
    }
}
