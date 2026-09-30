using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Player.Application.Ports;
using Player.Domain.Entities;
using Player.Infrastructure.Persistence;

namespace Player.Test.Integration.Players;

[Collection(PlayerEndpointCollection.Name)]
public sealed class PlayerHubIntegrationTests(PlayerEndpointFixture fixture)
{
    private const char RecordSeparator = '\u001e';
    private const string SignalRHeroClassCode = "signalr-lobby-mage";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Queue<JsonDocument> receivedMessages = new();

    [Fact]
    public async Task SignalRNegotiate_Preflight_AllowsConfiguredBrowserOrigin()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/hubs/player/negotiate?negotiateVersion=1"
        );
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        HttpResponseMessage response = await fixture.HttpClient.SendAsync(
            request,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response
            .Headers.GetValues("Access-Control-Allow-Origin")
            .Should()
            .ContainSingle("http://localhost:3000");
    }

    [Fact]
    public async Task LobbyLifecycle_OverSignalR_CreatesLobbyThenStartsTheDungeonRun()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid dungeonRunId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedHeroAsync(context, playerId, heroId));
        using WebSocket socket = await ConnectAsync();

        JsonElement lobby = await InvokeAndReadBroadcastAsync(
            socket,
            "create-lobby",
            "CreateSoloLobby",
            new
            {
                commandId = Guid.NewGuid(),
                playerId,
                heroId,
            }
        );
        lobby.GetProperty("state").GetString().Should().Be("Lobby");
        lobby.GetProperty("dungeonRunId").ValueKind.Should().Be(JsonValueKind.Null);
        lobby.GetProperty("creatorPlayerId").GetGuid().Should().Be(playerId);
        lobby.GetProperty("members").GetArrayLength().Should().Be(1);
        Guid sessionId = lobby.GetProperty("sessionId").GetGuid();

        fixture.SetDungeonClient(
            new StubDungeonClient(new DungeonRun(dungeonRunId, "dungeon-seed"))
        );
        JsonElement started = await InvokeAndReadBroadcastAsync(
            socket,
            "start-session",
            "StartSession",
            new
            {
                commandId = Guid.NewGuid(),
                playerId,
                sessionId,
            }
        );
        started.GetProperty("state").GetString().Should().Be("Active");
        started.GetProperty("dungeonRunId").GetGuid().Should().Be(dungeonRunId);
    }

    [Fact]
    public async Task CreateSoloLobby_WhenTheHeroDoesNotExist_ReturnsTheGenericCommandRejection()
    {
        using WebSocket socket = await ConnectAsync();
        await SendAsync(
            socket,
            new
            {
                type = 1,
                invocationId = "missing-hero",
                target = "CreateSoloLobby",
                arguments = new[]
                {
                    new
                    {
                        commandId = Guid.NewGuid(),
                        playerId = Guid.NewGuid(),
                        heroId = Guid.NewGuid(),
                    },
                },
            }
        );
        using JsonDocument rejection = await ReceiveAsync(socket);
        JsonElement result = rejection.RootElement.GetProperty("result");
        result.GetProperty("accepted").GetBoolean().Should().BeFalse();
        result
            .GetProperty("error")
            .GetProperty("code")
            .GetString()
            .Should()
            .Be("RESOURCE_NOT_FOUND");
    }

    private async Task<WebSocket> ConnectAsync()
    {
        string connectionToken = await NegotiateAsync();
        WebSocket socket = await fixture
            .CreateWebSocketClient()
            .ConnectAsync(
                new Uri($"ws://localhost/hubs/player?id={Uri.EscapeDataString(connectionToken)}"),
                TestContext.Current.CancellationToken
            );
        await SendAsync(socket, new { protocol = "json", version = 1 });
        using JsonDocument handshake = await ReceiveAsync(socket);
        handshake.RootElement.TryGetProperty("error", out _).Should().BeFalse();
        return socket;
    }

    private async Task<JsonElement> InvokeAndReadBroadcastAsync(
        WebSocket socket,
        string invocationId,
        string target,
        object command
    )
    {
        await SendAsync(
            socket,
            new
            {
                type = 1,
                invocationId,
                target,
                arguments = new[] { command },
            }
        );
        using JsonDocument first = await ReceiveAsync(socket);
        using JsonDocument second = await ReceiveAsync(socket);
        JsonDocument broadcast = new[] { first, second }.Single(message =>
            message.RootElement.GetProperty("type").GetInt32() == 1
        );
        return broadcast.RootElement.GetProperty("arguments")[0].Clone();
    }

    private async Task<string> NegotiateAsync()
    {
        HttpResponseMessage response = await fixture.HttpClient.PostAsync(
            "/hubs/player/negotiate?negotiateVersion=1",
            null,
            TestContext.Current.CancellationToken
        );
        response.EnsureSuccessStatusCode();
        using JsonDocument negotiation = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)
        );
        return negotiation.RootElement.GetProperty("connectionToken").GetString()!;
    }

    private static async Task SendAsync(WebSocket socket, object message)
    {
        byte[] payload = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message, JsonOptions) + RecordSeparator
        );
        await socket.SendAsync(
            payload,
            WebSocketMessageType.Text,
            true,
            TestContext.Current.CancellationToken
        );
    }

    private async Task<JsonDocument> ReceiveAsync(WebSocket socket)
    {
        if (receivedMessages.TryDequeue(out JsonDocument? buffered))
            return buffered;
        byte[] buffer = new byte[4096];
        WebSocketReceiveResult received = await socket.ReceiveAsync(
            buffer,
            TestContext.Current.CancellationToken
        );
        received.EndOfMessage.Should().BeTrue();
        foreach (
            string payload in Encoding
                .UTF8.GetString(buffer, 0, received.Count)
                .Split(RecordSeparator)
                .Where(payload => !string.IsNullOrWhiteSpace(payload))
        )
            receivedMessages.Enqueue(JsonDocument.Parse(payload));
        return receivedMessages.Dequeue();
    }

    private static Task SeedHeroAsync(PlayerDbContext context, Guid playerId, Guid heroId)
    {
        context.Players.Add(
            new Player.Domain.Entities.Player
            {
                Id = playerId,
                DisplayName = "test-player",
                AccountStatus = "Active",
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        context.HeroClasses.Add(
            new HeroClass
            {
                Code = SignalRHeroClassCode,
                Label = "Mage",
                BaseHealth = 45,
            }
        );
        context.Heroes.Add(
            new Hero
            {
                Id = heroId,
                PlayerId = playerId,
                ClassCode = SignalRHeroClassCode,
                Name = "Merlin",
                Level = 1,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        return Task.CompletedTask;
    }

    private sealed class StubDungeonClient(DungeonRun run) : IDungeonClient
    {
        public Task<DungeonRun> StartRunAsync(
            Guid commandId,
            Guid sessionId,
            IReadOnlyList<DungeonParticipant> participants,
            CancellationToken cancellationToken
        ) => Task.FromResult(run);
    }
}
