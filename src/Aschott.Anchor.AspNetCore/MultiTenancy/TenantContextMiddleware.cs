using Aschott.Anchor.Application.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Aschott.Anchor.AspNetCore.MultiTenancy;

/// <summary>
/// Walks the registered <see cref="ITenantResolver"/> chain in order and
/// installs the first non-null tenant id into <see cref="ICurrentTenant"/>
/// for the duration of the request.
/// </summary>
public sealed class TenantContextMiddleware(RequestDelegate next, IEnumerable<ITenantResolver> resolvers)
{
    public async Task InvokeAsync(HttpContext ctx, ICurrentTenant currentTenant)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(currentTenant);

        Guid? tenantId = null;
        foreach (var resolver in resolvers)
        {
            tenantId = resolver.Resolve(ctx);
            if (tenantId is not null) break;
        }

        using var _ = currentTenant.Change(tenantId);
        await next(ctx);
    }
}
