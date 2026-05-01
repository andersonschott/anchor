using Aschott.Anchor.AspNetCore.Endpoints;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.AspNetCore.Tests.Endpoints;

public sealed class EndpointVersionAttributeTests
{
    [EndpointVersion(2)]
    private sealed class V2Endpoint;

    [Fact]
    public void Stores_supplied_version_on_attribute()
    {
        var attribute = typeof(V2Endpoint).GetCustomAttributes(typeof(EndpointVersionAttribute), inherit: false);

        attribute.ShouldHaveSingleItem();
        ((EndpointVersionAttribute)attribute[0]).Version.ShouldBe(2);
    }
}
