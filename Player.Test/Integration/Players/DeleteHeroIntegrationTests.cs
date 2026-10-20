using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class DeleteHeroIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task Delete_FreeHero_FreesTheSlotAndIsIdempotent()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedAsync(context, playerId, heroId));

        HttpResponseMessage first = await fixture.HttpClient.DeleteAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage replay = await fixture.HttpClient.DeleteAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        (
            await first.Content.ReadFromJsonAsync<DeleteResponse>(
                TestContext.Current.CancellationToken
            )
        )!
            .AlreadyDeleted.Should()
            .BeFalse();
        replay.StatusCode.Should().Be(HttpStatusCode.OK);
        (
            await replay.Content.ReadFromJsonAsync<DeleteResponse>(
                TestContext.Current.CancellationToken
            )
        )!
            .AlreadyDeleted.Should()
            .BeTrue();

        List<object>? heroes = await fixture.HttpClient.GetFromJsonAsync<List<object>>(
            $"/api/v1/players/{playerId}/heroes",
            TestContext.Current.CancellationToken
        );
        heroes.Should().BeEmpty();
        await fixture.AssertAsync(context =>
            context.Players.Single(player => player.Id == playerId).SelectedHeroId.Should().BeNull()
        );
    }

    [Fact]
    public async Task GetSheet_DeletedHero_Returns410InsteadOf404()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedAsync(context, playerId, heroId));
        await fixture.HttpClient.DeleteAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );

        HttpResponseMessage response = await fixture.HttpClient.GetAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Delete_HeroInALobby_ClosesTheLobbyFirst()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        await fixture.SeedAsync(async context =>
        {
            await SeedAsync(context, playerId, heroId);
            context.Add(Session(sessionId, playerId, heroId, "Lobby"));
            await context.SaveChangesAsync();
        });

        HttpResponseMessage response = await fixture.HttpClient.DeleteAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await fixture.AssertAsync(context =>
        {
            GameSession session = context.GameSessions.Single(x => x.Id == sessionId);
            session.Status.Should().Be("Abandoned");
            session.EndedAt.Should().NotBeNull();
            context
                .GameSessionMembers.Single(x => x.SessionId == sessionId)
                .LeftAt.Should()
                .NotBeNull();
            context.Heroes.Single(x => x.Id == heroId).IsDeleted.Should().BeTrue();
        });
    }

    [Fact]
    public async Task Delete_HeroInARunningSession_Returns409AndKeepsTheHero()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        await fixture.SeedAsync(async context =>
        {
            await SeedAsync(context, playerId, heroId);
            context.Add(Session(sessionId, playerId, heroId, "Active"));
            await context.SaveChangesAsync();
        });

        HttpResponseMessage response = await fixture.HttpClient.DeleteAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await fixture.AssertAsync(context =>
        {
            context.Heroes.Single(x => x.Id == heroId).IsDeleted.Should().BeFalse();
            context.GameSessions.Single(x => x.Id == sessionId).Status.Should().Be("Active");
        });
    }

    [Fact]
    public async Task Delete_ThenReuseTheSameName_IsAllowed()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedAsync(context, playerId, heroId));
        await fixture.HttpClient.DeleteAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );

        Func<Task> reuse = () =>
            fixture.SeedAsync(async context =>
            {
                Hero deleted = context.Heroes.Single(x => x.Id == heroId);
                context.Add(
                    new Hero
                    {
                        Id = Guid.NewGuid(),
                        PlayerId = playerId,
                        ClassCode = deleted.ClassCode,
                        Name = deleted.Name,
                        CreatedAt = DateTimeOffset.UtcNow,
                    }
                );
                await context.SaveChangesAsync();
            });

        await reuse.Should().NotThrowAsync();
    }

    private static GameSession Session(Guid sessionId, Guid playerId, Guid heroId, string status) =>
        new()
        {
            Id = sessionId,
            CreatorPlayerId = playerId,
            Status = status,
            Mode = "Solo",
            Members =
            [
                new GameSessionMember
                {
                    SessionId = sessionId,
                    HeroId = heroId,
                    MemberStatus = "Active",
                    JoinedAt = DateTimeOffset.UtcNow,
                },
            ],
        };

    private static async Task SeedAsync(PlayerDbContext context, Guid playerId, Guid heroId)
    {
        string classCode = $"deletion-test-{playerId:N}";
        context.AddRange(
            new PlayerEntity
            {
                Id = playerId,
                DisplayName = "test-player",
                AccountStatus = "Active",
                CreatedAt = DateTimeOffset.UtcNow,
                SelectedHeroId = heroId,
            },
            new HeroClass
            {
                Code = classCode,
                Label = "Deletion test",
                BaseHealth = 1,
            },
            new Hero
            {
                Id = heroId,
                PlayerId = playerId,
                ClassCode = classCode,
                Name = "Brann",
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        await context.SaveChangesAsync();
    }

    private sealed record DeleteResponse(bool AlreadyDeleted);
}
