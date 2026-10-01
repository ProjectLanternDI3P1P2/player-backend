using System.Security.Cryptography;
using System.Text;
using Player.Application.Ports;

namespace Player.Infrastructure.Grpc.Clients;

/// <summary>
/// Temporary gRPC-client substitute used until Dungeon publishes Dungeon.Contracts.
/// It deliberately performs no HTTP call and preserves a stable result for a given
/// Player session, mirroring CreateDungeonRun's idempotent contract.
/// </summary>
public sealed class MockDungeonGrpcClient : IDungeonClient
{
    public Task<DungeonRun> StartRunAsync(
        Guid commandId,
        Guid sessionId,
        IReadOnlyList<DungeonParticipant> participants,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"dungeon-run:{sessionId:N}"));
        var runId = new Guid(hash[..16]);
        string seed = $"mock-dungeon-{sessionId:N}";

        return Task.FromResult(new DungeonRun(runId, seed));
    }
}
