namespace Player.Infrastructure.Grpc.Configuration;

/// <summary>Where the Dungeon service listens for internal gRPC calls (ADR-GLOB-008).</summary>
public sealed class DungeonGrpcClientOptions
{
    public const string SectionName = "Grpc:Dungeon";

    public string Address { get; init; } = "http://localhost:8081";

    /// <summary>Deadline of every call (ADR-GLOB-006).</summary>
    public int TimeoutSeconds { get; init; } = 2;
}
