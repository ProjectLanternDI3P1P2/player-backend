using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;

namespace Player.Application.Features.GameSessionUseCase;

internal static class LobbyAccess
{
    public static async Task<GameSession> GetCreatorLobbyForUpdateAsync(
        IGameSessionRepository repository,
        Guid sessionId,
        Guid playerId,
        string creatorRejection,
        string stateRejection,
        CancellationToken cancellationToken
    )
    {
        GameSession? session = await repository.GetByIdForUpdateAsync(sessionId, cancellationToken);
        if (session is null)
            throw new KeyNotFoundException($"Session '{sessionId}' was not found.");
        if (session.CreatorPlayerId != playerId)
            throw new ConflictException(creatorRejection);
        if (session.Status != "Lobby")
            throw new ConflictException(stateRejection);
        return session;
    }
}
