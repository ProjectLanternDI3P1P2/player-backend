namespace Player.Domain.Entities;

public sealed class MatchmakingGroup
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MinNewGamePlusLevel { get; set; }
    public int MaxNewGamePlusLevel { get; set; }
    public DateTimeOffset FormedAt { get; set; }
    public ICollection<MatchmakingQueueEntry> QueueEntries { get; set; } =
        new List<MatchmakingQueueEntry>();
    public GameSession? GameSession { get; set; }
}
