using Dungeon.Contracts.V1;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Player.Application.Ports;
using Player.Domain.Exceptions;
using Player.Infrastructure.Grpc.Configuration;

namespace Player.Infrastructure.Grpc.Clients;

/// <summary>
/// gRPC implementation of <see cref="IDungeonClient"/>, on the contract Dungeon publishes in
/// Dungeon.Contracts. Protobuf and gRPC types stay here: handlers only see the port.
/// </summary>
public sealed class DungeonGrpcClient(
    DungeonRunService.DungeonRunServiceClient client,
    IOptions<DungeonGrpcClientOptions> options
) : IDungeonClient
{
    public async Task<DungeonRun> StartRunAsync(
        Guid commandId,
        Guid sessionId,
        IReadOnlyList<DungeonParticipant> participants,
        CancellationToken cancellationToken
    )
    {
        var request = new CreateDungeonRunRequest
        {
            CommandId = commandId.ToString(),
            GameSessionId = sessionId.ToString(),
        };
        request.Participants.AddRange(
            participants.Select(participant => new DungeonRunParticipant
            {
                HeroId = participant.HeroId.ToString(),
            })
        );

        try
        {
            CreateDungeonRunResponse run = await client.CreateDungeonRunAsync(
                request,
                new CallOptions(
                    deadline: DateTime.UtcNow.AddSeconds(options.Value.TimeoutSeconds),
                    cancellationToken: cancellationToken
                )
            );

            return new DungeonRun(Guid.Parse(run.RunId), run.Seed);
        }
        catch (RpcException exception)
            when (exception.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded)
        {
            throw new DungeonUnavailableException(
                $"Dungeon did not start the run of game session '{sessionId}': {exception.Status.Detail}",
                exception
            );
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.AlreadyExists)
        {
            throw new ConflictException(
                "This command already started the dungeon run of another game session."
            );
        }
    }
}
