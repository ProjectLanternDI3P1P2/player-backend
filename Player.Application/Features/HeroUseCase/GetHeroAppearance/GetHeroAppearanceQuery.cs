using MediatR;

namespace Player.Application.Features.HeroUseCase.GetHeroAppearance;

/// <summary>Service-to-service read of a hero's appearance, whoever owns the hero.</summary>
public sealed record GetHeroAppearanceQuery(Guid HeroId) : IRequest<HeroAppearance?>;
