using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Player.Contracts.V1;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;

namespace Player.Test.Integration.Players;

/// <summary>Reads hero appearance through the real gRPC pipeline and PostgreSQL.</summary>
[Collection(PlayerEndpointCollection.Name)]
public sealed class PlayerHeroGrpcIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task GetHeroAppearance_HeroWithAChosenAppearance_ReturnsIt()
    {
        Guid heroId = await SeedHeroAsync(skinTone: 6, hairStyle: "dap1", hairColor: 9);

        HeroAppearanceReply reply = await GetAppearanceAsync(heroId.ToString());

        reply.HeroId.Should().Be(heroId.ToString());
        reply.SkinTone.Should().Be(6);
        reply.HairStyle.Should().Be("dap1");
        reply.HasHairColor.Should().BeTrue();
        reply.HairColor.Should().Be(9);
    }

    [Fact]
    public async Task GetHeroAppearance_BaldHero_ReturnsNoHairColor()
    {
        Guid heroId = await SeedHeroAsync(skinTone: 2, hairStyle: "bald", hairColor: null);

        HeroAppearanceReply reply = await GetAppearanceAsync(heroId.ToString());

        reply.HairStyle.Should().Be("bald");
        reply.HasHairColor.Should().BeFalse();
    }

    [Fact]
    public async Task GetHeroAppearance_HeroNeverCustomized_ReturnsTheDefaultAppearance()
    {
        Guid heroId = await SeedHeroAsync();

        HeroAppearanceReply reply = await GetAppearanceAsync(heroId.ToString());

        reply.SkinTone.Should().Be(HeroAppearanceCatalog.DefaultSkinTone);
        reply.HairStyle.Should().Be(HeroAppearanceCatalog.DefaultHairStyle);
        reply.HairColor.Should().Be(HeroAppearanceCatalog.DefaultHairColor);
    }

    [Fact]
    public async Task GetHeroAppearance_AfterTheAppearanceChanged_ReturnsTheNewOne()
    {
        Guid heroId = await SeedHeroAsync(skinTone: 3, hairStyle: "bob1", hairColor: 4);
        (await GetAppearanceAsync(heroId.ToString())).SkinTone.Should().Be(3);
        await fixture.SeedAsync(async context =>
        {
            await context
                .Heroes.Where(hero => hero.Id == heroId)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(hero => hero.SkinTone, 8)
                        .SetProperty(hero => hero.HairStyle, "dap1")
                        .SetProperty(hero => hero.HairColor, 12)
                );
        });

        HeroAppearanceReply reply = await GetAppearanceAsync(heroId.ToString());

        reply.SkinTone.Should().Be(8);
        reply.HairStyle.Should().Be("dap1");
        reply.HairColor.Should().Be(12);
    }

    [Fact]
    public async Task GetHeroAppearance_UnknownHero_ReturnsNotFound()
    {
        Func<Task> action = () => GetAppearanceAsync(Guid.NewGuid().ToString());

        (await action.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task GetHeroAppearance_MalformedHeroId_ReturnsInvalidArgument()
    {
        Func<Task> action = () => GetAppearanceAsync("not-a-uuid");

        (await action.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task GetCombatantSnapshot_HeroWithASkill_ReturnsStatsAbilitiesAndAppearance()
    {
        Guid heroId = await SeedHeroAsync(skinTone: 6, hairStyle: "dap1", hairColor: 9);
        string skillCode = await SeedSkillAsync(heroId);

        CombatantSnapshotReply reply = await GetCombatantAsync(heroId.ToString());

        reply.HeroId.Should().Be(heroId.ToString());
        reply.Name.Should().Be("Merlin");
        reply.Level.Should().Be(1);
        reply.MaxHp.Should().Be(45 + (10 * 6));
        reply.CurrentHp.Should().Be(reply.MaxHp);
        reply.MaxMana.Should().Be(20 * 3);
        reply.Attack.Should().Be(4);
        reply.Defense.Should().Be(5);
        reply.Speed.Should().Be(12);
        reply.Abilities.Should().ContainSingle(ability => ability.Code == skillCode);
        reply.Abilities[0].ManaCost.Should().Be(6);
        reply.Abilities[0].Power.Should().Be(170);
        reply.Appearance.SkinTone.Should().Be(6);
        reply.Appearance.HairColor.Should().Be(9);
    }

    [Fact]
    public async Task GetCombatantSnapshot_DeletedHero_ReturnsNotFound()
    {
        Guid heroId = await SeedHeroAsync();
        await fixture.SeedAsync(async context =>
        {
            await context
                .Heroes.Where(hero => hero.Id == heroId)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(hero => hero.IsDeleted, true)
                        .SetProperty(hero => hero.DeletedAt, DateTimeOffset.UtcNow)
                );
        });

        Func<Task> action = () => GetCombatantAsync(heroId.ToString());

        (await action.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task GetCombatantSnapshot_UnknownHero_ReturnsNotFound()
    {
        Func<Task> action = () => GetCombatantAsync(Guid.NewGuid().ToString());

        (await action.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.NotFound);
    }

    private async Task<CombatantSnapshotReply> GetCombatantAsync(string heroId)
    {
        using GrpcChannel channel = fixture.CreateGrpcChannel();
        var client = new PlayerHeroService.PlayerHeroServiceClient(channel);
        return await client.GetCombatantSnapshotAsync(
            new GetCombatantSnapshotRequest { HeroId = heroId },
            cancellationToken: TestContext.Current.CancellationToken
        );
    }

    private async Task<string> SeedSkillAsync(Guid heroId)
    {
        string skillCode = $"bolt-{heroId:N}";
        await fixture.SeedAsync(async context =>
        {
            string classCode = await context
                .Heroes.Where(hero => hero.Id == heroId)
                .Select(hero => hero.ClassCode)
                .SingleAsync();
            context.Skills.Add(
                new Skill
                {
                    Code = skillCode,
                    ClassCode = classCode,
                    Label = "Arcane Bolt",
                    RequiredLevel = 1,
                    TargetingType = "SingleEnemy",
                    ManaCost = 6,
                    Power = 170,
                }
            );
            context.HeroSkills.Add(
                new HeroSkill
                {
                    HeroId = heroId,
                    SkillCode = skillCode,
                    IsActive = true,
                    UnlockedAt = DateTimeOffset.UtcNow,
                }
            );
            await context.SaveChangesAsync();
        });
        return skillCode;
    }

    private async Task<HeroAppearanceReply> GetAppearanceAsync(string heroId)
    {
        using GrpcChannel channel = fixture.CreateGrpcChannel();
        var client = new PlayerHeroService.PlayerHeroServiceClient(channel);
        return await client.GetHeroAppearanceAsync(
            new GetHeroAppearanceRequest { HeroId = heroId },
            cancellationToken: TestContext.Current.CancellationToken
        );
    }

    private async Task<Guid> SeedHeroAsync(
        int? skinTone = null,
        string? hairStyle = null,
        int? hairColor = HeroAppearanceCatalog.DefaultHairColor
    )
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        string classCode = $"hero-grpc-{heroId:N}";
        await fixture.SeedAsync(async context =>
        {
            context.Players.Add(
                new Player.Domain.Entities.Player
                {
                    Id = playerId,
                    DisplayName = "hero-grpc-player",
                    AccountStatus = "Active",
                    CreatedAt = DateTimeOffset.UtcNow,
                }
            );
            context.HeroClasses.Add(
                new HeroClass
                {
                    Code = classCode,
                    Label = "Mage",
                    BaseHealth = 45,
                }
            );
            var hero = new Hero
            {
                Id = heroId,
                PlayerId = playerId,
                ClassCode = classCode,
                Name = "Merlin",
                Level = 1,
                Strength = 4,
                Endurance = 10,
                Agility = 12,
                Intelligence = 20,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            if (skinTone is not null)
                hero.SkinTone = skinTone.Value;
            if (hairStyle is not null)
            {
                hero.HairStyle = hairStyle;
                hero.HairColor = hairColor;
            }

            context.Heroes.Add(hero);
            await context.SaveChangesAsync();
        });
        return heroId;
    }
}
