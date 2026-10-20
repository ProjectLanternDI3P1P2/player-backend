using Player.Domain.Enums;

namespace Player.Domain.Entities;

public sealed class GameSessionTransition
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public GameSessionStatus SourceStatus { get; set; }
    public GameSessionStatus TargetStatus { get; set; }
    public string Cause { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public GameSession GameSession { get; set; } = null!;
}
