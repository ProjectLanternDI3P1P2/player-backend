using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
            $"/api/player/v1/{playerId}/heroes",
            request,
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage replayed = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/player/v1/{playerId}/heroes",
            request,
            TestContext.Current.CancellationToken
        );

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        replayed.StatusCode.Should().Be(HttpStatusCode.OK);
        CreateHeroResponse? createdHero =
            await created.Content.ReadFromJsonAsync<CreateHeroResponse>(
                TestContext.Current.CancellationToken
            );
        CreateHeroResponse? replayedHero =
            await replayed.Content.ReadFromJsonAsync<CreateHeroResponse>(
                TestContext.Current.CancellationToken
            );
        createdHero
            .Should()
            .BeEquivalentTo(
                new
                {
                    Name = "Merlin",
                    ClassCode = "mage",
                    Level = 1,
                    MaximumHealth = 45,
                }
            );
        createdHero!.UnlockedSkillCodes.Should().ContainSingle().Which.Should().Be("mage-bolt");
        replayedHero!.Id.Should().Be(createdHero.Id);
        replayedHero.AlreadyExists.Should().BeTrue();

        HttpResponseMessage duplicateName = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/player/v1/{playerId}/heroes",
            new CreateHeroRequest("Merlin", "mage", Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage invalidName = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/player/v1/{playerId}/heroes",
            new CreateHeroRequest("12", "mage", Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        duplicateName.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        invalidName.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        foreach (
            string name in new[]
            {
                "Aldric",
                "Brom",
                "Celia",
                "Doran",
                "Elora",
                "Faron",
                "Galen",
                "Helia",
                "Iris",
            }
        )
        {
            HttpResponseMessage response = await fixture.HttpClient.PostAsJsonAsync(
                $"/api/player/v1/{playerId}/heroes",
                new CreateHeroRequest(name, "mage", Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        HttpResponseMessage limitReached = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/player/v1/{playerId}/heroes",
            new CreateHeroRequest("Jorah", "mage", Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );
        limitReached.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateHero_WithAppearance_IsReturnedByTheSheetAndTheList()
    {
        Guid playerId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedMageAsync(context, playerId));

        HttpResponseMessage created = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/player/v1/{playerId}/heroes",
            new
            {
                name = "Merlin",
                classCode = "mage",
                idempotencyKey = Guid.NewGuid(),
                appearance = new
                {
                    skinTone = 6,
                    hairStyle = "dap1",
                    hairColor = 9,
                },
            },
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage bald = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/player/v1/{playerId}/heroes",
            new
            {
                name = "Gandor",
                classCode = "mage",
                idempotencyKey = Guid.NewGuid(),
                appearance = new
                {
                    skinTone = 2,
                    hairStyle = "bald",
                    hairColor = (int?)null,
                },
            },
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage invalid = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/player/v1/{playerId}/heroes",
            new
            {
                name = "Orin",
                classCode = "mage",
                idempotencyKey = Guid.NewGuid(),
                appearance = new
                {
                    skinTone = 42,
                    hairStyle = "bob1",
                    hairColor = 1,
                },
            },
            TestContext.Current.CancellationToken
        );

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        bald.StatusCode.Should().Be(HttpStatusCode.Created);
        invalid.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        AppearanceHolder? createdHero = await created.Content.ReadFromJsonAsync<AppearanceHolder>(
            TestContext.Current.CancellationToken
        );
        createdHero!.Appearance.Should().Be(new AppearanceResponse(6, "dap1", 9));

        AppearanceHolder? sheet = await fixture.HttpClient.GetFromJsonAsync<AppearanceHolder>(
            $"/api/player/v1/{playerId}/heroes/{createdHero.Id}",
            TestContext.Current.CancellationToken
        );
        sheet!.Appearance.Should().Be(new AppearanceResponse(6, "dap1", 9));

        IReadOnlyList<AppearanceHolder>? heroes = await fixture.HttpClient.GetFromJsonAsync<
            IReadOnlyList<AppearanceHolder>
        >($"/api/player/v1/{playerId}/heroes", TestContext.Current.CancellationToken);
        heroes!
            .Select(hero => hero.Appearance)
            .Should()
            .BeEquivalentTo([
                new AppearanceResponse(6, "dap1", 9),
                new AppearanceResponse(2, "bald", null),
            ]);
    }

    private static async Task SeedMageAsync(PlayerDbContext context, Guid playerId)
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
        if (!await context.HeroClasses.AnyAsync(heroClass => heroClass.Code == "mage"))
            context.HeroClasses.Add(
                new HeroClass
                {
                    Code = "mage",
                    Label = "Mage",
                    BaseHealth = 45,
                }
            );
        if (!await context.Skills.AnyAsync(skill => skill.Code == "mage-bolt"))
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
    }

    private sealed record CreateHeroRequest(string Name, string ClassCode, Guid IdempotencyKey);

    private sealed record CreateHeroResponse(
        Guid Id,
        string Name,
        string ClassCode,
        int Level,
        int MaximumHealth,
        DateTimeOffset CreatedAt,
        IReadOnlyList<string> UnlockedSkillCodes,
        bool AlreadyExists
    );

    private sealed record AppearanceHolder(Guid Id, AppearanceResponse Appearance);

    private sealed record AppearanceResponse(int SkinTone, string HairStyle, int? HairColor);
}
