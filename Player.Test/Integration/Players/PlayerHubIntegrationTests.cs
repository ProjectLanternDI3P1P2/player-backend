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
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreateSession_OverSignalR_AcknowledgesAndBroadcastsTheAuthoritativeSnapshot()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid dungeonRunId = Guid.NewGuid();
        await fixture.SeedAsync(context => SeedHeroAsync(context, playerId, heroId));
        fixture.SetDungeonClient(
            new StubDungeonClient(new DungeonRun(dungeonRunId, "dungeon-seed"))
        );

        string connectionToken = await NegotiateAsync();
        using WebSocket socket = await fixture
            .CreateWebSocketClient()
            .ConnectAsync(
                new Uri($"ws://localhost/hubs/player?id={Uri.EscapeDataString(connectionToken)}"),
                TestContext.Current.CancellationToken
            );
        await SendAsync(socket, new { protocol = "json", version = 1 });
        using JsonDocument handshake = await ReceiveAsync(socket);
        handshake.RootElement.TryGetProperty("error", out _).Should().BeFalse();

        Guid commandId = Guid.NewGuid();
        await SendAsync(
            socket,
            new
            {
                type = 1,
                invocationId = "create-session",
                target = "CreateSession",
                arguments = new[]
                {
                    new
                    {
                        commandId,
                        playerId,
                        heroId,
                    },
                },
            }
        );

        JsonDocument first = await ReceiveAsync(socket);
        JsonDocument second = await ReceiveAsync(socket);
        JsonElement stateChanged = new[] { first, second }
            .Select(message => message.RootElement)
            .Single(message => message.GetProperty("type").GetInt32() == 1);
        JsonElement acknowledgement = new[] { first, second }
            .Select(message => message.RootElement)
            .Single(message => message.GetProperty("type").GetInt32() == 3);

        JsonElement state = stateChanged.GetProperty("arguments")[0];
        acknowledgement
            .GetProperty("result")
            .GetProperty("accepted")
            .GetBoolean()
            .Should()
            .BeTrue();
        acknowledgement
            .GetProperty("result")
            .GetProperty("session")
            .GetProperty("sessionId")
            .GetGuid()
            .Should()
            .Be(state.GetProperty("sessionId").GetGuid());
        state.GetProperty("hero").GetProperty("id").GetGuid().Should().Be(heroId);
        state.GetProperty("state").GetString().Should().Be("Active");
        state.GetProperty("dungeonRunId").GetGuid().Should().Be(dungeonRunId);
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

    private static async Task<JsonDocument> ReceiveAsync(WebSocket socket)
    {
        byte[] buffer = new byte[4096];
        WebSocketReceiveResult received = await socket.ReceiveAsync(
            buffer,
            TestContext.Current.CancellationToken
        );
        received.EndOfMessage.Should().BeTrue();
        return JsonDocument.Parse(
            Encoding.UTF8.GetString(buffer, 0, received.Count).TrimEnd(RecordSeparator)
        );
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
                Code = "mage",
                Label = "Mage",
                BaseHealth = 45,
            }
        );
        context.Heroes.Add(
            new Hero
            {
                Id = heroId,
                PlayerId = playerId,
                ClassCode = "mage",
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
            Guid sessionId,
            Guid heroId,
            CancellationToken cancellationToken
        ) => Task.FromResult(run);
    }
}
