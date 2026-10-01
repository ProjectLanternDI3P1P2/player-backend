namespace Player.Domain.Entities;

public sealed class GameSession
{
    public Guid Id { get; set; }
    public Guid CreatorPlayerId { get; set; }
    public Guid? MatchmakingGroupId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public Guid? DungeonRunId { get; set; }
    public string? DungeonSeed { get; set; }
    public bool ProgressionRecorded { get; set; }
    public string? TerminationReason { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? LastActiveAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public MatchmakingGroup? MatchmakingGroup { get; set; }
    public ICollection<GameSessionMember> Members { get; set; } = new List<GameSessionMember>();
    public ICollection<GameSessionTransition> Transitions { get; set; } =
        new List<GameSessionTransition>();
}
