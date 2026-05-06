using FluentResults;
using Mediator;

namespace Aschott.Anchor.Application.Cqrs;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
