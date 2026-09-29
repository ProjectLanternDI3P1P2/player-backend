using Player.Domain.Entities;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Domain.Repositories;

public interface IHeroRepository
{
    Task<IReadOnlyList<HeroClass>> ListHeroClassesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Hero>> ListActiveByPlayerIdAsync(
        Guid playerId,
        CancellationToken cancellationToken
    );
    Task<IdempotencyKey?> GetIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken);
    Task<Hero?> GetHeroByIdAsync(Guid heroId, CancellationToken cancellationToken);
    Task<Hero?> GetActiveByIdAndPlayerIdAsync(
        Guid heroId,
        Guid playerId,
        CancellationToken cancellationToken
    );
    Task<PlayerEntity?> GetPlayerForUpdateAsync(Guid playerId, CancellationToken cancellationToken);
    Task<HeroClass?> GetHeroClassAsync(string classCode, CancellationToken cancellationToken);
    Task<Skill?> GetFirstSkillAsync(string classCode, CancellationToken cancellationToken);
    Task<int> CountActiveHeroesAsync(Guid playerId, CancellationToken cancellationToken);
    Task<bool> HeroNameExistsAsync(Guid playerId, string name, CancellationToken cancellationToken);
    void AddHero(Hero hero);
    void AddIdempotencyKey(IdempotencyKey idempotencyKey);
}
