using Aschott.Anchor.Application.Behaviors;
using FluentResults;
using FluentValidation;
using Mediator;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Application.Tests.Behaviors;

public sealed record CreateThing(string Name) : IRequest<Result<Guid>>;

public sealed class CreateThingValidator : AbstractValidator<CreateThing>
{
    public CreateThingValidator() => RuleFor(x => x.Name).NotEmpty();
}

public sealed record PingNotResult(string Msg) : IRequest<string>;

public sealed class PingNotResultValidator : AbstractValidator<PingNotResult>
{
    public PingNotResultValidator() => RuleFor(x => x.Msg).NotEmpty();
}

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Calls_next_when_no_validators_are_registered()
    {
        var behavior = new ValidationBehavior<CreateThing, Result<Guid>>([]);
        var nextCalled = false;
        MessageHandlerDelegate<CreateThing, Result<Guid>> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult(Result.Ok(Guid.NewGuid()));
        };

        var result = await behavior.Handle(new CreateThing("ok"), next, CancellationToken.None);

        nextCalled.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Calls_next_when_validators_pass()
    {
        var behavior = new ValidationBehavior<CreateThing, Result<Guid>>([new CreateThingValidator()]);
        var nextCalled = false;
        MessageHandlerDelegate<CreateThing, Result<Guid>> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult(Result.Ok(Guid.NewGuid()));
        };

        var result = await behavior.Handle(new CreateThing("ok"), next, CancellationToken.None);

        nextCalled.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Returns_failed_result_when_validators_fail_and_response_is_Result_T()
    {
        var behavior = new ValidationBehavior<CreateThing, Result<Guid>>([new CreateThingValidator()]);
        var nextCalled = false;
        MessageHandlerDelegate<CreateThing, Result<Guid>> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult(Result.Ok(Guid.NewGuid()));
        };

        var result = await behavior.Handle(new CreateThing(""), next, CancellationToken.None);

        nextCalled.ShouldBeFalse();
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
        result.Errors[0].Message.ShouldContain("Name");
    }

    [Fact]
    public async Task Throws_when_validators_fail_and_response_is_not_Result_T()
    {
        var behavior = new ValidationBehavior<PingNotResult, string>([new PingNotResultValidator()]);
        MessageHandlerDelegate<PingNotResult, string> next = (_, _) => ValueTask.FromResult("ok");

        await Should.ThrowAsync<ValidationException>(async () =>
            await behavior.Handle(new PingNotResult(""), next, CancellationToken.None));
    }
}
