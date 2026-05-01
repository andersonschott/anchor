using Aschott.Anchor.AspNetCore.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.AspNetCore.Tests.MultiTenancy;

public sealed class QueryStringTenantResolverTests
{
    [Fact]
    public void Returns_tenant_id_when_default_query_parameter_is_a_guid()
    {
        var tenantId = Guid.NewGuid();
        var ctx = new DefaultHttpContext();
        ctx.Request.QueryString = new QueryString($"?tenant={tenantId}");

        var resolved = new QueryStringTenantResolver().Resolve(ctx);

        resolved.ShouldBe(tenantId);
    }

    [Fact]
    public void Returns_null_when_parameter_is_absent()
    {
        var ctx = new DefaultHttpContext();

        var resolved = new QueryStringTenantResolver().Resolve(ctx);

        resolved.ShouldBeNull();
    }

    [Fact]
    public void Returns_null_when_value_is_not_a_guid()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.QueryString = new QueryString("?tenant=not-a-guid");

        var resolved = new QueryStringTenantResolver().Resolve(ctx);

        resolved.ShouldBeNull();
    }

    [Fact]
    public void Honors_custom_parameter_name()
    {
        var tenantId = Guid.NewGuid();
        var ctx = new DefaultHttpContext();
        ctx.Request.QueryString = new QueryString($"?org={tenantId}");

        var resolved = new QueryStringTenantResolver("org").Resolve(ctx);

        resolved.ShouldBe(tenantId);
    }
}
