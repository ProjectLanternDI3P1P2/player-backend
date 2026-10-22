using MediatR;
using Microsoft.AspNetCore.Http.Connections.Features;
using Player.Application.Features.GameSessionUseCase;
using Player.Application.Features.GameSessionUseCase.ChangeSessionHero;
using Player.Application.Features.GameSessionUseCase.CreateSoloLobby;
using Player.Application.Features.GameSessionUseCase.GetSessionSnapshot;
using Player.Application.Features.GameSessionUseCase.StartSession;
using Player.Presentation.Authentication;

namespace Player.Presentation.Hubs;

/// <summary>Gameplay boundary for Player-owned lobbies and game sessions.</summary>
public sealed class PlayerHub(ISender sender, ICurrentPlayerResolver currentPlayer) : HubBase
{
    public async Task<SessionCommandAcknowledgement> CreateSoloLobby(CreateSoloLobbyRequest command)
    {
        Guid playerId = await ResolvePlayerIdAsync();
        GameSessionSnapshot session = await sender.Send<GameSessionSnapshot>(
            new CreateSoloLobbyCommand(
                playerId,
                command.HeroId,
                command.DungeonSeed,
                command.CommandId
            ),
            Context.ConnectionAborted
        );
        await PublishSessionAsync(session);
        return SessionCommandAcknowledgement.Approve(session);
    }

    public async Task<SessionCommandAcknowledgement> GetSessionSnapshot(
        SessionSnapshotRequest request
    )
    {
        Guid playerId = await ResolvePlayerIdAsync();
        GameSessionSnapshot session = await sender.Send(
            new GetSessionSnapshotQuery(playerId, request.SessionId),
            Context.ConnectionAborted
        );
        await AddCallerToGroupAsync(GroupName(session.SessionId));
        return SessionCommandAcknowledgement.Approve(session);
    }

    public async Task<SessionCommandAcknowledgement> StartSession(StartSessionRequest command)
    {
        Guid playerId = await ResolvePlayerIdAsync();
        GameSessionSnapshot session = await sender.Send<GameSessionSnapshot>(
            new StartSessionCommand(playerId, command.SessionId, command.CommandId),
            Context.ConnectionAborted
        );
        await PublishSessionAsync(session);
        return SessionCommandAcknowledgement.Approve(session);
    }

    public async Task<SessionCommandAcknowledgement> ChangeSessionHero(
        ChangeSessionHeroRequest command
    )
    {
        Guid playerId = await ResolvePlayerIdAsync();
        GameSessionSnapshot session = await sender.Send<GameSessionSnapshot>(
            new ChangeSessionHeroCommand(
                playerId,
                command.SessionId,
                command.HeroId,
                command.CommandId
            ),
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

    private Task<Guid> ResolvePlayerIdAsync() =>
        currentPlayer.ResolveAsync(
            Context.Features?.Get<IHttpContextFeature>()?.HttpContext,
            Context.ConnectionAborted
        );

    internal static string GroupName(Guid sessionId) => $"session:{sessionId}";
}

public sealed record CreateSoloLobbyRequest(Guid CommandId, Guid HeroId, string? DungeonSeed);

public sealed record StartSessionRequest(Guid CommandId, Guid SessionId);

public sealed record ChangeSessionHeroRequest(Guid CommandId, Guid SessionId, Guid HeroId);

public sealed record SessionSnapshotRequest(Guid SessionId);

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
