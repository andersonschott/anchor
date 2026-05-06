using Aschott.Anchor.Application.MultiTenancy;
using Aschott.Anchor.AspNetCore.DependencyInjection;
using Aschott.Anchor.AspNetCore.Endpoints;
using Aschott.Anchor.AspNetCore.Errors;
using Aschott.Anchor.Infrastructure.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Aschott.Anchor.AspNetCore.Tests;

public sealed class WhoAmIEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/whoami", (ICurrentTenant tenant) =>
            Results.Json(new { tenantId = tenant.Id }));
    }
}

public sealed class BoomEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/boom", IResult () => throw new InvalidOperationException("kaboom"));
    }
}

public sealed class ProblemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/problem", () => ApiResults.Problem(
        [
            new FluentResults.Error("name: required"),
        ]));
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "Disposed via IAsyncLifetime.DisposeAsync, called by xUnit.")]
public sealed class TestAppFixture : IAsyncLifetime
{
    private IHost _host = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddAnchorEndpoints(typeof(TestAppFixture).Assembly);
                    services.AddAnchorAspNetCore();
                    services.AddScoped<ICurrentTenant, CurrentTenantAccessor>();
                });
                web.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAnchorAspNetCore();
                    app.UseEndpoints(routes => routes.MapAnchorEndpoints());
                });
            })
            .Build();

        await _host.StartAsync();
        Client = _host.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }
}
