using FluentAssertions;
using Moq;
using Player.Application.Features.HeroUseCase.ListHeroes;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Test.Features.HeroUseCase;

public sealed class ListHeroesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsHealthAndActiveSessionEngagementForEachHero()
    {
        Guid playerId = Guid.NewGuid();
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.ListActiveByPlayerIdAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateHero("Aldric", 60, 2, false), CreateHero("Brom", 50, 0, true)]);
        repository
            .Setup(x => x.GetSelectedHeroIdAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);
        var handler = new ListHeroesQueryHandler(repository.Object);

        IReadOnlyList<HeroSummary> result = await handler.Handle(
            new ListHeroesQuery(playerId),
            TestContext.Current.CancellationToken
        );

        result
            .Should()
            .BeEquivalentTo(
                [
                    new HeroSummary(
                        result[0].Id,
                        "Aldric",
                        "warrior",
                        1,
                        72,
                        false,
                        new DateTimeOffset(2026, 9, 28, 12, 0, 0, TimeSpan.Zero),
                        false
                    ),
                    new HeroSummary(
                        result[1].Id,
                        "Brom",
                        "shaman",
                        1,
                        50,
                        true,
                        new DateTimeOffset(2026, 9, 28, 12, 0, 0, TimeSpan.Zero),
                        false
                    ),
                ],
                options => options.WithStrictOrdering()
            );
    }

    private static Hero CreateHero(string name, int baseHealth, int endurance, bool engaged) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            ClassCode = baseHealth == 60 ? "warrior" : "shaman",
            Level = 1,
            CreatedAt = new DateTimeOffset(2026, 9, 28, 12, 0, 0, TimeSpan.Zero),
            Endurance = endurance,
            HeroClass = new HeroClass { BaseHealth = baseHealth },
            SessionMembers = engaged
                ? [new GameSessionMember { GameSession = new GameSession { Status = "Started" } }]
                : [],
        };
}
