using MediatR;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Application.Features.HeroUseCase.GetHeroSheet;

public sealed class GetHeroSheetQueryHandler(IHeroRepository repository)
    : IRequestHandler<GetHeroSheetQuery, HeroSheet?>
{
    public async Task<HeroSheet?> Handle(
        GetHeroSheetQuery request,
        CancellationToken cancellationToken
    )
    {
        Hero? hero = await repository.GetActiveByIdAndPlayerIdAsync(
            request.HeroId,
            request.PlayerId,
            cancellationToken
        );
        if (hero is null)
        {
            return null;
        }

        return new HeroSheet(
            hero.Id,
            hero.Name,
            hero.ClassCode,
            hero.Level,
            new HeroAttributes(hero.Strength, hero.Endurance, hero.Agility, hero.Intelligence),
            hero.HeroClass.BaseHealth + (hero.Endurance * 6),
            hero.HeroSkills.OrderBy(skill => skill.SkillCode)
                .Select(skill => new HeroAbility(
                    skill.SkillCode,
                    skill.Skill.Label,
                    skill.Skill.TargetingType
                ))
                .ToList()
        );
    }
}
