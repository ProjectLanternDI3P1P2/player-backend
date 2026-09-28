namespace Player.Domain.Entities;

public sealed class GameSessionTransition
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string SourceStatus { get; set; } = string.Empty;
    public string TargetStatus { get; set; } = string.Empty;
    public string Cause { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public GameSession GameSession { get; set; } = null!;
}
