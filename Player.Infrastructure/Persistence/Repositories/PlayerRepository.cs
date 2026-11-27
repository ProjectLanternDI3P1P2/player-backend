using Microsoft.EntityFrameworkCore;
using Player.Domain.Enums;
using Player.Domain.Repositories;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence.Repositories;

public sealed class PlayerRepository(PlayerDbContext dbContext) : IPlayerRepository
{
    public Task<PlayerEntity?> GetWithActiveHeroesByIdAsync(
        Guid playerId,
        CancellationToken cancellationToken
    ) =>
        WithActiveHeroes().SingleOrDefaultAsync(player => player.Id == playerId, cancellationToken);

    public Task<PlayerEntity?> GetWithActiveHeroesByPublicHandleAsync(
        string publicHandle,
        CancellationToken cancellationToken
    ) =>
        WithActiveHeroes()
            .SingleOrDefaultAsync(player => player.PublicHandle == publicHandle, cancellationToken);

    public Task<int> CountCompletedSessionsAsync(
        Guid playerId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .GameSessions.AsNoTracking()
            .CountAsync(
                session =>
                    session.Status == GameSessionStatus.Completed
                    && session.Members.Any(member => member.Hero.PlayerId == playerId),
                cancellationToken
            );

    private IQueryable<PlayerEntity> WithActiveHeroes() =>
        dbContext
            .Players.AsNoTracking()
            .Include(player => player.Heroes.Where(hero => !hero.IsDeleted))
                .ThenInclude(hero => hero.HeroClass);
}
