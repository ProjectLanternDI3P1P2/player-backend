namespace Player.Domain.Entities;

public sealed class MatchmakingQueueEntry
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid HeroId { get; set; }
    public Guid? GroupId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int NewGamePlusLevelAtEntry { get; set; }
    public DateTimeOffset EnteredAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }
    public Player Player { get; set; } = null!;
    public Hero Hero { get; set; } = null!;
    public MatchmakingGroup? Group { get; set; }
}
