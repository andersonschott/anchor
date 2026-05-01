using Aschott.Anchor.Application.Behaviors;
using Aschott.Anchor.Application.DependencyInjection;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Application.Tests.DependencyInjection;

public sealed class AnchorApplicationExtensionsTests
{
    [Fact]
    public void Registers_pipeline_behaviors_in_canonical_order()
    {
        var services = new ServiceCollection();

        services.AddAnchorApplication();

        var registrations = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType)
            .ToList();

        registrations.ShouldBe(
        [
            typeof(LoggingBehavior<,>),
            typeof(TenantContextBehavior<,>),
            typeof(ValidationBehavior<,>),
            typeof(UnitOfWorkBehavior<,>),
            typeof(DomainEventDispatchBehavior<,>),
        ]);
    }

    [Fact]
    public void Registers_validators_when_assemblies_are_supplied()
    {
        var services = new ServiceCollection();

        services.AddAnchorApplication(typeof(AnchorApplicationExtensionsTests).Assembly);

        services.Any(d => d.ServiceType.IsGenericType
            && d.ServiceType.GetGenericTypeDefinition() == typeof(FluentValidation.IValidator<>))
            .ShouldBeTrue();
    }
}
