using MediatR;

namespace Player.Application.Features.HeroUseCase.GetHeroSheet;

public sealed record GetHeroSheetQuery(Guid PlayerId, Guid HeroId) : IRequest<HeroSheet?>;
