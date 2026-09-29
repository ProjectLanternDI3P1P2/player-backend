using Player.Domain.Entities;

namespace Player.Domain.Repositories;

public interface IGameSessionRepository
{
    Task<IdempotencyKey?> GetIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken);
    Task<GameSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<Hero?> GetActiveHeroForUpdateAsync(
        Guid heroId,
        Guid playerId,
        CancellationToken cancellationToken
    );
    Task<bool> HasActiveSessionAsync(Guid heroId, CancellationToken cancellationToken);
    void Add(GameSession session);
    void AddIdempotencyKey(IdempotencyKey idempotencyKey);
}
