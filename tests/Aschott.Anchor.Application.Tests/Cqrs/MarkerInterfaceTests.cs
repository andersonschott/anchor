using Aschott.Anchor.Application.Cqrs;
using FluentResults;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Application.Tests.Cqrs;

public sealed class MarkerInterfaceTests
{
    private sealed record CreateCustomer(string Name) : ICommand<Guid>;
    private sealed record GetCustomer(Guid Id) : IQuery<string>;

    [Fact]
    public void ICommand_extends_IRequest_with_Result_wrapping()
    {
        var cmd = new CreateCustomer("Acme");

        cmd.ShouldBeAssignableTo<Mediator.IRequest<Result<Guid>>>();
    }

    [Fact]
    public void IQuery_extends_IRequest_with_Result_wrapping()
    {
        var q = new GetCustomer(Guid.NewGuid());

        q.ShouldBeAssignableTo<Mediator.IRequest<Result<string>>>();
    }
}
