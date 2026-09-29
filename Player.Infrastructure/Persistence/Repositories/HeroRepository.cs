using Microsoft.EntityFrameworkCore;
using Player.Domain.Entities;
using Player.Domain.Repositories;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence.Repositories;

public sealed class HeroRepository(PlayerDbContext dbContext) : IHeroRepository
{
    public async Task<IReadOnlyList<Hero>> ListActiveByPlayerIdAsync(
        Guid playerId,
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .Heroes.AsNoTracking()
            .Include(x => x.HeroClass)
            .Include(x => x.SessionMembers)
                .ThenInclude(x => x.GameSession)
            .Where(x => x.PlayerId == playerId && !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(10)
            .ToListAsync(cancellationToken);

    public Task<IdempotencyKey?> GetIdempotencyKeyAsync(
        Guid key,
        CancellationToken cancellationToken
    ) => dbContext.IdempotencyKeys.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);

    public Task<Hero?> GetHeroByIdAsync(Guid heroId, CancellationToken cancellationToken) =>
        dbContext
            .Heroes.Include(x => x.HeroClass)
            .Include(x => x.HeroSkills)
            .SingleOrDefaultAsync(x => x.Id == heroId, cancellationToken);

    public Task<Hero?> GetActiveByIdAndPlayerIdAsync(
        Guid heroId,
        Guid playerId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Heroes.AsNoTracking()
            .Include(x => x.HeroClass)
            .Include(x => x.HeroSkills)
                .ThenInclude(x => x.Skill)
            .Where(x => x.Id == heroId && x.PlayerId == playerId && !x.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<PlayerEntity?> GetPlayerForUpdateAsync(
        Guid playerId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Players.FromSql($"SELECT * FROM player WHERE id = {playerId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<HeroClass?> GetHeroClassAsync(
        string classCode,
        CancellationToken cancellationToken
    ) => dbContext.HeroClasses.SingleOrDefaultAsync(x => x.Code == classCode, cancellationToken);

    public Task<Skill?> GetFirstSkillAsync(string classCode, CancellationToken cancellationToken) =>
        dbContext
            .Skills.OrderBy(x => x.Code)
            .FirstOrDefaultAsync(
                x => x.ClassCode == classCode && x.RequiredLevel == 1,
                cancellationToken
            );

    public Task<int> CountActiveHeroesAsync(Guid playerId, CancellationToken cancellationToken) =>
        dbContext.Heroes.CountAsync(x => x.PlayerId == playerId && !x.IsDeleted, cancellationToken);

    public Task<bool> HeroNameExistsAsync(
        Guid playerId,
        string name,
        CancellationToken cancellationToken
    ) =>
        dbContext.Heroes.AnyAsync(
            x => x.PlayerId == playerId && !x.IsDeleted && x.Name.ToUpper() == name.ToUpper(),
            cancellationToken
        );

    public void AddHero(Hero hero) => dbContext.Heroes.Add(hero);

    public void AddIdempotencyKey(IdempotencyKey idempotencyKey) =>
        dbContext.IdempotencyKeys.Add(idempotencyKey);
}
