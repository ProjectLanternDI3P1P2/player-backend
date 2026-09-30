using MediatR;
using Player.Application.Features.GameSessionUseCase;
using Player.Application.Features.GameSessionUseCase.ChangeSessionHero;
using Player.Application.Features.GameSessionUseCase.CreateSoloLobby;
using Player.Application.Features.GameSessionUseCase.GetSessionSnapshot;
using Player.Application.Features.GameSessionUseCase.StartSession;

namespace Player.Presentation.Hubs;

/// <summary>Gameplay boundary for Player-owned lobbies and game sessions.</summary>
public sealed class PlayerHub(ISender sender) : HubBase
{
    public async Task<SessionCommandAcknowledgement> CreateSoloLobby(CreateSoloLobbyRequest command)
    {
        GameSessionSnapshot session = await sender.Send<GameSessionSnapshot>(
            new CreateSoloLobbyCommand(command.PlayerId, command.HeroId, command.CommandId),
            Context.ConnectionAborted
        );
        await PublishSessionAsync(session);
        return SessionCommandAcknowledgement.Approve(session);
    }

    public async Task<SessionCommandAcknowledgement> GetSessionSnapshot(
        SessionSnapshotRequest request
    )
    {
        GameSessionSnapshot session = await sender.Send(
            new GetSessionSnapshotQuery(request.PlayerId, request.SessionId),
            Context.ConnectionAborted
        );
        await AddCallerToGroupAsync(GroupName(session.SessionId));
        return SessionCommandAcknowledgement.Approve(session);
    }

    public async Task<SessionCommandAcknowledgement> StartSession(StartSessionRequest command)
    {
        GameSessionSnapshot session = await sender.Send<GameSessionSnapshot>(
            new StartSessionCommand(command.PlayerId, command.SessionId, command.CommandId),
            Context.ConnectionAborted
        );
        await PublishSessionAsync(session);
        return SessionCommandAcknowledgement.Approve(session);
    }

    public async Task<SessionCommandAcknowledgement> ChangeSessionHero(ChangeSessionHeroRequest command)
    {
        GameSessionSnapshot session = await sender.Send<GameSessionSnapshot>(
            new ChangeSessionHeroCommand(command.PlayerId, command.SessionId, command.HeroId, command.CommandId),
            Context.ConnectionAborted
        );
        await PublishSessionAsync(session);
        return SessionCommandAcknowledgement.Approve(session);
    }

    private async Task PublishSessionAsync(GameSessionSnapshot session)
    {
        string groupName = GroupName(session.SessionId);
        await AddCallerToGroupAsync(groupName);
        await BroadcastToGroupAsync(
            groupName,
            nameof(SessionStateChanged),
            SessionStateChanged.From(session)
        );
    }

    private static string GroupName(Guid sessionId) => $"session:{sessionId}";
}

public sealed record CreateSoloLobbyRequest(Guid CommandId, Guid PlayerId, Guid HeroId);

public sealed record StartSessionRequest(Guid CommandId, Guid PlayerId, Guid SessionId);

public sealed record ChangeSessionHeroRequest(Guid CommandId, Guid PlayerId, Guid SessionId, Guid HeroId);

public sealed record SessionSnapshotRequest(Guid PlayerId, Guid SessionId);

public sealed record SessionCommandAcknowledgement(bool Accepted, SessionStateChanged? Session)
{
    public static SessionCommandAcknowledgement Approve(GameSessionSnapshot session) =>
        new(true, SessionStateChanged.From(session));
}

public sealed record SessionStateChanged(
    Guid SessionId,
    Guid CreatorPlayerId,
    string State,
    string Mode,
    IReadOnlyList<SessionHero> Members,
    Guid? DungeonRunId,
    string? DungeonSeed,
    string? FailureReason,
    bool AlreadyExists
)
{
    public static SessionStateChanged From(GameSessionSnapshot session) =>
        new(
            session.SessionId,
            session.CreatorPlayerId,
            session.State,
            session.Mode,
            session.Members,
            session.DungeonRunId,
            session.DungeonSeed,
            session.FailureReason,
            session.AlreadyExists
        );
}
