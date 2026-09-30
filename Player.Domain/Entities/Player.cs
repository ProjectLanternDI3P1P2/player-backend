namespace Player.Domain.Entities;

public sealed class Player
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public int NewGamePlusLevel { get; set; }
    public DateTimeOffset? NewGamePlusUpdatedAt { get; set; }
    public Guid? SelectedHeroId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AnonymizedAt { get; set; }
    public ICollection<Hero> Heroes { get; set; } = new List<Hero>();
    public ICollection<IdempotencyKey> IdempotencyKeys { get; set; } = new List<IdempotencyKey>();
    public ICollection<MatchmakingQueueEntry> QueueEntries { get; set; } =
        new List<MatchmakingQueueEntry>();
}
