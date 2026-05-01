using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Aschott.Anchor.AspNetCore.Endpoints;

public static class EndpointMappingExtensions
{
    /// <summary>
    /// Scans the supplied assemblies for non-abstract <see cref="IEndpoint"/>
    /// implementations and registers them as transient services.
    /// </summary>
    public static IServiceCollection AddAnchorEndpoints(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var endpointType in assemblies
                     .SelectMany(a => a.GetTypes())
                     .Where(t => !t.IsAbstract && t.IsClass && typeof(IEndpoint).IsAssignableFrom(t)))
        {
            services.AddTransient(typeof(IEndpoint), endpointType);
        }

        return services;
    }

    /// <summary>
    /// Maps every registered <see cref="IEndpoint"/> against the route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapAnchorEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        foreach (var endpoint in app.ServiceProvider.GetServices<IEndpoint>())
            endpoint.MapEndpoint(app);

        return app;
    }
}
