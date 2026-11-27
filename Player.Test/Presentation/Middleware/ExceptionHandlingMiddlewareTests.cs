using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Player.Domain.Exceptions;
using Player.Presentation.Middleware;
using Serilog;

namespace Player.Test.Presentation.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenTheHeroIsAlreadyInASession_Returns409WithTheOngoingSessionId()
    {
        var conflict = new HeroAlreadyInSessionException(Guid.NewGuid(), Guid.NewGuid());

        (int status, JsonElement body) = await InvokeAsync(conflict);

        status.Should().Be(StatusCodes.Status409Conflict);
        body.GetProperty("activeSessionId").GetGuid().Should().Be(conflict.ActiveSessionId);
    }

    [Fact]
    public async Task InvokeAsync_WhenAnotherConflictOccurs_Returns409WithoutASessionId()
    {
        (int status, JsonElement body) = await InvokeAsync(new ConflictException("Conflict."));

        status.Should().Be(StatusCodes.Status409Conflict);
        body.TryGetProperty("activeSessionId", out _).Should().BeFalse();
    }

    private static async Task<(int Status, JsonElement Body)> InvokeAsync(Exception exception)
    {
        var middleware = new ExceptionHandlingMiddleware(
            new LoggerConfiguration().CreateLogger(),
            Mock.Of<IHostEnvironment>()
        );
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => throw exception);

        context.Response.Body.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(
            context.Response.Body,
            cancellationToken: TestContext.Current.CancellationToken
        );
        return (context.Response.StatusCode, document.RootElement.Clone());
    }
}
