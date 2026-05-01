using Aschott.Anchor.AspNetCore.Errors;
using Aschott.Anchor.AspNetCore.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Aschott.Anchor.AspNetCore.DependencyInjection;

public static class AnchorAspNetCoreExtensions
{
    /// <summary>
    /// Registers the default <see cref="ITenantResolver"/> chain
    /// (header → claim → host) suitable for most apps. Consumers can
    /// add or replace resolvers afterward.
    /// </summary>
    public static IServiceCollection AddAnchorAspNetCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ITenantResolver, HeaderTenantResolver>();
        services.AddScoped<ITenantResolver, ClaimTenantResolver>();
        services.AddScoped<ITenantResolver, HostTenantResolver>();

        return services;
    }

    /// <summary>
    /// Wires the canonical Anchor middleware pipeline:
    /// exception handling → tenant context.
    /// </summary>
    public static IApplicationBuilder UseAnchorAspNetCore(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<AnchorExceptionHandlingMiddleware>();
        app.UseMiddleware<TenantContextMiddleware>();
        return app;
    }
}
