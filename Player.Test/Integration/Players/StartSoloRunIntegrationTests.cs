using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Player.Application.Ports;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class StartSoloRunIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task StartSoloRun_ThenReplay_CreatesExactlyOneActiveSession()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid runId = Guid.NewGuid();
        await fixture.SeedAsync(context =>
            SeedHeroAsync(context, playerId, heroId, "solo-session-success")
        );
        fixture.SetDungeonClient(new StubDungeonClient(new DungeonRun(runId, "dungeon-seed")));
        Guid idempotencyKey = Guid.NewGuid();
        var request = new StartSoloRunRequest(idempotencyKey);
        string path = $"/api/v1/players/{playerId}/heroes/{heroId}/sessions";

        HttpResponseMessage created = await fixture.HttpClient.PostAsJsonAsync(
            path,
            request,
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage replayed = await fixture.HttpClient.PostAsJsonAsync(
            path,
            request,
            TestContext.Current.CancellationToken
        );

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        replayed.StatusCode.Should().Be(HttpStatusCode.OK);
        StartSoloRunResponse? createdBody =
            await created.Content.ReadFromJsonAsync<StartSoloRunResponse>(
                TestContext.Current.CancellationToken
            );
        StartSoloRunResponse? replayedBody =
            await replayed.Content.ReadFromJsonAsync<StartSoloRunResponse>(
                TestContext.Current.CancellationToken
            );
        createdBody
            .Should()
            .BeEquivalentTo(
                new
                {
                    Hero = new { Id = heroId },
                    State = "Active",
                    DungeonRunId = runId,
                    DungeonSeed = "dungeon-seed",
                    FailureReason = (string?)null,
                    AlreadyExists = false,
                }
            );
        replayedBody!.SessionId.Should().Be(createdBody!.SessionId);
        replayedBody.AlreadyExists.Should().BeTrue();

        await fixture.AssertAsync(context =>
            context
                .GameSessions.Count(session => session.Id == createdBody.SessionId)
                .Should()
                .Be(1)
        );
    }

    [Fact]
    public async Task StartSoloRun_WhenDungeonIsUnavailable_PersistsAFailedSession()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        await fixture.SeedAsync(context =>
            SeedHeroAsync(context, playerId, heroId, "solo-session-failure")
        );
        fixture.SetDungeonClient(new FailingDungeonClient());

        HttpResponseMessage response = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}/sessions",
            new StartSoloRunRequest(Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        StartSoloRunResponse? body = await response.Content.ReadFromJsonAsync<StartSoloRunResponse>(
            TestContext.Current.CancellationToken
        );
        body.Should()
            .BeEquivalentTo(new { State = "Failed", FailureReason = "DungeonUnavailable" });
    }

    private static Task SeedHeroAsync(
        PlayerDbContext context,
        Guid playerId,
        Guid heroId,
        string classCode
    )
    {
        context.Players.Add(
            new Player.Domain.Entities.Player
            {
                Id = playerId,
                DisplayName = "test-player",
                AccountStatus = "Active",
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        context.Heroes.Add(
            new Hero
            {
                Id = heroId,
                PlayerId = playerId,
                ClassCode = classCode,
                Name = "Merlin",
                Level = 1,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        context.HeroClasses.Add(
            new HeroClass
            {
                Code = classCode,
                Label = classCode,
                BaseHealth = 45,
            }
        );
        return Task.CompletedTask;
    }

    private sealed record StartSoloRunRequest(Guid IdempotencyKey);

    private sealed record StartSoloRunResponse(
        Guid SessionId,
        HeroResponse Hero,
        string State,
        Guid? DungeonRunId,
        string? DungeonSeed,
        string? FailureReason,
        bool AlreadyExists
    );

    private sealed record HeroResponse(Guid Id, string Name, string ClassCode, int Level);

    private sealed class StubDungeonClient(DungeonRun run) : IDungeonClient
    {
        public Task<DungeonRun> StartRunAsync(
            Guid sessionId,
            Guid heroId,
            CancellationToken cancellationToken
        ) => Task.FromResult(run);
    }

    private sealed class FailingDungeonClient : IDungeonClient
    {
        public Task<DungeonRun> StartRunAsync(
            Guid sessionId,
            Guid heroId,
            CancellationToken cancellationToken
        ) => Task.FromException<DungeonRun>(new HttpRequestException());
    }
}
