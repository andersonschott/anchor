using Microsoft.AspNetCore.Http;

namespace Aschott.Anchor.AspNetCore.MultiTenancy;

/// <summary>
/// Resolves the tenant id for the current request from a specific source
/// (claim, header, host, query string, etc.).
/// Returning <c>null</c> defers to the next resolver in the chain.
/// </summary>
public interface ITenantResolver
{
    Guid? Resolve(HttpContext context);
}
