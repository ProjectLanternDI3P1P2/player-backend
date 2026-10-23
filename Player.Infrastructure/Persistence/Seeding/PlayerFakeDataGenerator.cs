using Bogus;
using Player.Domain.Entities;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence.Seeding;

public static class PlayerFakeDataGenerator
{
    public static readonly Guid DemoPlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public const string DemoExternalUserId = "00000000-0000-0000-0000-000000000010";
    public static readonly Guid DemoWarriorId = Guid.Parse("00000000-0000-0000-0000-000000000101");
    public static readonly Guid DemoShamanId = Guid.Parse("00000000-0000-0000-0000-000000000102");
    public static readonly Guid DemoMageId = Guid.Parse("00000000-0000-0000-0000-000000000103");

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

    /// <summary>
    /// Creates the development account used by the game client and manual API testing.
    /// Its predictable identifier makes it safe to put directly in a local URL or request.
    /// </summary>
    public static PlayerEntity CreateDemoPlayer(DateTimeOffset now) =>
        new()
        {
            Id = DemoPlayerId,
            ExternalUserId = DemoExternalUserId,
            DisplayName = "MaelisRune",
            AccountStatus = "Active",
            NewGamePlusLevel = 2,
            NewGamePlusUpdatedAt = now,
            SelectedHeroId = DemoMageId,
            CreatedAt = now,
        };

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

    public static IReadOnlyList<Hero> CreateDemoHeroes(DateTimeOffset now) =>
        [
            new Hero
            {
                Id = DemoWarriorId,
                PlayerId = DemoPlayerId,
                ClassCode = "warrior",
                Name = "Aldric",
                Level = 9,
                Strength = 18,
                Endurance = 16,
                Agility = 9,
                Intelligence = 7,
                SkinTone = 3,
                HairStyle = HeroAppearanceCatalog.DapperHairStyle,
                HairColor = 5,
                CreatedAt = now,
                HeroSkills =
                [
                    new HeroSkill
                    {
                        HeroId = DemoWarriorId,
                        SkillCode = "warrior-strike",
                        UnlockedAt = now,
                        IsActive = true,
                    },
                ],
            },
            new Hero
            {
                Id = DemoShamanId,
                PlayerId = DemoPlayerId,
                ClassCode = "shaman",
                Name = "Sahra",
                Level = 7,
                Strength = 8,
                Endurance = 13,
                Agility = 12,
                Intelligence = 17,
                SkinTone = 8,
                HairStyle = HeroAppearanceCatalog.BobHairStyle,
                HairColor = 11,
                CreatedAt = now,
                HeroSkills =
                [
                    new HeroSkill
                    {
                        HeroId = DemoShamanId,
                        SkillCode = "shaman-totem",
                        UnlockedAt = now,
                        IsActive = true,
                    },
                ],
            },
            new Hero
            {
                Id = DemoMageId,
                PlayerId = DemoPlayerId,
                ClassCode = "mage",
                Name = "Corvin",
                Level = 11,
                Strength = 6,
                Endurance = 9,
                Agility = 14,
                Intelligence = 20,
                SkinTone = 5,
                HairStyle = HeroAppearanceCatalog.BaldHairStyle,
                HairColor = null,
                CreatedAt = now,
                HeroSkills =
                [
                    new HeroSkill
                    {
                        HeroId = DemoMageId,
                        SkillCode = "mage-bolt",
                        UnlockedAt = now,
                        IsActive = true,
                    },
                ],
            },
        ];
}
