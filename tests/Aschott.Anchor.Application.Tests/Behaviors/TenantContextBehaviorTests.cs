using Aschott.Anchor.Application.Behaviors;
using Aschott.Anchor.Application.MultiTenancy;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Application.Tests.Behaviors;

[RequiresTenant]
public sealed record TenantedRequest(string Payload) : IRequest<string>;

public sealed record HostRequest(string Payload) : IRequest<string>;

public sealed class TenantContextBehaviorTests
{
    [Fact]
    public async Task Throws_when_request_requires_tenant_and_current_tenant_is_null()
    {
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns((Guid?)null);
        var behavior = new TenantContextBehavior<TenantedRequest, string>(tenant);
        MessageHandlerDelegate<TenantedRequest, string> next = (_, _) => ValueTask.FromResult("ok");

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await behavior.Handle(new TenantedRequest("p"), next, CancellationToken.None));

        ex.Message.ShouldContain(nameof(TenantedRequest));
    }

    [Fact]
    public async Task Calls_next_when_request_requires_tenant_and_current_tenant_is_set()
    {
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(Guid.NewGuid());
        var behavior = new TenantContextBehavior<TenantedRequest, string>(tenant);
        var nextCalled = false;
        MessageHandlerDelegate<TenantedRequest, string> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult("ok");
        };

        var result = await behavior.Handle(new TenantedRequest("p"), next, CancellationToken.None);

        nextCalled.ShouldBeTrue();
        result.ShouldBe("ok");
    }

    [Fact]
    public async Task Calls_next_when_request_has_no_RequiresTenant_attribute()
    {
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns((Guid?)null);
        var behavior = new TenantContextBehavior<HostRequest, string>(tenant);
        MessageHandlerDelegate<HostRequest, string> next = (_, _) => ValueTask.FromResult("ok");

        var result = await behavior.Handle(new HostRequest("p"), next, CancellationToken.None);

        result.ShouldBe("ok");
    }
}
