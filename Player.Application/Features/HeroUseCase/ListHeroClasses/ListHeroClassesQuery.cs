using MediatR;

namespace Player.Application.Features.HeroUseCase.ListHeroClasses;

public sealed record ListHeroClassesQuery : IRequest<IReadOnlyList<HeroClassSummary>>;
