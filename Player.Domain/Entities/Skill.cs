namespace Player.Domain.Entities;

public sealed class Skill
{
    public string Code { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
    public string TargetingType { get; set; } = string.Empty;
    public HeroClass HeroClass { get; set; } = null!;
    public ICollection<HeroSkill> HeroSkills { get; set; } = new List<HeroSkill>();
}
