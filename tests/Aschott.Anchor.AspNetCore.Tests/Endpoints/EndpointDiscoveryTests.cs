using System.Net;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.AspNetCore.Tests.Endpoints;

public sealed class EndpointDiscoveryTests : IClassFixture<TestAppFixture>
{
    private readonly TestAppFixture _fixture;

    public EndpointDiscoveryTests(TestAppFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Discovered_endpoint_responds_with_200()
    {
        var response = await _fixture.Client.GetAsync(new Uri("/whoami", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
