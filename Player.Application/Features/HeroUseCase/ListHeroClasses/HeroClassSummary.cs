namespace Player.Application.Features.HeroUseCase.ListHeroClasses;

public sealed record HeroClassSummary(
    string Code,
    string Label,
    string Description,
    int BaseHealth
);
