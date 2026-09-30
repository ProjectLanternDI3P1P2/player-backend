using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class ListHeroesIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task List_WhenPlayerHasHeroes_ReturnsStableSummariesAndSessionEngagement()
    {
        Guid playerId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedAsync(context, playerId));

        HttpResponseMessage response = await fixture.HttpClient.GetAsync(
            $"/api/v1/players/{playerId}/heroes",
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<HeroResponse>? heroes = await response.Content.ReadFromJsonAsync<List<HeroResponse>>(
            TestContext.Current.CancellationToken
        );
        heroes
            .Should()
            .BeEquivalentTo(
                [
                    new HeroResponse(
                        "Aldric",
                        "warrior",
                        3,
                        72,
                        false,
                        new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero),
                        true
                    ),
                    new HeroResponse(
                        "Brom",
                        "shaman",
                        1,
                        50,
                        true,
                        new(2026, 9, 28, 12, 1, 0, TimeSpan.Zero),
                        false
                    ),
                ],
                options => options.WithStrictOrdering()
            );
    }

    private static Task SeedAsync(PlayerDbContext context, Guid playerId)
    {
        DateTimeOffset now = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);
        var warrior = new HeroClass
        {
            Code = "warrior",
            Label = "Warrior",
            BaseHealth = 60,
        };
        var shaman = new HeroClass
        {
            Code = "shaman",
            Label = "Shaman",
            BaseHealth = 50,
        };
        var aldric = new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            ClassCode = warrior.Code,
            Name = "Aldric",
            Level = 3,
            Endurance = 2,
            CreatedAt = now,
        };
        var brom = new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            ClassCode = shaman.Code,
            Name = "Brom",
            Level = 1,
            CreatedAt = now.AddMinutes(1),
        };
        var deleted = new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            ClassCode = warrior.Code,
            Name = "Deleted",
            IsDeleted = true,
            CreatedAt = now.AddMinutes(2),
        };
        var activeSession = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = "Started",
            Mode = "Solo",
            Members =
            [
                new GameSessionMember
                {
                    HeroId = brom.Id,
                    MemberStatus = "Joined",
                    JoinedAt = now,
                },
            ],
        };

        context.AddRange(
            new PlayerEntity
            {
                Id = playerId,
                DisplayName = "test-player",
                AccountStatus = "Active",
                SelectedHeroId = aldric.Id,
                CreatedAt = now,
            },
            warrior,
            shaman,
            aldric,
            brom,
            deleted,
            activeSession
        );
        return Task.CompletedTask;
    }

    private sealed record HeroResponse(
        string Name,
        string ClassCode,
        int Level,
        int MaximumHealth,
        bool IsEngagedInActiveSession,
        DateTimeOffset CreatedAt
        bool IsSelected
    );
}
