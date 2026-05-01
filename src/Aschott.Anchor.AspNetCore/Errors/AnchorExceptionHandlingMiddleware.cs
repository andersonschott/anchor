using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aschott.Anchor.AspNetCore.Errors;

/// <summary>
/// Last-line exception handler. Logs the exception and writes a
/// 500 ProblemDetails-style JSON body so unhandled errors never leak
/// stack traces to clients.
/// </summary>
public sealed class AnchorExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<AnchorExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(
                new
                {
                    title = "Internal Server Error",
                    status = StatusCodes.Status500InternalServerError,
                },
                options: null,
                contentType: "application/problem+json");
        }
    }
}
