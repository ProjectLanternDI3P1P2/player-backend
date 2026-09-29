using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class GetHeroSheetIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task GetSheet_WhenHeroIsOwned_ReturnsOnlyThePlayerOwnedSheet()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedAsync(context, playerId, heroId));

        HttpResponseMessage response = await fixture.HttpClient.GetAsync(
            $"/api/v1/players/{playerId}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HeroSheetResponse? body = await response.Content.ReadFromJsonAsync<HeroSheetResponse>(
            TestContext.Current.CancellationToken
        );
        body.Should()
            .BeEquivalentTo(
                new HeroSheetResponse(
                    heroId,
                    "Aldric",
                    ClassCode(heroId),
                    4,
                    new AttributesResponse(10, 3, 4, 2),
                    78,
                    [new AbilityResponse(SkillCode(heroId), "Strike", "SingleEnemy")]
                )
            );
    }

    [Fact]
    public async Task GetSheet_WhenHeroBelongsToAnotherPlayer_ReturnsNotFound()
    {
        Guid ownerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedAsync(context, ownerId, heroId));

        HttpResponseMessage response = await fixture.HttpClient.GetAsync(
            $"/api/v1/players/{Guid.NewGuid()}/heroes/{heroId}",
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static Task SeedAsync(PlayerDbContext context, Guid playerId, Guid heroId)
    {
        DateTimeOffset now = new(2026, 9, 29, 12, 0, 0, TimeSpan.Zero);
        var heroClass = new HeroClass
        {
            Code = ClassCode(heroId),
            Label = "Warrior",
            BaseHealth = 60,
        };
        var skill = new Skill
        {
            Code = SkillCode(heroId),
            ClassCode = heroClass.Code,
            Label = "Strike",
            RequiredLevel = 1,
            TargetingType = "SingleEnemy",
        };
        var hero = new Hero
        {
            Id = heroId,
            PlayerId = playerId,
            ClassCode = heroClass.Code,
            Name = "Aldric",
            Level = 4,
            Strength = 10,
            Endurance = 3,
            Agility = 4,
            Intelligence = 2,
            CreatedAt = now,
        };
        var heroSkill = new HeroSkill
        {
            HeroId = heroId,
            SkillCode = skill.Code,
            UnlockedAt = now,
            IsActive = true,
        };

        context.AddRange(
            new PlayerEntity
            {
                Id = playerId,
                DisplayName = "test-player",
                AccountStatus = "Active",
                CreatedAt = now,
            },
            heroClass,
            skill,
            hero,
            heroSkill
        );
        return Task.CompletedTask;
    }

    private sealed record HeroSheetResponse(
        Guid Id,
        string Name,
        string ClassCode,
        int Level,
        AttributesResponse Attributes,
        int MaximumHealth,
        IReadOnlyList<AbilityResponse> Abilities
    );

    private sealed record AttributesResponse(
        int Strength,
        int Endurance,
        int Agility,
        int Intelligence
    );

    private sealed record AbilityResponse(string Code, string Label, string TargetingType);

    private static string ClassCode(Guid heroId) => $"warrior-{heroId:N}";

    private static string SkillCode(Guid heroId) => $"warrior-strike-{heroId:N}";
}
