using Dungeon.Contracts.V1;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Moq;
using Player.Application.Ports;
using Player.Domain.Exceptions;
using Player.Infrastructure.Grpc.Clients;
using Player.Infrastructure.Grpc.Configuration;

namespace Player.Test.Infrastructure.Grpc;

public sealed class DungeonGrpcClientTests
{
    private static readonly Guid CommandId = Guid.NewGuid();
    private static readonly Guid SessionId = Guid.NewGuid();
    private static readonly Guid HeroId = Guid.NewGuid();
    private static readonly Guid RunId = Guid.NewGuid();

    private readonly Mock<DungeonRunService.DungeonRunServiceClient> _grpcClient = new();
    private readonly DungeonGrpcClient _client;
    private CreateDungeonRunRequest? _sentRequest;
    private CallOptions _sentOptions;

    public DungeonGrpcClientTests()
    {
        _client = new DungeonGrpcClient(
            _grpcClient.Object,
            Options.Create(new DungeonGrpcClientOptions { TimeoutSeconds = 3 })
        );
    }

    [Fact]
    public async Task StartRunAsync_DungeonStartsTheRun_SendsTheSessionAndItsHeroes()
    {
        // Arrange
        Answer(
            Task.FromResult(
                new CreateDungeonRunResponse { RunId = RunId.ToString(), Seed = "0KX4M2T9QZ7PA" }
            )
        );
        DateTime before = DateTime.UtcNow;

        // Act
        DungeonRun run = await StartRunAsync();

        // Assert
        run.Should().Be(new DungeonRun(RunId, "0KX4M2T9QZ7PA"));
        _sentRequest!.CommandId.Should().Be(CommandId.ToString());
        _sentRequest.GameSessionId.Should().Be(SessionId.ToString());
        _sentRequest
            .Participants.Select(participant => participant.HeroId)
            .Should()
            .Equal(HeroId.ToString());
        _sentRequest.Seed.Should().BeEmpty();
        _sentOptions.Deadline.Should().BeOnOrAfter(before.AddSeconds(3));
        _sentOptions.Deadline.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(3));
    }

    [Theory]
    [InlineData(StatusCode.Unavailable)]
    [InlineData(StatusCode.DeadlineExceeded)]
    public async Task StartRunAsync_DungeonDownOrTooSlow_IsUnavailable(StatusCode statusCode)
    {
        // Arrange
        Answer(Task.FromException<CreateDungeonRunResponse>(Failure(statusCode)));

        // Act
        Func<Task> act = StartRunAsync;

        // Assert
        (await act.Should().ThrowAsync<DungeonUnavailableException>())
            .WithInnerException<RpcException>();
    }

    [Fact]
    public async Task StartRunAsync_CommandAlreadyUsedForAnotherSession_IsAConflict()
    {
        // Arrange
        Answer(Task.FromException<CreateDungeonRunResponse>(Failure(StatusCode.AlreadyExists)));

        // Act
        Func<Task> act = StartRunAsync;

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task StartRunAsync_RequestRefused_LetsTheFailureThrough()
    {
        // Arrange: a malformed request is a bug, not a transient failure.
        Answer(Task.FromException<CreateDungeonRunResponse>(Failure(StatusCode.InvalidArgument)));

        // Act
        Func<Task> act = StartRunAsync;

        // Assert
        (await act.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.InvalidArgument);
    }

    private Task<DungeonRun> StartRunAsync() =>
        _client.StartRunAsync(
            CommandId,
            SessionId,
            [new DungeonParticipant(HeroId)],
            TestContext.Current.CancellationToken
        );

    private void Answer(Task<CreateDungeonRunResponse> response)
    {
        _grpcClient
            .Setup(client =>
                client.CreateDungeonRunAsync(
                    It.IsAny<CreateDungeonRunRequest>(),
                    It.IsAny<CallOptions>()
                )
            )
            .Callback<CreateDungeonRunRequest, CallOptions>(
                (request, options) =>
                {
                    _sentRequest = request;
                    _sentOptions = options;
                }
            )
            .Returns(
                new AsyncUnaryCall<CreateDungeonRunResponse>(
                    response,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                )
            );
    }

    private static RpcException Failure(StatusCode statusCode) =>
        new(new Status(statusCode, "test"));
}
