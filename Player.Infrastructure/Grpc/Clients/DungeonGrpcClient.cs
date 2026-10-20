using Dungeon.Contracts.V1;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Player.Application.Ports;
using Player.Infrastructure.Grpc.Configuration;

namespace Player.Infrastructure.Grpc.Clients;

/// <summary>gRPC implementation of the application port used to start Dungeon-owned runs.</summary>
public sealed class DungeonGrpcClient(
    DungeonRunService.DungeonRunServiceClient client,
    IOptions<DungeonGrpcClientOptions> options
) : IDungeonClient
{
    public async Task<DungeonRun> StartRunAsync(
        Guid commandId,
        Guid sessionId,
        IReadOnlyList<DungeonParticipant> participants,
        string? seed,
        CancellationToken cancellationToken
    )
    {
        var request = new CreateDungeonRunRequest
        {
            CommandId = commandId.ToString(),
            GameSessionId = sessionId.ToString(),
            Seed = seed ?? string.Empty,
        };
        request.Participants.Add(
            participants.Select(participant => new DungeonRunParticipant
            {
                HeroId = participant.HeroId.ToString(),
            })
        );

        try
        {
            CreateDungeonRunResponse response = await client.CreateDungeonRunAsync(
                request,
                new CallOptions(
                    deadline: DateTime.UtcNow.AddSeconds(options.Value.TimeoutSeconds),
                    cancellationToken: cancellationToken
                )
            );

            return new DungeonRun(Guid.Parse(response.RunId), response.Seed);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.DeadlineExceeded)
        {
            throw new TimeoutException(
                "Dungeon did not respond before the configured deadline.",
                exception
            );
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Unavailable)
        {
            throw new HttpRequestException("Dungeon is unavailable.", exception);
        }
    }
}
