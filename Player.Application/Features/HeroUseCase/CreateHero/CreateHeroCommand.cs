using Player.Application.Abstractions;

namespace Player.Application.Features.HeroUseCase.CreateHero;

public sealed record CreateHeroCommand(
    Guid PlayerId,
    string Name,
    string ClassCode,
    Guid IdempotencyKey
) : ICommand<CreateHeroResult>;
