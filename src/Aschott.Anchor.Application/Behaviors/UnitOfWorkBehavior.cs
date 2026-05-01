using Aschott.Anchor.Application.UnitOfWork;
using Mediator;

namespace Aschott.Anchor.Application.Behaviors;

/// <summary>
/// After a successful command pipeline (<c>TRequest : ICommand&lt;...&gt;</c>),
/// commits the unit of work. Queries pass through unchanged.
/// Exceptions in <c>next()</c> propagate without commit (implicit rollback).
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Type AnchorCommandOpenGeneric = typeof(Cqrs.ICommand<>);

    private static readonly bool IsCommand =
        typeof(TRequest).GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == AnchorCommandOpenGeneric);

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);
        if (IsCommand)
            await unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}
