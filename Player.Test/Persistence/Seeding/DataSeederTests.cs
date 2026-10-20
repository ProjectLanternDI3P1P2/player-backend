using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Player.Infrastructure.Persistence;
using Player.Infrastructure.Persistence.Seeding;

namespace Player.Test.Persistence.Seeding;

public sealed class DataSeederTests
{
    [Fact]
    public async Task SeedAsync_EmptyDatabase_CreatesDemoPlayerWithThreeDistinctHeroes()
    {
        DbContextOptions<PlayerDbContext> options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new PlayerDbContext(options);

        await DataSeeder.SeedAsync(context, TestContext.Current.CancellationToken);

        var demoPlayer = await context
            .Players.Include(player => player.Heroes)
            .SingleAsync(
                player => player.Id == PlayerFakeDataGenerator.DemoPlayerId,
                TestContext.Current.CancellationToken
            );

        demoPlayer.DisplayName.Should().Be("MaelisRune");
        demoPlayer.SelectedHeroId.Should().Be(PlayerFakeDataGenerator.DemoMageId);
        demoPlayer.Heroes.Should().HaveCount(3);
        demoPlayer
            .Heroes.Select(hero => hero.ClassCode)
            .Should()
            .BeEquivalentTo("warrior", "shaman", "mage");
        demoPlayer
            .Heroes.Select(hero => (hero.SkinTone, hero.HairStyle, hero.HairColor))
            .Should()
            .OnlyHaveUniqueItems();
        (await context.HeroSkills.CountAsync(TestContext.Current.CancellationToken)).Should().Be(3);

        await DataSeeder.SeedAsync(context, TestContext.Current.CancellationToken);

        (await context.Players.CountAsync(TestContext.Current.CancellationToken)).Should().Be(11);
        (await context.Heroes.CountAsync(TestContext.Current.CancellationToken)).Should().Be(13);
    }
}
