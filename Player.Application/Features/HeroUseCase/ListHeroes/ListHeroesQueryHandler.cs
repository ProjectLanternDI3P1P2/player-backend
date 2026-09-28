using MediatR;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Application.Features.HeroUseCase.ListHeroes;

public sealed class ListHeroesQueryHandler(IHeroRepository repository)
    : IRequestHandler<ListHeroesQuery, IReadOnlyList<HeroSummary>>
{
    public async Task<IReadOnlyList<HeroSummary>> Handle(
        ListHeroesQuery request,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<Hero> heroes = await repository.ListActiveByPlayerIdAsync(
            request.PlayerId,
            cancellationToken
        );

        return heroes
            .Select(hero => new HeroSummary(
                hero.Id,
                hero.Name,
                hero.ClassCode,
                hero.Level,
                hero.HeroClass.BaseHealth + (hero.Endurance * 6),
                hero.SessionMembers.Any(member =>
                    member.LeftAt is null && member.GameSession.EndedAt is null
                )
            ))
            .ToList();
    }
}
