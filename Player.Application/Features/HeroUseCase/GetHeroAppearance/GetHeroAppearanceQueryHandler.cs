using MediatR;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Application.Features.HeroUseCase.GetHeroAppearance;

/// <summary>
/// Returns the appearance stored on the hero, or null when the hero is unknown. A deleted hero
/// keeps its appearance so that the run of a finished session can still be displayed.
/// </summary>
public sealed class GetHeroAppearanceQueryHandler(IHeroRepository repository)
    : IRequestHandler<GetHeroAppearanceQuery, HeroAppearance?>
{
    public async Task<HeroAppearance?> Handle(
        GetHeroAppearanceQuery request,
        CancellationToken cancellationToken
    )
    {
        Hero? hero = await repository.GetHeroByIdAsync(request.HeroId, cancellationToken);
        return hero is null ? null : HeroAppearance.From(hero);
    }
}
