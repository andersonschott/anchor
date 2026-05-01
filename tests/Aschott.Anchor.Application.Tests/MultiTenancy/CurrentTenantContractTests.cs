using Aschott.Anchor.Application.MultiTenancy;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Application.Tests.MultiTenancy;

public sealed class CurrentTenantContractTests
{
    [Fact]
    public void Interface_exposes_nullable_id_and_change_method()
    {
        typeof(ICurrentTenant).GetProperty("Id")!.PropertyType.ShouldBe(typeof(Guid?));
        typeof(ICurrentTenant).GetMethod("Change").ShouldNotBeNull();
    }
}
