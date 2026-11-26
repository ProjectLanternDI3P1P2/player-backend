namespace Player.Application.Features.HeroUseCase.GetCombatantSnapshot;

/// <summary>Immutable picture of a hero at the moment a fight opens.</summary>
public sealed record CombatantSnapshot(
    Guid HeroId,
    string Name,
    string ClassCode,
    int Level,
    int MaxHp,
    int MaxMana,
    int Attack,
    int Defense,
    int Speed,
    IReadOnlyList<CombatantAbilitySnapshot> Abilities,
    HeroAppearance Appearance
);

public sealed record CombatantAbilitySnapshot(
    string Code,
    string Name,
    int ManaCost,
    string TargetType,
    int Power
);
