using Aschott.Anchor.Application.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Aschott.Anchor.AspNetCore.MultiTenancy;

/// <summary>
/// Walks the registered <see cref="ITenantResolver"/> chain in order and
/// installs the first non-null tenant id into <see cref="ICurrentTenant"/>
/// for the duration of the request.
/// </summary>
/// <remarks>
/// <see cref="ITenantResolver"/> implementations may be registered as Scoped.
/// Method injection on <see cref="InvokeAsync"/> resolves parameters from the
/// per-request scope, avoiding the captive-dependency issue that occurs when
/// Scoped services are injected via the constructor (which uses the root
/// service provider).
/// </remarks>
public sealed class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext ctx,
        ICurrentTenant currentTenant,
        IEnumerable<ITenantResolver> resolvers)
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
