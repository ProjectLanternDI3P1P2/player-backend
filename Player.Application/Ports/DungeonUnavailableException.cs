namespace Player.Application.Ports;

/// <summary>
/// Dungeon could not be reached or did not answer in time. Transient: the same command can
/// be sent again, Dungeon creates at most one run per game session.
/// </summary>
public sealed class DungeonUnavailableException(string message, Exception innerException)
    : Exception(message, innerException);
