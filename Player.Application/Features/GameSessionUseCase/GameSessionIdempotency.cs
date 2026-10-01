using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;

namespace Player.Application.Features.GameSessionUseCase;

internal static class GameSessionIdempotency
{
    public static async Task<GameSessionSnapshot> GetExistingAsync(
        IGameSessionRepository repository,
        IdempotencyKey key,
        Guid playerId,
        string scope,
        string fingerprint,
        CancellationToken cancellationToken
    )
    {
        if (
            key.PlayerId != playerId
            || key.Scope != scope
            || key.RequestFingerprint != fingerprint
            || key.ProducedResourceId is null
        )
            throw new ConflictException(
                "This idempotency key was already used for a different request."
            );

        GameSession? session = await repository.GetByIdAsync(
            key.ProducedResourceId.Value,
            cancellationToken
        );
        if (session is null)
            throw new InvalidOperationException(
                "The idempotent session result is no longer available."
            );
        return GameSessionSnapshot.From(session, true);
    }
}
