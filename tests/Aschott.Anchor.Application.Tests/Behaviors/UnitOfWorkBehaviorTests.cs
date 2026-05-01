using Aschott.Anchor.Application.Behaviors;
using Aschott.Anchor.Application.UnitOfWork;
using FluentResults;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Application.Tests.Behaviors;

public sealed record SampleCommand(string Name) : Aschott.Anchor.Application.Cqrs.ICommand<Guid>;
public sealed record SampleQuery(Guid Id) : Aschott.Anchor.Application.Cqrs.IQuery<string>;
public sealed record PlainRequest(string Msg) : Mediator.IRequest<string>;

public sealed class UnitOfWorkBehaviorTests
{
    [Fact]
    public async Task Saves_changes_after_successful_command()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
        var behavior = new UnitOfWorkBehavior<SampleCommand, Result<Guid>>(uow);
        MessageHandlerDelegate<SampleCommand, Result<Guid>> next = (_, _) =>
            ValueTask.FromResult(Result.Ok(Guid.NewGuid()));

        await behavior.Handle(new SampleCommand("x"), next, CancellationToken.None);

        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_save_changes_for_query()
    {
        var uow = Substitute.For<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<SampleQuery, Result<string>>(uow);
        MessageHandlerDelegate<SampleQuery, Result<string>> next = (_, _) =>
            ValueTask.FromResult(Result.Ok("v"));

        await behavior.Handle(new SampleQuery(Guid.NewGuid()), next, CancellationToken.None);

        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_save_changes_for_plain_request_that_is_not_a_command()
    {
        var uow = Substitute.For<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<PlainRequest, string>(uow);
        MessageHandlerDelegate<PlainRequest, string> next = (_, _) => ValueTask.FromResult("ok");

        await behavior.Handle(new PlainRequest("x"), next, CancellationToken.None);

        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_save_changes_when_inner_handler_throws()
    {
        var uow = Substitute.For<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<SampleCommand, Result<Guid>>(uow);
        MessageHandlerDelegate<SampleCommand, Result<Guid>> next =
            (_, _) => throw new InvalidOperationException("boom");

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await behavior.Handle(new SampleCommand("x"), next, CancellationToken.None));

        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
