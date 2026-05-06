using System.Net;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.AspNetCore.Tests.Errors;

public sealed class ExceptionHandlingMiddlewareTests : IClassFixture<TestAppFixture>
{
    private readonly TestAppFixture _fixture;

    public ExceptionHandlingMiddlewareTests(TestAppFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Unhandled_exception_returns_500_problem_json()
    {
        var response = await _fixture.Client.GetAsync(new Uri("/boom", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task ApiResults_Problem_returns_400_problem_details()
    {
        var response = await _fixture.Client.GetAsync(new Uri("/problem", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
