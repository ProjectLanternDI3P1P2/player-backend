using MediatR;

namespace Player.Application.Features.HeroUseCase.ListHeroes;

public sealed record ListHeroesQuery(Guid PlayerId) : IRequest<IReadOnlyList<HeroSummary>>;
