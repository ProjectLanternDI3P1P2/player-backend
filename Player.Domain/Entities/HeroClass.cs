namespace Player.Domain.Entities;

public sealed class HeroClass
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BaseHealth { get; set; }
    public int BaseMana { get; set; }
    public ICollection<Hero> Heroes { get; set; } = new List<Hero>();
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
