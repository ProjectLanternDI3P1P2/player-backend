namespace Player.Application.Features.HeroUseCase.GetHeroSheet;

public sealed record HeroSheet(
    Guid Id,
    string Name,
    string ClassCode,
    int Level,
    HeroAttributes Attributes,
    int MaximumHealth,
    IReadOnlyList<HeroAbility> Abilities
);

public sealed record HeroAttributes(int Strength, int Endurance, int Agility, int Intelligence);

public sealed record HeroAbility(string Code, string Label, string TargetingType);
