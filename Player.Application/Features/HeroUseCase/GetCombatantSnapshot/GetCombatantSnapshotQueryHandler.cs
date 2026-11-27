using MediatR;
using Player.Domain.Entities;
using Player.Domain.Repositories;
using Player.Domain.Services;
using ILogger = Serilog.ILogger;

namespace Player.Application.Features.HeroUseCase.GetCombatantSnapshot;

/// <summary>
/// Builds the combatant snapshot of a hero, or returns null when the hero is unknown or
/// deleted: a deleted hero can no longer fight. Pure read, no side effect.
/// </summary>
public sealed class GetCombatantSnapshotQueryHandler(IHeroRepository repository, ILogger logger)
    : IRequestHandler<GetCombatantSnapshotQuery, CombatantSnapshot?>
{
    public async Task<CombatantSnapshot?> Handle(
        GetCombatantSnapshotQuery request,
        CancellationToken cancellationToken
    )
    {
        Hero? hero = await repository.GetCombatantByIdAsync(request.HeroId, cancellationToken);
        if (hero is null || hero.IsDeleted)
        {
            logger.Information(
                "Combatant snapshot requested for unknown or deleted hero {HeroId}.",
                request.HeroId
            );
            return null;
        }

        CombatantStats stats = CombatantStatsCalculator.Calculate(hero, hero.HeroClass);
        List<CombatantAbilitySnapshot> abilities = hero
            .HeroSkills.Where(heroSkill =>
                heroSkill.IsActive && heroSkill.Skill.RequiredLevel <= hero.Level
            )
            .OrderBy(heroSkill => heroSkill.SkillCode, StringComparer.Ordinal)
            .Select(heroSkill => new CombatantAbilitySnapshot(
                heroSkill.SkillCode,
                heroSkill.Skill.Label,
                Math.Max(0, heroSkill.Skill.ManaCost),
                heroSkill.Skill.TargetingType,
                Math.Max(0, heroSkill.Skill.Power)
            ))
            .ToList();

        return new CombatantSnapshot(
            hero.Id,
            hero.Name,
            hero.ClassCode,
            hero.Level,
            stats.MaxHp,
            stats.MaxMana,
            stats.Attack,
            stats.Defense,
            stats.Speed,
            abilities,
            HeroAppearance.From(hero)
        );
    }
}
