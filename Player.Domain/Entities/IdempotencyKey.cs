namespace Player.Domain.Entities;

public sealed class IdempotencyKey
{
    public Guid Key { get; set; }
    public Guid PlayerId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string RequestFingerprint { get; set; } = string.Empty;
    public Guid? ProducedResourceId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public Player Player { get; set; } = null!;
}
