using MediatR;
using Microsoft.AspNetCore.SignalR;
using Player.Application.Features.GameSessionUseCase.StartSoloRun;

namespace Player.Presentation.Hubs;

/// <summary>
/// Gameplay boundary owned by Player. A client never supplies a SignalR group:
/// the server adds it to the session group only after the command succeeds.
/// </summary>
public sealed class PlayerHub(ISender sender) : HubBase
{
    public async Task<SessionCommandAcknowledgement> CreateSession(CreateSoloSessionCommand command)
    {
        StartSoloRunResult result = await sender.Send<StartSoloRunResult>(
            new StartSoloRunCommand(command.PlayerId, command.HeroId, command.CommandId),
            Context.ConnectionAborted
        );
        SessionStateChanged state = SessionStateChanged.From(result);
        string groupName = GetSessionGroupName(result.SessionId);

        await AddCallerToGroupAndBroadcastAsync(groupName, nameof(SessionStateChanged), state);

        return SessionCommandAcknowledgement.Approve(state);
    }

    private static string GetSessionGroupName(Guid sessionId) => $"session:{sessionId}";
}

public sealed record CreateSoloSessionCommand(Guid CommandId, Guid PlayerId, Guid HeroId);

public sealed record SessionCommandAcknowledgement(bool Accepted, SessionStateChanged? Session)
{
    public static SessionCommandAcknowledgement Approve(SessionStateChanged session) =>
        new(true, session);
}

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
