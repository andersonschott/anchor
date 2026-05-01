using System.Reflection;
using Aschott.Anchor.Application.Behaviors;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Aschott.Anchor.Application.DependencyInjection;

public static class AnchorApplicationExtensions
{
    /// <summary>
    /// Registers Anchor's CQRS pipeline behaviors in canonical execution order:
    /// Logging → TenantContext → Validation → UnitOfWork → DomainEventDispatch.
    /// FluentValidation validators are scanned from <paramref name="validatorAssemblies"/>.
    /// Consumers must call <c>services.AddMediator(...)</c> separately — Mediator 3.x emits
    /// its DI extension via source generator at the consumer's assembly.
    /// </summary>
    public static IServiceCollection AddAnchorApplication(
        this IServiceCollection services,
        params Assembly[] validatorAssemblies)
    {
        if (validatorAssemblies.Length > 0)
            services.AddValidatorsFromAssemblies(validatorAssemblies, includeInternalTypes: true);

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TenantContextBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>));

        return services;
    }
}
