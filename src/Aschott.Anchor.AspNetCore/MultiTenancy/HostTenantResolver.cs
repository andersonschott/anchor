using Microsoft.AspNetCore.Http;

namespace Aschott.Anchor.AspNetCore.MultiTenancy;

/// <summary>
/// Reads the tenant id from the leading host segment when it parses as a Guid
/// (e.g. <c>3f7b...e1c.app.example.com</c>). For human-friendly subdomain
/// → tenant lookup, the Tenants module (F3+) plugs in a richer resolver.
/// </summary>
public sealed class HostTenantResolver : ITenantResolver
{
    public Guid? Resolve(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var host = context.Request.Host.Host;
        var dot = host.IndexOf('.', StringComparison.Ordinal);
        if (dot <= 0) return null;

        var leading = host[..dot];
        return Guid.TryParse(leading, out var id) ? id : null;
    }
}
