using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Application.Features.GameSessionUseCase;

internal sealed record LobbyCommandPreparation(
    GameSession? Lobby,
    GameSessionSnapshot? ExistingSnapshot
)
{
    public static async Task<LobbyCommandPreparation> CreateAsync(
        IGameSessionRepository repository,
        Guid commandId,
        Guid playerId,
        Guid sessionId,
        string scope,
        string fingerprint,
        string creatorRejection,
        string stateRejection,
        CancellationToken cancellationToken
    )
    {
        IdempotencyKey? existing = await repository.GetIdempotencyKeyAsync(
            commandId,
            cancellationToken
        );
        if (existing is not null)
            return new(
                null,
                await GameSessionIdempotency.GetExistingAsync(
                    repository,
                    existing,
                    playerId,
                    scope,
                    fingerprint,
                    cancellationToken
                )
            );

        GameSession lobby = await LobbyAccess.GetCreatorLobbyForUpdateAsync(
            repository,
            sessionId,
            playerId,
            creatorRejection,
            stateRejection,
            cancellationToken
        );
        existing = await repository.GetIdempotencyKeyAsync(commandId, cancellationToken);
        return existing is null
            ? new(lobby, null)
            : new(
                null,
                await GameSessionIdempotency.GetExistingAsync(
                    repository,
                    existing,
                    playerId,
                    scope,
                    fingerprint,
                    cancellationToken
                )
            );
    }
}
