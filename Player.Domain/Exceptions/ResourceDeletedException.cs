namespace Player.Domain.Exceptions;

/// <summary>
/// The resource existed but was soft-deleted. Mapped to HTTP 410 so that consumers can tell
/// a deliberate deletion apart from an unknown identifier (404).
/// </summary>
public sealed class ResourceDeletedException(
    string message,
    Guid resourceId,
    DateTimeOffset deletedAt
) : Exception(message)
{
    public Guid ResourceId { get; } = resourceId;
    public DateTimeOffset DeletedAt { get; } = deletedAt;
}
