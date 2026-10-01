using MediatR;

namespace Player.Application.Features.GameSessionUseCase.ChangeSessionHero;

public sealed record ChangeSessionHeroCommand(
    Guid PlayerId,
    Guid SessionId,
    Guid HeroId,
    Guid IdempotencyKey
) : IRequest<GameSessionSnapshot>;
