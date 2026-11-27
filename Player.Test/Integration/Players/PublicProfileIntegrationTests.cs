using System.Net;
using System.Text.Json;
using FluentAssertions;
using Player.Domain.Entities;
using Player.Domain.Enums;
using Player.Infrastructure.Persistence;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class PublicProfileIntegrationTests(PlayerEndpointFixture fixture)
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetByHandle_AnonymousVisitor_ReturnsPublicProfileWithFreshnessPolicy()
    {
        Guid playerId = Guid.NewGuid();
        string publicHandle = PlayerPublicHandle.Create("Public Visitor", playerId);
        await fixture.SeedAsync(context => SeedAsync(context, playerId, "Public Visitor", null));

        HttpResponseMessage response = await AnonymousClient()
            .GetAsync(
                $"/api/player/v1/players/by-handle/{publicHandle.ToUpperInvariant()}/public-profile",
                TestContext.Current.CancellationToken
            );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl!.Public.Should().BeTrue();
        response.Headers.CacheControl.MaxAge.Should().Be(TimeSpan.FromSeconds(60));
        response
            .Headers.CacheControl.Extensions.Should()
            .ContainSingle(extension =>
                extension.Name == "stale-while-revalidate" && extension.Value == "300"
            );
        string json = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        json.Should()
            .NotContainEquivalentOf("externalUserId")
            .And.NotContainEquivalentOf("accountStatus");
        JsonSerializer
            .Deserialize<PublicProfileBody>(json, WebJson)
            .Should()
            .BeEquivalentTo(
                new PublicProfileBody(
                    playerId,
                    publicHandle,
                    "Public Visitor",
                    1,
                    [new PublicHeroBody("Aldric", ClassCode(playerId), "Warrior", 5)],
                    new CountersBody(1, 5, 2)
                ),
                options => options.WithStrictOrdering()
            );
    }

    [Fact]
    public async Task GetById_UnknownPlayer_ReturnsNotFoundWithShortFreshnessPolicy()
    {
        HttpResponseMessage response = await AnonymousClient()
            .GetAsync(
                $"/api/player/v1/players/{Guid.NewGuid()}/public-profile",
                TestContext.Current.CancellationToken
            );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Headers.CacheControl!.Public.Should().BeTrue();
        response.Headers.CacheControl.MaxAge.Should().Be(TimeSpan.FromSeconds(60));
        response.Headers.CacheControl.Extensions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_AnonymizedPlayer_ReturnsNotFound()
    {
        Guid playerId = Guid.NewGuid();
        await fixture.SeedAsync(context =>
            SeedAsync(
                context,
                playerId,
                "Gone Player",
                new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero)
            )
        );

        HttpResponseMessage response = await AnonymousClient()
            .GetAsync(
                $"/api/player/v1/players/{playerId}/public-profile",
                TestContext.Current.CancellationToken
            );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HeadByHandle_AnonymousVisitor_ReturnsOkWithFreshnessPolicy()
    {
        Guid playerId = Guid.NewGuid();
        string publicHandle = PlayerPublicHandle.Create("Head Visitor", playerId);
        await fixture.SeedAsync(context => SeedAsync(context, playerId, "Head Visitor", null));

        using var request = new HttpRequestMessage(
            HttpMethod.Head,
            $"/api/player/v1/players/by-handle/{publicHandle}/public-profile"
        );
        HttpResponseMessage response = await AnonymousClient()
            .SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl!.Public.Should().BeTrue();
        response.Headers.CacheControl.MaxAge.Should().Be(TimeSpan.FromSeconds(60));
    }

    private HttpClient AnonymousClient()
    {
        HttpClient client = fixture.HttpClient;
        client.DefaultRequestHeaders.Remove("X-User-Id");
        return client;
    }

    private static Task SeedAsync(
        PlayerDbContext context,
        Guid playerId,
        string displayName,
        DateTimeOffset? anonymizedAt
    )
    {
        DateTimeOffset now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
        var heroClass = new HeroClass
        {
            Code = ClassCode(playerId),
            Label = "Warrior",
            BaseHealth = 60,
        };
        var aldric = new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            ClassCode = heroClass.Code,
            Name = "Aldric",
            Level = 5,
            CreatedAt = now,
        };
        var ghost = new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            ClassCode = heroClass.Code,
            Name = "Ghost",
            Level = 9,
            IsDeleted = true,
            CreatedAt = now,
            DeletedAt = now,
        };

        context.AddRange(
            new PlayerEntity
            {
                Id = playerId,
                DisplayName = displayName,
                PublicHandle = PlayerPublicHandle.Create(displayName, playerId),
                AccountStatus = "Active",
                NewGamePlusLevel = 1,
                CreatedAt = now,
                AnonymizedAt = anonymizedAt,
            },
            heroClass,
            aldric,
            ghost,
            Session(playerId, aldric.Id, GameSessionStatus.Completed, now),
            Session(playerId, ghost.Id, GameSessionStatus.Completed, now),
            Session(playerId, aldric.Id, GameSessionStatus.Abandoned, now)
        );
        return Task.CompletedTask;
    }

    private static GameSession Session(
        Guid playerId,
        Guid heroId,
        GameSessionStatus status,
        DateTimeOffset endedAt
    )
    {
        Guid sessionId = Guid.NewGuid();
        return new GameSession
        {
            Id = sessionId,
            CreatorPlayerId = playerId,
            Status = status,
            Mode = "Solo",
            EndedAt = endedAt,
            Members =
            [
                new GameSessionMember
                {
                    SessionId = sessionId,
                    HeroId = heroId,
                    MemberStatus = GameSessionMemberStatus.Left,
                    JoinedAt = endedAt.AddMinutes(-30),
                    LeftAt = endedAt,
                },
            ],
        };
    }

    private static string ClassCode(Guid playerId) => $"public-{playerId:N}";

    private sealed record PublicProfileBody(
        Guid PlayerId,
        string PublicHandle,
        string DisplayName,
        int NewGamePlusLevel,
        IReadOnlyList<PublicHeroBody> Heroes,
        CountersBody Counters
    );

    private sealed record PublicHeroBody(
        string Name,
        string ClassCode,
        string ClassName,
        int Level
    );

    private sealed record CountersBody(
        int HeroCount,
        int HighestHeroLevel,
        int CompletedSessionCount
    );
}
