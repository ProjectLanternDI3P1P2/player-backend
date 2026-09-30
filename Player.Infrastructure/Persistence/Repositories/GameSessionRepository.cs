using Microsoft.EntityFrameworkCore;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Infrastructure.Persistence.Repositories;

public sealed class GameSessionRepository(PlayerDbContext dbContext) : IGameSessionRepository
{
    public Task<IdempotencyKey?> GetIdempotencyKeyAsync(
        Guid key,
        CancellationToken cancellationToken
    ) => dbContext.IdempotencyKeys.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);

    public Task<GameSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken) =>
        dbContext
            .GameSessions.Include(x => x.Members)
                .ThenInclude(x => x.Hero)
            .SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

    public Task<Hero?> GetActiveHeroForUpdateAsync(
        Guid heroId,
        Guid playerId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Heroes.FromSql(
                $"SELECT * FROM hero WHERE id = {heroId} AND player_id = {playerId} AND NOT is_deleted FOR UPDATE"
            )
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> HasActiveSessionAsync(Guid heroId, CancellationToken cancellationToken) =>
        dbContext.GameSessionMembers.AnyAsync(
            member =>
                member.HeroId == heroId
                && member.MemberStatus == "Active"
                && member.GameSession.Status == "Active",
            cancellationToken
        );

    public void Add(GameSession session) => dbContext.GameSessions.Add(session);

    public void AddIdempotencyKey(IdempotencyKey idempotencyKey) =>
        dbContext.IdempotencyKeys.Add(idempotencyKey);
}
