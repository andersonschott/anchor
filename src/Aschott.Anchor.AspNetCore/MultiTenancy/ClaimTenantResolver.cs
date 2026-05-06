using Microsoft.AspNetCore.Http;

namespace Aschott.Anchor.AspNetCore.MultiTenancy;

/// <summary>
/// Reads the tenant id from a JWT/cookie claim (default <c>tenantid</c>).
/// </summary>
public sealed class ClaimTenantResolver(string claimType = "tenantid") : ITenantResolver
{
    public Guid? Resolve(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var claim = context.User.FindFirst(claimType);
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
