namespace Player.Application.Ports;

/// <summary>Creates the Dungeon-owned run associated with a Player game session.</summary>
public interface IDungeonClient
{
    Task<DungeonRun> StartRunAsync(
        Guid sessionId,
        Guid heroId,
        CancellationToken cancellationToken
    );
}

public sealed record DungeonRun(Guid Id, string Seed);
