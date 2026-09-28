namespace Player.Application.Features.HeroUseCase.CreateHero;

public sealed record CreateHeroResult(
    Guid Id,
    string Name,
    string ClassCode,
    int Level,
    int MaximumHealth,
    IReadOnlyList<string> UnlockedSkillCodes,
    bool AlreadyExists
);
