using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class ListHeroClassesIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task List_WhenClassesExist_ReturnsTheirCreationMetadata()
    {
        await fixture.SeedAsync(SeedAsync);

        HttpResponseMessage response = await fixture.HttpClient.GetAsync(
            "/api/v1/hero-classes",
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<HeroClassResponse>? heroClasses = await response.Content.ReadFromJsonAsync<
            List<HeroClassResponse>
        >(TestContext.Current.CancellationToken);
        heroClasses
            .Should()
            .BeEquivalentTo(
                [
                    new HeroClassResponse(
                        "test-mage",
                        "Mage",
                        "Master of the arcane. Ranged area damage, but fragile.",
                        45
                    ),
                    new HeroClassResponse(
                        "test-warrior",
                        "Warrior",
                        "Front-line fighter. Takes the hits and protects the team.",
                        60
                    ),
                ],
                options => options.WithStrictOrdering()
            );
    }

    private static Task SeedAsync(PlayerDbContext context)
    {
        context.AddRange(
            new HeroClass
            {
                Code = "test-warrior",
                Label = "Warrior",
                Description = "Front-line fighter. Takes the hits and protects the team.",
                BaseHealth = 60,
            },
            new HeroClass
            {
                Code = "test-mage",
                Label = "Mage",
                Description = "Master of the arcane. Ranged area damage, but fragile.",
                BaseHealth = 45,
            }
        );
        return Task.CompletedTask;
    }

    private sealed record HeroClassResponse(
        string Code,
        string Label,
        string Description,
        int BaseHealth
    );
}
