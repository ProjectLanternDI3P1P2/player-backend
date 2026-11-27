using FluentAssertions;
using Moq;
using Player.Application.Features.HeroUseCase;
using Player.Application.Features.HeroUseCase.GetCombatantSnapshot;
using Player.Domain.Entities;
using Player.Domain.Repositories;
using Serilog;

namespace Player.Test.Features.HeroUseCase;

public sealed class GetCombatantSnapshotQueryHandlerTests
{
    [Fact]
    public async Task Handle_KnownWarrior_ReturnsStatsAbilitiesAndAppearance()
    {
        Hero hero = Warrior();
        GetCombatantSnapshotQueryHandler handler = HandlerReturning(hero);

        CombatantSnapshot? snapshot = await handler.Handle(
            new GetCombatantSnapshotQuery(hero.Id),
            TestContext.Current.CancellationToken
        );

        snapshot.Should().NotBeNull();
        snapshot!.HeroId.Should().Be(hero.Id);
        snapshot.Name.Should().Be("Aldric");
        snapshot.ClassCode.Should().Be("warrior");
        snapshot.Level.Should().Be(2);
        snapshot.MaxHp.Should().Be(156);
        snapshot.MaxMana.Should().Be(21);
        snapshot.Attack.Should().Be(18);
        snapshot.Defense.Should().Be(0);
        snapshot.Speed.Should().Be(9);
        snapshot.Appearance.Should().Be(new HeroAppearance(4, "bob1", 3));
    }

    [Fact]
    public async Task Handle_Abilities_KeepsActiveUnlockedOnesOrderedByCode()
    {
        Hero hero = Warrior();
        GetCombatantSnapshotQueryHandler handler = HandlerReturning(hero);

        CombatantSnapshot? snapshot = await handler.Handle(
            new GetCombatantSnapshotQuery(hero.Id),
            TestContext.Current.CancellationToken
        );

        snapshot!
            .Abilities.Should()
            .Equal(
                new CombatantAbilitySnapshot("warrior-guard", "Guard", 3, "Self", 100),
                new CombatantAbilitySnapshot("warrior-strike", "Strike", 4, "SingleEnemy", 150)
            );
    }

    [Fact]
    public async Task Handle_BaldHero_HasNoHairColor()
    {
        Hero hero = Warrior();
        hero.HairStyle = "bald";
        hero.HairColor = null;
        GetCombatantSnapshotQueryHandler handler = HandlerReturning(hero);

        CombatantSnapshot? snapshot = await handler.Handle(
            new GetCombatantSnapshotQuery(hero.Id),
            TestContext.Current.CancellationToken
        );

        snapshot!.Appearance.HairColor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UnknownHero_ReturnsNull()
    {
        GetCombatantSnapshotQueryHandler handler = HandlerReturning(null);

        CombatantSnapshot? snapshot = await handler.Handle(
            new GetCombatantSnapshotQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        snapshot.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeletedHero_ReturnsNull()
    {
        Hero hero = Warrior();
        hero.IsDeleted = true;
        GetCombatantSnapshotQueryHandler handler = HandlerReturning(hero);

        CombatantSnapshot? snapshot = await handler.Handle(
            new GetCombatantSnapshotQuery(hero.Id),
            TestContext.Current.CancellationToken
        );

        snapshot.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CalledTwice_ReturnsTheSameSnapshot()
    {
        Hero hero = Warrior();
        GetCombatantSnapshotQueryHandler handler = HandlerReturning(hero);

        CombatantSnapshot? first = await handler.Handle(
            new GetCombatantSnapshotQuery(hero.Id),
            TestContext.Current.CancellationToken
        );
        CombatantSnapshot? second = await handler.Handle(
            new GetCombatantSnapshotQuery(hero.Id),
            TestContext.Current.CancellationToken
        );

        second.Should().BeEquivalentTo(first);
    }

    private static GetCombatantSnapshotQueryHandler HandlerReturning(Hero? hero)
    {
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.GetCombatantByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hero);
        return new GetCombatantSnapshotQueryHandler(repository.Object, new Mock<ILogger>().Object);
    }

    private static Hero Warrior()
    {
        Guid heroId = Guid.NewGuid();
        var heroClass = new HeroClass
        {
            Code = "warrior",
            BaseHealth = 60,
            BaseMana = 0,
        };
        return new Hero
        {
            Id = heroId,
            Name = "Aldric",
            ClassCode = "warrior",
            Level = 2,
            Strength = 18,
            Endurance = 16,
            Agility = 9,
            Intelligence = 7,
            SkinTone = 4,
            HairStyle = "bob1",
            HairColor = 3,
            HeroClass = heroClass,
            HeroSkills =
            [
                Skill(heroId, "warrior-strike", "Strike", 1, "SingleEnemy", 4, 150, active: true),
                Skill(heroId, "warrior-guard", "Guard", 2, "Self", 3, 100, active: true),
                Skill(heroId, "warrior-rage", "Rage", 5, "Self", 8, 120, active: true),
                Skill(heroId, "warrior-old", "Old", 1, "SingleEnemy", 1, 100, active: false),
            ],
        };
    }

    private static HeroSkill Skill(
        Guid heroId,
        string code,
        string label,
        int requiredLevel,
        string targetingType,
        int manaCost,
        int power,
        bool active
    ) =>
        new()
        {
            HeroId = heroId,
            SkillCode = code,
            IsActive = active,
            Skill = new Skill
            {
                Code = code,
                Label = label,
                RequiredLevel = requiredLevel,
                TargetingType = targetingType,
                ManaCost = manaCost,
                Power = power,
            },
        };
}
