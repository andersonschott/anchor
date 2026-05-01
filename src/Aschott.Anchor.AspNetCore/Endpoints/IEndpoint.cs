using Microsoft.AspNetCore.Routing;

namespace Aschott.Anchor.AspNetCore.Endpoints;

/// <summary>
/// Implementations contribute one or more route mappings to the application's
/// endpoint route builder. Discovered via assembly scanning and invoked by
/// <c>MapAnchorEndpoints</c>.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
