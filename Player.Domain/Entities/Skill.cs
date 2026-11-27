namespace Player.Domain.Entities;

public sealed class Skill
{
    public string Code { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
    public string TargetingType { get; set; } = string.Empty;

    /// <summary>Mana spent each time the ability is used in a fight.</summary>
    public int ManaCost { get; set; }

    /// <summary>
    /// Percentage of the caster's attack for an offensive ability, or of the target's maximum
    /// hit points for a healing one.
    /// </summary>
    public int Power { get; set; } = 100;
    public HeroClass HeroClass { get; set; } = null!;
    public ICollection<HeroSkill> HeroSkills { get; set; } = new List<HeroSkill>();
}
