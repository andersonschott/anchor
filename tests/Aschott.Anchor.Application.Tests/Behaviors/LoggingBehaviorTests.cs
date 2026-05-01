using Aschott.Anchor.Application.Behaviors;
using Mediator;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Application.Tests.Behaviors;

public sealed record LoggingPing(string Msg) : IRequest<string>;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Returns_inner_handler_response_unchanged()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<LoggingPing, string>>>();
        var behavior = new LoggingBehavior<LoggingPing, string>(logger);
        MessageHandlerDelegate<LoggingPing, string> next = (_, _) => ValueTask.FromResult("ok");

        var result = await behavior.Handle(new LoggingPing("hi"), next, CancellationToken.None);

        result.ShouldBe("ok");
    }

    [Fact]
    public async Task Logs_at_least_two_information_messages_around_next()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<LoggingPing, string>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var behavior = new LoggingBehavior<LoggingPing, string>(logger);
        MessageHandlerDelegate<LoggingPing, string> next = (_, _) => ValueTask.FromResult("ok");

        await behavior.Handle(new LoggingPing("hi"), next, CancellationToken.None);

        logger.ReceivedCalls().Count().ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Logs_error_and_rethrows_when_inner_handler_throws()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<LoggingPing, string>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var behavior = new LoggingBehavior<LoggingPing, string>(logger);
        var boom = new InvalidOperationException("kaboom");
        MessageHandlerDelegate<LoggingPing, string> next = (_, _) => throw boom;

        var thrown = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await behavior.Handle(new LoggingPing("x"), next, CancellationToken.None));

        thrown.ShouldBe(boom);
        logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            boom,
            Arg.Any<Func<object, Exception?, string>>()!);
    }
}
