using Dungeon.Contracts.V1;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Player.Application.Ports;
using Player.Infrastructure.Grpc;
using Player.Infrastructure.Grpc.Clients;
using Player.Infrastructure.Grpc.Configuration;

namespace Player.Test.Infrastructure.Grpc;

public sealed class DungeonGrpcClientTests
{
    [Fact]
    public async Task StartRunAsync_SendsTheSessionAndParticipants_AndMapsTheRun()
    {
        Guid commandId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        Guid firstHeroId = Guid.NewGuid();
        Guid secondHeroId = Guid.NewGuid();
        Guid runId = Guid.NewGuid();
        const string seed = "A1B2C3D4E5F6G";
        var client = CreateClient(
            CreateCall(
                new CreateDungeonRunResponse { RunId = runId.ToString(), Seed = "A1B2C3D4E5F6G" }
            )
        );
        var options = Options.Create(new DungeonGrpcClientOptions { TimeoutSeconds = 5 });
        var dungeonClient = new DungeonGrpcClient(client.Object, options);

        DungeonRun run = await dungeonClient.StartRunAsync(
            commandId,
            sessionId,
            [new DungeonParticipant(firstHeroId), new DungeonParticipant(secondHeroId)],
            seed,
            TestContext.Current.CancellationToken
        );

        run.Should().Be(new DungeonRun(runId, "A1B2C3D4E5F6G"));
        client.Verify(
            service =>
                service.CreateDungeonRunAsync(
                    It.Is<CreateDungeonRunRequest>(request =>
                        request.CommandId == commandId.ToString()
                        && request.GameSessionId == sessionId.ToString()
                        && request.Seed == seed
                        && request.Participants.Count == 2
                        && request.Participants[0].HeroId == firstHeroId.ToString()
                        && request.Participants[1].HeroId == secondHeroId.ToString()
                    ),
                    It.Is<CallOptions>(callOptions =>
                        callOptions.CancellationToken == TestContext.Current.CancellationToken
                        && callOptions.Deadline.HasValue
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task StartRunAsync_WhenDungeonDeadlineExpires_ThrowsTimeoutException()
    {
        var client = CreateClient(
            CreateCall<CreateDungeonRunResponse>(
                Task.FromException<CreateDungeonRunResponse>(
                    new RpcException(new Status(StatusCode.DeadlineExceeded, "deadline"))
                )
            )
        );
        var dungeonClient = new DungeonGrpcClient(
            client.Object,
            Options.Create(new DungeonGrpcClientOptions())
        );

        Func<Task> action = () =>
            dungeonClient.StartRunAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                [new DungeonParticipant(Guid.NewGuid())],
                seed: null,
                cancellationToken: TestContext.Current.CancellationToken
            );

        await action
            .Should()
            .ThrowAsync<TimeoutException>()
            .WithMessage("Dungeon did not respond before the configured deadline.");
    }

    [Fact]
    public async Task StartRunAsync_WhenDungeonIsUnavailable_ThrowsHttpRequestException()
    {
        var client = CreateClient(
            CreateCall<CreateDungeonRunResponse>(
                Task.FromException<CreateDungeonRunResponse>(
                    new RpcException(new Status(StatusCode.Unavailable, "unavailable"))
                )
            )
        );
        var dungeonClient = new DungeonGrpcClient(
            client.Object,
            Options.Create(new DungeonGrpcClientOptions())
        );

        Func<Task> action = () =>
            dungeonClient.StartRunAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                [new DungeonParticipant(Guid.NewGuid())],
                seed: null,
                cancellationToken: TestContext.Current.CancellationToken
            );

        await action
            .Should()
            .ThrowAsync<HttpRequestException>()
            .WithMessage("Dungeon is unavailable.");
    }

    [Fact]
    public void AddGrpcConfiguration_RegistersTheDungeonClient()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddGrpcConfiguration(
                Configuration([
                    new KeyValuePair<string, string?>(
                        "Grpc:Dungeon:Address",
                        "http://dungeon:8081"
                    ),
                    new KeyValuePair<string, string?>("Grpc:Dungeon:TimeoutSeconds", "7"),
                ])
            )
            .BuildServiceProvider();

        provider.GetRequiredService<IDungeonClient>().Should().BeOfType<DungeonGrpcClient>();
        provider
            .GetRequiredService<IOptions<DungeonGrpcClientOptions>>()
            .Value.Should()
            .BeEquivalentTo(
                new DungeonGrpcClientOptions { Address = "http://dungeon:8081", TimeoutSeconds = 7 }
            );
    }

    [Theory]
    [InlineData("not-a-uri", "5")]
    [InlineData("http://dungeon:8081", "0")]
    public void AddGrpcConfiguration_WhenOptionsAreInvalid_Throws(
        string address,
        string timeoutSeconds
    )
    {
        Action action = () =>
            new ServiceCollection().AddGrpcConfiguration(
                Configuration([
                    new KeyValuePair<string, string?>("Grpc:Dungeon:Address", address),
                    new KeyValuePair<string, string?>(
                        "Grpc:Dungeon:TimeoutSeconds",
                        timeoutSeconds
                    ),
                ])
            );

        action.Should().Throw<InvalidOperationException>();
    }

    private static Mock<DungeonRunService.DungeonRunServiceClient> CreateClient(
        AsyncUnaryCall<CreateDungeonRunResponse> call
    )
    {
        var client = new Mock<DungeonRunService.DungeonRunServiceClient>(Mock.Of<CallInvoker>());
        client
            .Setup(service =>
                service.CreateDungeonRunAsync(
                    It.IsAny<CreateDungeonRunRequest>(),
                    It.IsAny<CallOptions>()
                )
            )
            .Returns(call);
        return client;
    }

    private static AsyncUnaryCall<TResponse> CreateCall<TResponse>(Task<TResponse> response) =>
        new(
            response,
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );

    private static AsyncUnaryCall<CreateDungeonRunResponse> CreateCall(
        CreateDungeonRunResponse response
    ) => CreateCall(Task.FromResult(response));

    private static IConfiguration Configuration(
        IEnumerable<KeyValuePair<string, string?>> settings
    ) => new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
}
