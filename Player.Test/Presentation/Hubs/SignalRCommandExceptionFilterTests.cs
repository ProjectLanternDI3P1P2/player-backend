using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Player.Presentation.Hubs.Filters;

namespace Player.Test.Presentation.Hubs;

public sealed class SignalRCommandExceptionFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_WhenApplicationCommandConflicts_ReturnsGenericRejection()
    {
        var context = new Mock<HubCallerContext>();
        context.SetupGet(value => value.ConnectionAborted).Returns(CancellationToken.None);
        var hub = new TestHub { Context = context.Object };
        var invocation = new HubInvocationContext(
            context.Object,
            Mock.Of<IServiceProvider>(),
            hub,
            typeof(TestHub).GetMethod(nameof(TestHub.AnyCommand))!,
            []
        );
        var filter = new SignalRCommandExceptionFilter(
            Mock.Of<ILogger<SignalRCommandExceptionFilter>>()
        );

        object? result = await filter.InvokeMethodAsync(
            invocation,
            _ =>
                ValueTask.FromException<object?>(
                    new Player.Domain.Exceptions.ConflictException("State already changed.")
                )
        );

        result
            .Should()
            .Be(
                new SignalRCommandRejection(
                    false,
                    new SignalRCommandError("COMMAND_CONFLICT", "State already changed.")
                )
            );
    }

    private sealed class TestHub : Hub
    {
        public Task AnyCommand() => Task.CompletedTask;
    }
}
