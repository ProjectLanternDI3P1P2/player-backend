using Microsoft.EntityFrameworkCore;
using Player.Domain.Entities;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(
        PlayerDbContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (await context.Players.AnyAsync(cancellationToken))
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        IReadOnlyList<HeroClass> heroClasses = PlayerFakeDataGenerator.CreateHeroClasses();
        IReadOnlyList<Skill> skills = PlayerFakeDataGenerator.CreateSkills();
        IReadOnlyList<PlayerEntity> players = PlayerFakeDataGenerator.CreatePlayers(now);
        IReadOnlyList<Hero> heroes = PlayerFakeDataGenerator.CreateHeroes(
            players,
            heroClasses,
            now
        );

        await context.HeroClasses.AddRangeAsync(heroClasses, cancellationToken);
        await context.Skills.AddRangeAsync(skills, cancellationToken);
        await context.Players.AddRangeAsync(players, cancellationToken);
        await context.Heroes.AddRangeAsync(heroes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
