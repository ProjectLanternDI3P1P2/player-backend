using FluentAssertions;
using Moq;
using Player.Application.Features.HeroUseCase;
using Player.Application.Features.HeroUseCase.CreateHero;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Features.HeroUseCase;

public sealed class CreateHeroCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidMage_CreatesLevelOneHeroWithItsFirstSkillAndHealth()
    {
        Guid playerId = Guid.NewGuid();
        var repository = CreateRepository(playerId, "mage", 45, "mage-bolt");
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        CreateHeroResult result = await handler.Handle(
            new CreateHeroCommand(playerId, "Merlin", "Mage", Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result
            .Should()
            .BeEquivalentTo(
                new
                {
                    Name = "Merlin",
                    ClassCode = "mage",
                    Level = 1,
                    MaximumHealth = 45,
                    UnlockedSkillCodes = new[] { "mage-bolt" },
                    AlreadyExists = false,
                }
            );
        repository.Verify(x => x.AddHero(It.Is<Hero>(hero => hero.Endurance == 0)), Times.Once);
        repository.Verify(x => x.AddIdempotencyKey(It.IsAny<IdempotencyKey>()), Times.Once);
    }

    [Theory]
    [InlineData("warrior", 60, "warrior-strike")]
    [InlineData("shaman", 50, "shaman-totem")]
    [InlineData("mage", 45, "mage-bolt")]
    public async Task Handle_EachAllowedClass_AppliesItsConfiguredBaseHealth(
        string classCode,
        int baseHealth,
        string skillCode
    )
    {
        Guid playerId = Guid.NewGuid();
        var repository = CreateRepository(playerId, classCode, baseHealth, skillCode);
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        CreateHeroResult result = await handler.Handle(
            new CreateHeroCommand(playerId, "Aster", classCode, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.MaximumHealth.Should().Be(baseHealth);
        result.UnlockedSkillCodes.Should().ContainSingle().Which.Should().Be(skillCode);
    }

    [Fact]
    public async Task Handle_TenActiveHeroes_RejectsCreation()
    {
        Guid playerId = Guid.NewGuid();
        var repository = CreateRepository(playerId, "warrior", 60, "warrior-strike", heroCount: 10);
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        Func<Task> action = () =>
            handler.Handle(
                new CreateHeroCommand(playerId, "Conan", "warrior", Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<ConflictException>();
        repository.Verify(x => x.AddHero(It.IsAny<Hero>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateName_RejectsCreation()
    {
        Guid playerId = Guid.NewGuid();
        var repository = CreateRepository(
            playerId,
            "warrior",
            60,
            "warrior-strike",
            nameExists: true
        );
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        Func<Task> action = () =>
            handler.Handle(
                new CreateHeroCommand(playerId, "Conan", "warrior", Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_InvalidName_RejectsCreation()
    {
        var handler = new CreateHeroCommandHandler(Mock.Of<IHeroRepository>(), new FixedClock());

        Func<Task> action = () =>
            handler.Handle(
                new CreateHeroCommand(Guid.NewGuid(), "12", "mage", Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_PreviouslyProcessedRequest_ReturnsTheExistingHero()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid idempotencyKey = Guid.NewGuid();
        const string name = "Merlin";
        const string classCode = "mage";
        var hero = new Hero
        {
            Id = heroId,
            Name = name,
            ClassCode = classCode,
            Level = 1,
            HeroClass = new HeroClass { Code = classCode, BaseHealth = 45 },
            HeroSkills = [new HeroSkill { SkillCode = "mage-bolt", IsActive = true }],
        };
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.GetIdempotencyKeyAsync(idempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new IdempotencyKey
                {
                    Key = idempotencyKey,
                    PlayerId = playerId,
                    Scope = "hero.create",
                    RequestFingerprint = Fingerprint(name, classCode),
                    ProducedResourceId = heroId,
                }
            );
        repository
            .Setup(x => x.GetHeroByIdAsync(heroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hero);
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        CreateHeroResult result = await handler.Handle(
            new CreateHeroCommand(playerId, name, classCode, idempotencyKey),
            TestContext.Current.CancellationToken
        );

        result.Id.Should().Be(heroId);
        result.AlreadyExists.Should().BeTrue();
        repository.Verify(x => x.AddHero(It.IsAny<Hero>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAppearance_StoresTheChosenSkinAndHair()
    {
        Guid playerId = Guid.NewGuid();
        var repository = CreateRepository(playerId, "mage", 45, "mage-bolt");
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        CreateHeroResult result = await handler.Handle(
            new CreateHeroCommand(
                playerId,
                "Merlin",
                "mage",
                Guid.NewGuid(),
                new HeroAppearance(7, "DAP1", 12)
            ),
            TestContext.Current.CancellationToken
        );

        result.Appearance.Should().Be(new HeroAppearance(7, "dap1", 12));
        repository.Verify(
            x =>
                x.AddHero(
                    It.Is<Hero>(hero =>
                        hero.SkinTone == 7 && hero.HairStyle == "dap1" && hero.HairColor == 12
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_BaldHero_StoresNoHairColor()
    {
        Guid playerId = Guid.NewGuid();
        var repository = CreateRepository(playerId, "warrior", 60, "warrior-strike");
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        CreateHeroResult result = await handler.Handle(
            new CreateHeroCommand(
                playerId,
                "Conan",
                "warrior",
                Guid.NewGuid(),
                new HeroAppearance(3, "bald", null)
            ),
            TestContext.Current.CancellationToken
        );

        result.Appearance.Should().Be(new HeroAppearance(3, "bald", null));
    }

    [Fact]
    public async Task Handle_WithoutAppearance_UsesTheDefaultAppearance()
    {
        Guid playerId = Guid.NewGuid();
        var repository = CreateRepository(playerId, "shaman", 50, "shaman-totem");
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        CreateHeroResult result = await handler.Handle(
            new CreateHeroCommand(playerId, "Aster", "shaman", Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.Appearance.Should().Be(HeroAppearance.Default);
    }

    [Theory]
    [InlineData(0, "bob1", 1)]
    [InlineData(11, "bob1", 1)]
    [InlineData(1, "mohawk", 1)]
    [InlineData(1, "bob1", 14)]
    [InlineData(1, "bob1", null)]
    [InlineData(1, "bald", 2)]
    public async Task Handle_UnsupportedAppearance_RejectsCreation(
        int skinTone,
        string hairStyle,
        int? hairColor
    )
    {
        Guid playerId = Guid.NewGuid();
        var repository = CreateRepository(playerId, "mage", 45, "mage-bolt");
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        Func<Task> action = () =>
            handler.Handle(
                new CreateHeroCommand(
                    playerId,
                    "Merlin",
                    "mage",
                    Guid.NewGuid(),
                    new HeroAppearance(skinTone, hairStyle, hairColor)
                ),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<BadRequestException>();
        repository.Verify(x => x.AddHero(It.IsAny<Hero>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReplayWithAnotherAppearance_RejectsTheKey()
    {
        Guid playerId = Guid.NewGuid();
        Guid idempotencyKey = Guid.NewGuid();
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.GetIdempotencyKeyAsync(idempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new IdempotencyKey
                {
                    Key = idempotencyKey,
                    PlayerId = playerId,
                    Scope = "hero.create",
                    RequestFingerprint = Fingerprint("Merlin", "mage"),
                    ProducedResourceId = Guid.NewGuid(),
                }
            );
        var handler = new CreateHeroCommandHandler(repository.Object, new FixedClock());

        Func<Task> action = () =>
            handler.Handle(
                new CreateHeroCommand(
                    playerId,
                    "Merlin",
                    "mage",
                    idempotencyKey,
                    new HeroAppearance(4, "bald", null)
                ),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<ConflictException>();
    }

    private static Mock<IHeroRepository> CreateRepository(
        Guid playerId,
        string classCode,
        int baseHealth,
        string skillCode,
        int heroCount = 0,
        bool nameExists = false
    )
    {
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.GetIdempotencyKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyKey?)null);
        repository
            .Setup(x => x.GetPlayerForUpdateAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlayerEntity { Id = playerId });
        repository
            .Setup(x => x.CountActiveHeroesAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(heroCount);
        repository
            .Setup(x =>
                x.HeroNameExistsAsync(playerId, It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(nameExists);
        repository
            .Setup(x => x.GetHeroClassAsync(classCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroClass { Code = classCode, BaseHealth = baseHealth });
        repository
            .Setup(x => x.GetFirstSkillAsync(classCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Skill
                {
                    Code = skillCode,
                    ClassCode = classCode,
                    RequiredLevel = 1,
                }
            );
        return repository;
    }

    private static string Fingerprint(string name, string classCode) =>
        Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"{name}\n{classCode}\n1\nbob1\n1")
            )
        );

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);
    }
}
