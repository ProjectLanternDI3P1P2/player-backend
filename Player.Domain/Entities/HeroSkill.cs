namespace Player.Domain.Entities;

public sealed class HeroSkill
{
    public Guid HeroId { get; set; }
    public string SkillCode { get; set; } = string.Empty;
    public DateTimeOffset UnlockedAt { get; set; }
    public bool IsActive { get; set; }
    public Hero Hero { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
