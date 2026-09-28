using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class CreateHeroIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task CreateHero_ThenReplayWithTheSameKey_CreatesExactlyOneHero()
    {
        Guid playerId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedMageAsync(context, playerId));
        Guid idempotencyKey = Guid.NewGuid();
        var request = new CreateHeroRequest("Merlin", "mage", idempotencyKey);

        HttpResponseMessage created = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/players/{playerId}/heroes",
            request,
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage replayed = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/players/{playerId}/heroes",
            request,
            TestContext.Current.CancellationToken
        );

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        replayed.StatusCode.Should().Be(HttpStatusCode.OK);
        CreateHeroResponse? createdHero = await created.Content.ReadFromJsonAsync<CreateHeroResponse>(
            TestContext.Current.CancellationToken
        );
        CreateHeroResponse? replayedHero = await replayed.Content.ReadFromJsonAsync<CreateHeroResponse>(
            TestContext.Current.CancellationToken
        );
        createdHero.Should().BeEquivalentTo(
            new { Name = "Merlin", ClassCode = "mage", Level = 1, MaximumHealth = 45 }
        );
        createdHero!.UnlockedSkillCodes.Should().ContainSingle().Which.Should().Be("mage-bolt");
        replayedHero!.Id.Should().Be(createdHero.Id);
        replayedHero.AlreadyExists.Should().BeTrue();

        HttpResponseMessage duplicateName = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/players/{playerId}/heroes",
            new CreateHeroRequest("Merlin", "mage", Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage invalidName = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/players/{playerId}/heroes",
            new CreateHeroRequest("12", "mage", Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        duplicateName.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        invalidName.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        foreach (string name in new[]
                 {
                     "Aldric", "Brom", "Celia", "Doran", "Elora", "Faron", "Galen", "Helia", "Iris",
                 })
        {
            HttpResponseMessage response = await fixture.HttpClient.PostAsJsonAsync(
                $"/api/v1/players/{playerId}/heroes",
                new CreateHeroRequest(name, "mage", Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        HttpResponseMessage limitReached = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/players/{playerId}/heroes",
            new CreateHeroRequest("Jorah", "mage", Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );
        limitReached.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private static Task SeedMageAsync(PlayerDbContext context, Guid playerId)
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
        context.HeroClasses.Add(new HeroClass { Code = "mage", Label = "Mage", BaseHealth = 45 });
        context.Skills.Add(
            new Skill
            {
                Code = "mage-bolt",
                ClassCode = "mage",
                Label = "Arcane Bolt",
                RequiredLevel = 1,
                TargetingType = "SingleEnemy",
            }
        );
        return Task.CompletedTask;
    }

    private sealed record CreateHeroRequest(string Name, string ClassCode, Guid IdempotencyKey);

    private sealed record CreateHeroResponse(
        Guid Id,
        string Name,
        string ClassCode,
        int Level,
        int MaximumHealth,
        IReadOnlyList<string> UnlockedSkillCodes,
        bool AlreadyExists
    );
}
