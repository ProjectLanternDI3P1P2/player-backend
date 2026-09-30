namespace Player.Domain.Entities;

public sealed class Hero
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Strength { get; set; }
    public int Endurance { get; set; }
    public int Agility { get; set; }
    public int Intelligence { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Player Player { get; set; } = null!;
    public HeroClass HeroClass { get; set; } = null!;
    public ICollection<HeroSkill> HeroSkills { get; set; } = new List<HeroSkill>();
    public ICollection<MatchmakingQueueEntry> QueueEntries { get; set; } =
        new List<MatchmakingQueueEntry>();
    public ICollection<GameSessionMember> SessionMembers { get; set; } =
        new List<GameSessionMember>();
}
