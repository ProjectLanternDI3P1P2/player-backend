using FluentAssertions;
using Moq;
using Player.Application.Features.HeroUseCase.GetHeroSheet;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Test.Features.HeroUseCase;

public sealed class GetHeroSheetQueryHandlerTests
{
    [Fact]
    public async Task Handle_OwnedHero_ReturnsItsAttributesHealthAndOrderedAbilities()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x =>
                x.GetActiveByIdAndPlayerIdAsync(heroId, playerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new Hero
                {
                    Id = heroId,
                    Name = "Aldric",
                    ClassCode = "warrior",
                    Level = 4,
                    Strength = 10,
                    Endurance = 3,
                    Agility = 4,
                    Intelligence = 2,
                    HeroClass = new HeroClass { BaseHealth = 60 },
                    HeroSkills =
                    [
                        new HeroSkill
                        {
                            SkillCode = "warrior-strike",
                            Skill = new Skill { Label = "Strike", TargetingType = "SingleEnemy" },
                        },
                        new HeroSkill
                        {
                            SkillCode = "warrior-guard",
                            Skill = new Skill { Label = "Guard", TargetingType = "Self" },
                        },
                    ],
                }
            );
        var handler = new GetHeroSheetQueryHandler(repository.Object);

        HeroSheet? result = await handler.Handle(
            new GetHeroSheetQuery(playerId, heroId),
            TestContext.Current.CancellationToken
        );

        result
            .Should()
            .BeEquivalentTo(
                new HeroSheet(
                    heroId,
                    "Aldric",
                    "warrior",
                    4,
                    new HeroAttributes(10, 3, 4, 2),
                    78,
                    [
                        new HeroAbility("warrior-guard", "Guard", "Self"),
                        new HeroAbility("warrior-strike", "Strike", "SingleEnemy"),
                    ]
                )
            );
    }

    [Fact]
    public async Task Handle_HeroNotOwnedByPlayer_ReturnsNull()
    {
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x =>
                x.GetActiveByIdAndPlayerIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Hero?)null);
        var handler = new GetHeroSheetQueryHandler(repository.Object);

        HeroSheet? result = await handler.Handle(
            new GetHeroSheetQuery(Guid.NewGuid(), Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.Should().BeNull();
    }
}
