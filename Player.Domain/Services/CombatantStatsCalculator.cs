using Player.Domain.Entities;

namespace Player.Domain.Services;

/// <summary>
/// Turns a hero's attributes into the statistics Combat fights with. Kept in one place
/// because the balance is expected to change; the formulas are documented in
/// docs/player/combatant_snapshot.en.md.
/// </summary>
public static class CombatantStatsCalculator
{
    public const int HealthPerEndurance = 6;
    public const int ManaPerIntelligence = 3;

    /// <summary>Classes whose attack scales with Intelligence instead of Strength.</summary>
    private static readonly HashSet<string> IntelligenceClasses = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "mage",
        "shaman",
    };

    public static CombatantStats Calculate(Hero hero, HeroClass heroClass)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(heroClass);

        int maxHp = Math.Max(1, heroClass.BaseHealth + (hero.Endurance * HealthPerEndurance));
        int maxMana = Math.Max(0, heroClass.BaseMana + (hero.Intelligence * ManaPerIntelligence));
        int attack = IntelligenceClasses.Contains(hero.ClassCode)
            ? hero.Intelligence
            : hero.Strength;

        return new CombatantStats(
            maxHp,
            maxMana,
            Math.Max(0, attack),
            Math.Max(0, hero.Endurance / 2),
            Math.Max(0, hero.Agility)
        );
    }
}

public sealed record CombatantStats(int MaxHp, int MaxMana, int Attack, int Defense, int Speed);
