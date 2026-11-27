using FluentAssertions;
using Moq;
using Player.Application.Features.PlayerUseCase.GetPublicProfile;
using Player.Domain.Entities;
using Player.Domain.Repositories;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Features.PlayerUseCase;

public sealed class GetPublicProfileQueryHandlerTests
{
    private static readonly Guid PlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string PublicHandle = "maelisrune-00000000";

    [Fact]
    public async Task Handle_ActivePlayerById_ReturnsProfileWithSortedActiveHeroesAndCounters()
    {
        PlayerEntity player = CreatePlayer(
            heroes:
            [
                CreateHero("Brom", "shaman", "Shaman", 3),
                CreateHero("Aldric", "warrior", "Warrior", 7),
                CreateHero("Cara", "mage", "Mage", 3),
                CreateHero("Ghost", "warrior", "Warrior", 99, isDeleted: true),
            ]
        );
        var repository = new Mock<IPlayerRepository>();
        repository
            .Setup(x => x.GetWithActiveHeroesByIdAsync(PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
        repository
            .Setup(x => x.CountCompletedSessionsAsync(PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);
        var handler = new GetPublicProfileQueryHandler(repository.Object);

        PublicProfile? result = await handler.Handle(
            GetPublicProfileQuery.ById(PlayerId),
            TestContext.Current.CancellationToken
        );

        result
            .Should()
            .BeEquivalentTo(
                new PublicProfile(
                    PlayerId,
                    PublicHandle,
                    "MaelisRune",
                    2,
                    [
                        new PublicProfileHero("Aldric", "warrior", "Warrior", 7),
                        new PublicProfileHero("Brom", "shaman", "Shaman", 3),
                        new PublicProfileHero("Cara", "mage", "Mage", 3),
                    ],
                    new PublicProfileCounters(3, 7, 4)
                ),
                options => options.WithStrictOrdering()
            );
    }

    [Fact]
    public async Task Handle_MixedCaseHandle_LooksUpTheLowercaseHandle()
    {
        var repository = new Mock<IPlayerRepository>();
        repository
            .Setup(x =>
                x.GetWithActiveHeroesByPublicHandleAsync(
                    PublicHandle,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(CreatePlayer());
        var handler = new GetPublicProfileQueryHandler(repository.Object);

        PublicProfile? result = await handler.Handle(
            GetPublicProfileQuery.ByPublicHandle("MaelisRune-00000000"),
            TestContext.Current.CancellationToken
        );

        result.Should().NotBeNull();
        result!.PublicHandle.Should().Be("maelisrune-00000000");
        repository.Verify(
            x =>
                x.GetWithActiveHeroesByPublicHandleAsync(
                    PublicHandle,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("bad handle!")]
    [InlineData("")]
    [InlineData("a-handle-that-is-much-longer-than-32-chars")]
    public async Task Handle_MalformedHandle_ReturnsNullWithoutLookup(string handle)
    {
        var repository = new Mock<IPlayerRepository>();
        var handler = new GetPublicProfileQueryHandler(repository.Object);

        PublicProfile? result = await handler.Handle(
            GetPublicProfileQuery.ByPublicHandle(handle),
            TestContext.Current.CancellationToken
        );

        result.Should().BeNull();
        repository.Verify(
            x =>
                x.GetWithActiveHeroesByPublicHandleAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_UnknownPlayer_ReturnsNull()
    {
        var repository = new Mock<IPlayerRepository>();
        var handler = new GetPublicProfileQueryHandler(repository.Object);

        PublicProfile? result = await handler.Handle(
            GetPublicProfileQuery.ById(Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AnonymizedPlayer_ReturnsNull()
    {
        var repository = new Mock<IPlayerRepository>();
        repository
            .Setup(x => x.GetWithActiveHeroesByIdAsync(PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                CreatePlayer(anonymizedAt: new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero))
            );
        var handler = new GetPublicProfileQueryHandler(repository.Object);

        PublicProfile? result = await handler.Handle(
            GetPublicProfileQuery.ById(PlayerId),
            TestContext.Current.CancellationToken
        );

        result.Should().BeNull();
        repository.Verify(
            x => x.CountCompletedSessionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_AccountNotActive_ReturnsNull()
    {
        var repository = new Mock<IPlayerRepository>();
        repository
            .Setup(x => x.GetWithActiveHeroesByIdAsync(PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePlayer(accountStatus: "Suspended"));
        var handler = new GetPublicProfileQueryHandler(repository.Object);

        PublicProfile? result = await handler.Handle(
            GetPublicProfileQuery.ById(PlayerId),
            TestContext.Current.CancellationToken
        );

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NoActiveHero_ReturnsEmptyHeroesAndZeroCounters()
    {
        PlayerEntity player = CreatePlayer(
            heroes: [CreateHero("Ghost", "warrior", "Warrior", 5, isDeleted: true)]
        );
        var repository = new Mock<IPlayerRepository>();
        repository
            .Setup(x => x.GetWithActiveHeroesByIdAsync(PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
        var handler = new GetPublicProfileQueryHandler(repository.Object);

        PublicProfile? result = await handler.Handle(
            GetPublicProfileQuery.ById(PlayerId),
            TestContext.Current.CancellationToken
        );

        result.Should().NotBeNull();
        result!.Heroes.Should().BeEmpty();
        result.Counters.Should().BeEquivalentTo(new PublicProfileCounters(0, 0, 0));
    }

    private static PlayerEntity CreatePlayer(
        string accountStatus = "Active",
        DateTimeOffset? anonymizedAt = null,
        IReadOnlyList<Hero>? heroes = null
    ) =>
        new()
        {
            Id = PlayerId,
            ExternalUserId = "external-user",
            DisplayName = "MaelisRune",
            PublicHandle = PublicHandle,
            AccountStatus = accountStatus,
            NewGamePlusLevel = 2,
            AnonymizedAt = anonymizedAt,
            Heroes = (heroes ?? []).ToList(),
        };

    private static Hero CreateHero(
        string name,
        string classCode,
        string classLabel,
        int level,
        bool isDeleted = false
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            PlayerId = PlayerId,
            Name = name,
            ClassCode = classCode,
            Level = level,
            IsDeleted = isDeleted,
            HeroClass = new HeroClass { Code = classCode, Label = classLabel },
        };
}
