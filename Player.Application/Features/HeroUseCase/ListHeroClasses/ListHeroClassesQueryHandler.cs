using MediatR;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Application.Features.HeroUseCase.ListHeroClasses;

public sealed class ListHeroClassesQueryHandler(IHeroRepository repository)
    : IRequestHandler<ListHeroClassesQuery, IReadOnlyList<HeroClassSummary>>
{
    public async Task<IReadOnlyList<HeroClassSummary>> Handle(
        ListHeroClassesQuery request,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<HeroClass> heroClasses = await repository.ListHeroClassesAsync(
            cancellationToken
        );

        return heroClasses
            .Select(heroClass => new HeroClassSummary(
                heroClass.Code,
                heroClass.Label,
                heroClass.Description,
                heroClass.BaseHealth
            ))
            .ToList();
    }
}
