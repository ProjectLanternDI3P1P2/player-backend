using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class SelectHeroIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task Select_ActiveOwnedHero_PersistsTheSelectionAndReturnsItFromTheList()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedAsync(context, playerId, heroId));

        HttpResponseMessage response = await fixture.HttpClient.PutAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}/selection",
            null,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await fixture.AssertAsync(context =>
            context
                .Players.Single(player => player.Id == playerId)
                .SelectedHeroId.Should()
                .Be(heroId)
        );

        List<HeroResponse>? heroes = await fixture.HttpClient.GetFromJsonAsync<List<HeroResponse>>(
            $"/api/v1/players/{playerId}/heroes",
            TestContext.Current.CancellationToken
        );
        heroes.Should().ContainSingle().Which.IsSelected.Should().BeTrue();
    }

    private static Task SeedAsync(PlayerDbContext context, Guid playerId, Guid heroId)
    {
        const string classCode = "selection-test-class";
        context.AddRange(
            new PlayerEntity
            {
                Id = playerId,
                DisplayName = "test-player",
                AccountStatus = "Active",
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new HeroClass
            {
                Code = classCode,
                Label = "Selection test",
                BaseHealth = 1,
            },
            new Hero
            {
                Id = heroId,
                PlayerId = playerId,
                ClassCode = classCode,
                Name = "Aster",
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        return Task.CompletedTask;
    }

    private sealed record HeroResponse(bool IsSelected);
}
