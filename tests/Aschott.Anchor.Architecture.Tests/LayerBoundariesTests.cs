using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Architecture.Tests;

public sealed class LayerBoundariesTests
{
    [Fact]
    public void Domain_does_not_reference_Application_Infrastructure_or_AspNetCore()
    {
        var result = Types.InAssembly(typeof(Aschott.Anchor.Domain.Entities.Entity<>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Aschott.Anchor.Application",
                "Aschott.Anchor.Infrastructure",
                "Aschott.Anchor.AspNetCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_does_not_reference_Infrastructure_or_AspNetCore()
    {
        var result = Types.InAssembly(typeof(Aschott.Anchor.Application.Cqrs.ICommand<>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Aschott.Anchor.Infrastructure",
                "Aschott.Anchor.AspNetCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void AspNetCore_does_not_reference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Aschott.Anchor.AspNetCore.Endpoints.IEndpoint).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Aschott.Anchor.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Contracts_does_not_reference_Application_Infrastructure_or_AspNetCore()
    {
        var result = Types.InAssembly(typeof(Aschott.Anchor.Contracts.IntegrationEvent).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Aschott.Anchor.Application",
                "Aschott.Anchor.Infrastructure",
                "Aschott.Anchor.AspNetCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
