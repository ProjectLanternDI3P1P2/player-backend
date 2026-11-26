using FluentAssertions;
using Player.Domain.Entities;
using Player.Domain.Services;

namespace Player.Test.Domain.Services;

public sealed class CombatantStatsCalculatorTests
{
    [Fact]
    public void Calculate_Warrior_AttacksWithStrength()
    {
        Hero hero = Hero("warrior", strength: 18, endurance: 16, agility: 9, intelligence: 7);

        CombatantStats stats = CombatantStatsCalculator.Calculate(hero, Class(60, 0));

        stats
            .Should()
            .Be(new CombatantStats(MaxHp: 156, MaxMana: 21, Attack: 18, Defense: 8, Speed: 9));
    }

    [Theory]
    [InlineData("mage")]
    [InlineData("shaman")]
    [InlineData("MAGE")]
    public void Calculate_Caster_AttacksWithIntelligence(string classCode)
    {
        Hero hero = Hero(classCode, strength: 6, endurance: 9, agility: 14, intelligence: 20);

        CombatantStats stats = CombatantStatsCalculator.Calculate(hero, Class(45, 0));

        stats.Attack.Should().Be(20);
        stats.MaxHp.Should().Be(99);
        stats.MaxMana.Should().Be(60);
        stats.Defense.Should().Be(4);
        stats.Speed.Should().Be(14);
    }

    [Fact]
    public void Calculate_ClassWithBaseMana_AddsIt()
    {
        Hero hero = Hero("mage", strength: 1, endurance: 1, agility: 1, intelligence: 10);

        CombatantStats stats = CombatantStatsCalculator.Calculate(hero, Class(45, 15));

        stats.MaxMana.Should().Be(45);
    }

    [Fact]
    public void Calculate_NegativeAttributes_NeverReturnsNegativeValues()
    {
        Hero hero = Hero("warrior", strength: -5, endurance: -20, agility: -3, intelligence: -4);

        CombatantStats stats = CombatantStatsCalculator.Calculate(hero, Class(10, 0));

        stats.MaxHp.Should().Be(1);
        stats.MaxMana.Should().Be(0);
        stats.Attack.Should().Be(0);
        stats.Defense.Should().Be(0);
        stats.Speed.Should().Be(0);
    }

    private static Hero Hero(
        string classCode,
        int strength,
        int endurance,
        int agility,
        int intelligence
    ) =>
        new()
        {
            ClassCode = classCode,
            Strength = strength,
            Endurance = endurance,
            Agility = agility,
            Intelligence = intelligence,
        };

    private static HeroClass Class(int baseHealth, int baseMana) =>
        new() { BaseHealth = baseHealth, BaseMana = baseMana };
}
