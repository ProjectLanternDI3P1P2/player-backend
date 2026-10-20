using Player.Application.Abstractions;

namespace Player.Application.Features.HeroUseCase.CreateHero;

/// <param name="Appearance">Optional: the default appearance is used when omitted.</param>
public sealed record CreateHeroCommand(
    Guid PlayerId,
    string Name,
    string ClassCode,
    Guid IdempotencyKey,
    HeroAppearance? Appearance = null
) : ICommand<CreateHeroResult>;
