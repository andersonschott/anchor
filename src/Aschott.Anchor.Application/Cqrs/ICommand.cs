using FluentResults;
using Mediator;

namespace Aschott.Anchor.Application.Cqrs;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
