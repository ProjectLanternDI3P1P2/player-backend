namespace Player.Domain.Exceptions;

/// <summary>
/// A hero can take part in only one active session. Carries the ongoing session so that the
/// client can reconnect to it instead of starting a second game.
/// </summary>
public sealed class HeroAlreadyInSessionException(Guid heroId, Guid activeSessionId)
    : ConflictException($"Hero '{heroId}' already takes part in session '{activeSessionId}'.")
{
    public Guid HeroId { get; } = heroId;
    public Guid ActiveSessionId { get; } = activeSessionId;
}
