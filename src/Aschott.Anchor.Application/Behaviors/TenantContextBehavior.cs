using Aschott.Anchor.Application.MultiTenancy;
using Mediator;

namespace Aschott.Anchor.Application.Behaviors;

public sealed class TenantContextBehavior<TRequest, TResponse>(ICurrentTenant currentTenant)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly bool RequiresTenant =
        typeof(TRequest).IsDefined(typeof(RequiresTenantAttribute), inherit: false);

    public ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (RequiresTenant && currentTenant.Id is null)
        {
            throw new InvalidOperationException(
                $"{typeof(TRequest).Name} requires a tenant context, but ICurrentTenant.Id is null.");
        }

        return next(message, cancellationToken);
    }
}
