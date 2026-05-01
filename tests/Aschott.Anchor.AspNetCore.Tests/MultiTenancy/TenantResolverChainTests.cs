using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.AspNetCore.Tests.MultiTenancy;

public sealed class TenantResolverChainTests : IClassFixture<TestAppFixture>
{
    private readonly TestAppFixture _fixture;

    public TenantResolverChainTests(TestAppFixture fixture) => _fixture = fixture;

    private sealed record WhoAmIResponse(Guid? TenantId);

    [Fact]
    public async Task Header_tenant_is_resolved_into_current_tenant()
    {
        var tenantId = Guid.NewGuid();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/whoami", UriKind.Relative));
        request.Headers.Add("X-Tenant", tenantId.ToString());

        using var response = await _fixture.Client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<WhoAmIResponse>();

        body.ShouldNotBeNull();
        body.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task Host_subdomain_guid_is_resolved_when_no_header()
    {
        var tenantId = Guid.NewGuid();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/whoami", UriKind.Relative));
        request.Headers.Host = $"{tenantId}.app.example.com";

        using var response = await _fixture.Client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<WhoAmIResponse>();

        body.ShouldNotBeNull();
        body.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task No_signal_yields_null_current_tenant()
    {
        var body = await _fixture.Client.GetFromJsonAsync<WhoAmIResponse>(
            new Uri("/whoami", UriKind.Relative));

        body.ShouldNotBeNull();
        body.TenantId.ShouldBeNull();
    }
}
