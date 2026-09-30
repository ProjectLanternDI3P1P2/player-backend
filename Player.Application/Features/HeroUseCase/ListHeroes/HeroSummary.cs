namespace Player.Application.Features.HeroUseCase.ListHeroes;

public sealed record HeroSummary(
    Guid Id,
    string Name,
    string ClassCode,
    int Level,
    int MaximumHealth,
    bool IsEngagedInActiveSession,
    DateTimeOffset CreatedAt
    bool IsSelected
);
