using Microsoft.EntityFrameworkCore;
using Player.Domain.Entities;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence.Seeding;

public static class DataSeeder
{
    private const string TruncateDevelopmentDataSql = """
        TRUNCATE TABLE
            "game_session_transition",
            "game_session_member",
            "game_session",
            "matchmaking_queue_entry",
            "matchmaking_group",
            "idempotency_key",
            "hero_skill",
            "hero",
            "skill",
            "hero_class",
            "player"
        RESTART IDENTITY CASCADE;
        """;

    public static async Task SeedAsync(
        PlayerDbContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (context.Database.IsRelational())
        {
            await context.Database.ExecuteSqlRawAsync(
                TruncateDevelopmentDataSql,
                cancellationToken
            );
        }
        else
        {
            await context.Database.EnsureDeletedAsync(cancellationToken);
        }
        context.ChangeTracker.Clear();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        IReadOnlyList<HeroClass> heroClasses = PlayerFakeDataGenerator.CreateHeroClasses();
        IReadOnlyList<Skill> skills = PlayerFakeDataGenerator.CreateSkills();
        List<PlayerEntity> players =
        [
            PlayerFakeDataGenerator.CreateDemoPlayer(now),
            .. PlayerFakeDataGenerator.CreatePlayers(now),
        ];
        List<Hero> heroes =
        [
            .. PlayerFakeDataGenerator.CreateDemoHeroes(now),
            .. PlayerFakeDataGenerator.CreateHeroes(players.Skip(1).ToList(), heroClasses, now),
        ];

        await context.HeroClasses.AddRangeAsync(heroClasses, cancellationToken);
        await context.Skills.AddRangeAsync(skills, cancellationToken);
        await context.Players.AddRangeAsync(players, cancellationToken);
        await context.Heroes.AddRangeAsync(heroes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
