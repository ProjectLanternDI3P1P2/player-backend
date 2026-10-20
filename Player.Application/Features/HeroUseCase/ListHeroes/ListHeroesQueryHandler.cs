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
        Guid? selectedHeroId = await repository.GetSelectedHeroIdAsync(
            request.PlayerId,
            cancellationToken
        );
        IReadOnlyList<Hero> heroes = await repository.ListActiveByPlayerIdAsync(
            request.PlayerId,
            cancellationToken
        );

        return heroes.Select(hero => ToSummary(hero, selectedHeroId)).ToList();
    }

    private static HeroSummary ToSummary(Hero hero, Guid? selectedHeroId)
    {
        // Lets the client tell a lobby (deletable after confirmation) from a running dungeon.
        GameSession? openSession = hero
            .SessionMembers.Where(member =>
                member.LeftAt is null && member.GameSession.EndedAt is null
            )
            .Select(member => member.GameSession)
            .OrderByDescending(session => session.Status == "Active")
            .FirstOrDefault();

        return new HeroSummary(
            hero.Id,
            hero.Name,
            hero.ClassCode,
            hero.Level,
            hero.HeroClass.BaseHealth + (hero.Endurance * 6),
            openSession is not null,
            hero.CreatedAt,
            hero.Id == selectedHeroId,
            openSession?.Id,
            openSession?.Status
        );
    }
}
