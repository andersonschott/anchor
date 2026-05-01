using Microsoft.AspNetCore.Http;

namespace Aschott.Anchor.AspNetCore.MultiTenancy;

/// <summary>
/// Reads the tenant id from a query-string parameter (default <c>tenant</c>).
/// Intended for development/testing only — register conditionally on
/// <c>IHostEnvironment.IsDevelopment()</c>.
/// </summary>
public sealed class QueryStringTenantResolver(string parameterName = "tenant") : ITenantResolver
{
    public Guid? Resolve(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Query.TryGetValue(parameterName, out var values))
            return null;

        return Guid.TryParse(values.ToString(), out var id) ? id : null;
    }
}
