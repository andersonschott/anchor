using FluentResults;
using Mediator;

namespace Aschott.Anchor.Application.Cqrs;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
