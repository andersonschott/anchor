using System.Reflection;
using FluentResults;
using FluentValidation;
using Mediator;

namespace Aschott.Anchor.Application.Behaviors;

/// <summary>
/// Runs registered <see cref="IValidator{T}"/>s for the request. If any fail and
/// <typeparamref name="TResponse"/> is <c>Result&lt;T&gt;</c>, returns a failed result;
/// otherwise throws <see cref="FluentValidation.ValidationException"/>.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorList = validators.ToList();
        if (validatorList.Count == 0)
            return await next(message, cancellationToken);

        var ctx = new ValidationContext<TRequest>(message);
        var results = await Task.WhenAll(validatorList.Select(v => v.ValidateAsync(ctx, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
            return await next(message, cancellationToken);

        var responseType = typeof(TResponse);
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
            throw new ValidationException(failures);

        var errors = failures
            .Select(f => (IError)new Error($"{f.PropertyName}: {f.ErrorMessage}"))
            .ToList();

        var innerType = responseType.GetGenericArguments()[0];
        var failMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Result.Fail)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 1
                         && m.GetParameters()[0].ParameterType == typeof(IEnumerable<IError>))
            .MakeGenericMethod(innerType);

        return (TResponse)failMethod.Invoke(null, [errors])!;
    }
}
