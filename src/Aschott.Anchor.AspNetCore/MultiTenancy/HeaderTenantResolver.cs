using Microsoft.AspNetCore.Http;

namespace Aschott.Anchor.AspNetCore.MultiTenancy;

/// <summary>
/// Reads the tenant id from a request header (default <c>X-Tenant</c>).
/// </summary>
public sealed class HeaderTenantResolver(string headerName = "X-Tenant") : ITenantResolver
{
    public Guid? Resolve(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Headers.TryGetValue(headerName, out var values))
            return null;

        var raw = values.ToString();
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
