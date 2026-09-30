using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Player.Domain.Exceptions;

namespace Player.Presentation.Hubs.Filters;

/// <summary>
/// Converts application exceptions from every gameplay Hub into the same safe,
/// explicit command rejection. Hubs only implement accepted transitions.
/// </summary>
public sealed class SignalRCommandExceptionFilter(ILogger<SignalRCommandExceptionFilter> logger)
    : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next
    )
    {
        try
        {
            return await next(invocationContext);
        }
        catch (OperationCanceledException)
            when (invocationContext.Context.ConnectionAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "SignalR command {Hub}.{Method} was rejected",
                invocationContext.Hub.GetType().Name,
                invocationContext.HubMethodName
            );
            return SignalRCommandRejection.From(exception);
        }
    }
}

public sealed record SignalRCommandRejection(bool Accepted, SignalRCommandError Error)
{
    public static SignalRCommandRejection From(Exception exception) =>
        new(false, SignalRCommandError.From(exception));
}

public sealed record SignalRCommandError(string Code, string Message)
{
    public static SignalRCommandError From(Exception exception) =>
        exception switch
        {
            ValidationException => new("VALIDATION_ERROR", "The command is invalid."),
            KeyNotFoundException => new(
                "RESOURCE_NOT_FOUND",
                "The requested resource was not found."
            ),
            ConflictException => new("COMMAND_CONFLICT", exception.Message),
            _ => new("COMMAND_FAILED", "The command could not be completed. Please retry."),
        };
}
