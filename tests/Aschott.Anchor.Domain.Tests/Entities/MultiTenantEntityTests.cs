using Aschott.Anchor.Domain.Entities;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Domain.Tests.Entities;

public sealed class MultiTenantEntityTests
{
    private sealed class Customer : MultiTenantEntity<Guid>
    {
        public Customer() { }
        public Customer(Guid id, Guid? tenantId) : base(id, tenantId) { }
    }

    [Fact]
    public void TenantId_is_null_by_default_for_host_level_data()
    {
        var c = new Customer(Guid.NewGuid(), null);

        c.TenantId.ShouldBeNull();
    }

    [Fact]
    public void TenantId_persists_when_set_via_constructor()
    {
        var tenantId = Guid.NewGuid();

        var c = new Customer(Guid.NewGuid(), tenantId);

        c.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public void Implements_IMultiTenant_marker()
    {
        var c = new Customer(Guid.NewGuid(), Guid.NewGuid());

        c.ShouldBeAssignableTo<IMultiTenant>();
    }

    [Fact]
    public void Inherits_AggregateRoot()
    {
        var c = new Customer(Guid.NewGuid(), Guid.NewGuid());

        c.ShouldBeAssignableTo<AggregateRoot<Guid>>();
    }
}
