using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Player.Application.Features.GameSessionUseCase.StartSoloRun;
using Player.Domain.Exceptions;

namespace Player.Presentation.Hubs;

/// <summary>
/// Gameplay boundary owned by Player. A client never supplies a SignalR group:
/// the server adds it to the session group only after the command succeeds.
/// </summary>
public sealed class PlayerHub(ISender sender, ILogger<PlayerHub> logger) : Hub
{
    public async Task<SessionCommandAcknowledgement> CreateSession(CreateSoloSessionCommand command)
    {
        try
        {
            StartSoloRunResult result = await sender.Send<StartSoloRunResult>(
                new StartSoloRunCommand(command.PlayerId, command.HeroId, command.CommandId),
                Context.ConnectionAborted
            );
            SessionStateChanged state = SessionStateChanged.From(result);
            string groupName = GetSessionGroupName(result.SessionId);

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                groupName,
                Context.ConnectionAborted
            );
            await Clients
                .Group(groupName)
                .SendAsync(nameof(SessionStateChanged), state, Context.ConnectionAborted);

            return SessionCommandAcknowledgement.Approve(state);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "PlayerHub CreateSession rejected for player {PlayerId}, hero {HeroId}, command {CommandId}",
                command.PlayerId,
                command.HeroId,
                command.CommandId
            );
            return SessionCommandAcknowledgement.Rejected(ToError(exception));
        }
    }

    private static string GetSessionGroupName(Guid sessionId) => $"session:{sessionId}";

    private static SessionCommandError ToError(Exception exception) =>
        exception switch
        {
            ValidationException => new("VALIDATION_ERROR", "The game creation request is invalid."),
            KeyNotFoundException => new("HERO_NOT_FOUND", "The selected hero was not found."),
            ConflictException => new("SESSION_CONFLICT", exception.Message),
            _ => new("GAME_CREATION_FAILED", "The game could not be created. Please retry."),
        };
}

public sealed record CreateSoloSessionCommand(Guid CommandId, Guid PlayerId, Guid HeroId);

public sealed record SessionCommandAcknowledgement(
    bool Accepted,
    SessionStateChanged? Session,
    SessionCommandError? Error
)
{
    public static SessionCommandAcknowledgement Approve(SessionStateChanged session) =>
        new(true, session, null);

    public static SessionCommandAcknowledgement Rejected(SessionCommandError error) =>
        new(false, null, error);
}

public sealed record SessionCommandError(string Code, string Message);

public sealed record SessionStateChanged(
    Guid SessionId,
    SessionHero Hero,
    string State,
    Guid? DungeonRunId,
    string? DungeonSeed,
    string? FailureReason,
    bool AlreadyExists
)
{
    public static SessionStateChanged From(StartSoloRunResult result) =>
        new(
            result.SessionId,
            result.Hero,
            result.State,
            result.DungeonRunId,
            result.DungeonSeed,
            result.FailureReason,
            result.AlreadyExists
        );
}
