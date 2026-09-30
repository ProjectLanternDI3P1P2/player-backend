using Bogus;
using Player.Domain.Entities;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence.Seeding;

public static class PlayerFakeDataGenerator
{
    public static IReadOnlyList<HeroClass> CreateHeroClasses() =>
        [
            new HeroClass
            {
                Code = "warrior",
                Label = "Warrior",
                Description = "Front-line fighter. Takes the hits and protects the team.",
                BaseHealth = 60,
                BaseMana = 0,
            },
            new HeroClass
            {
                Code = "shaman",
                Label = "Shaman",
                Description = "Spirit wielder. Uses totems and resilience to support the party.",
                BaseHealth = 50,
                BaseMana = 0,
            },
            new HeroClass
            {
                Code = "mage",
                Label = "Mage",
                Description = "Master of the arcane. Ranged area damage, but fragile.",
                BaseHealth = 45,
                BaseMana = 0,
            },
        ];

    public static IReadOnlyList<Skill> CreateSkills() =>
        [
            new Skill
            {
                Code = "shaman-totem",
                ClassCode = "shaman",
                Label = "Totem",
                RequiredLevel = 1,
                TargetingType = "SingleEnemy",
            },
            new Skill
            {
                Code = "warrior-strike",
                ClassCode = "warrior",
                Label = "Strike",
                RequiredLevel = 1,
                TargetingType = "SingleEnemy",
            },
            new Skill
            {
                Code = "mage-bolt",
                ClassCode = "mage",
                Label = "Arcane Bolt",
                RequiredLevel = 1,
                TargetingType = "SingleEnemy",
            },
        ];

    public static IReadOnlyList<PlayerEntity> CreatePlayers(DateTimeOffset now, int count = 10) =>
        new Faker<PlayerEntity>("en")
            .UseSeed(417)
            .RuleFor(player => player.Id, _ => Guid.NewGuid())
            .RuleFor(player => player.DisplayName, faker => faker.Internet.UserName())
            .RuleFor(player => player.AccountStatus, _ => "Active")
            .RuleFor(player => player.NewGamePlusLevel, faker => faker.Random.Int(0, 3))
            .RuleFor(
                player => player.NewGamePlusUpdatedAt,
                (_, player) => player.NewGamePlusLevel == 0 ? null : now
            )
            .RuleFor(player => player.CreatedAt, _ => now)
            .Generate(count);

    public static IReadOnlyList<Hero> CreateHeroes(
        IReadOnlyList<PlayerEntity> players,
        IReadOnlyList<HeroClass> heroClasses,
        DateTimeOffset now
    ) =>
        players
            .Select(
                (player, index) =>
                    new Faker<Hero>("en")
                        .UseSeed(418 + index)
                        .RuleFor(hero => hero.Id, _ => Guid.NewGuid())
                        .RuleFor(hero => hero.PlayerId, _ => player.Id)
                        .RuleFor(
                            hero => hero.ClassCode,
                            _ => heroClasses[index % heroClasses.Count].Code
                        )
                        .RuleFor(hero => hero.Name, faker => faker.Name.FirstName())
                        .RuleFor(hero => hero.Level, faker => faker.Random.Int(1, 10))
                        .RuleFor(hero => hero.Strength, faker => faker.Random.Int(5, 20))
                        .RuleFor(hero => hero.Endurance, faker => faker.Random.Int(5, 20))
                        .RuleFor(hero => hero.Agility, faker => faker.Random.Int(5, 20))
                        .RuleFor(hero => hero.Intelligence, faker => faker.Random.Int(5, 20))
                        .RuleFor(hero => hero.CreatedAt, _ => now)
                        .Generate()
            )
            .ToList();
}
