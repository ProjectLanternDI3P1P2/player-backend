namespace Player.Application.Ports;

/// <summary>Creates the Dungeon-owned run associated with a Player game session.</summary>
public interface IDungeonClient
{
    /// <summary>
    /// Idempotent per game session: calling it again returns the run Dungeon already started.
    /// </summary>
    /// <exception cref="DungeonUnavailableException">Dungeon is down or too slow to answer.</exception>
    Task<DungeonRun> StartRunAsync(
        Guid commandId,
        Guid sessionId,
        IReadOnlyList<DungeonParticipant> participants,
        CancellationToken cancellationToken
    );
}

public sealed record DungeonRun(Guid Id, string Seed);

public sealed record DungeonParticipant(Guid HeroId);
