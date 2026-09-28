using FluentAssertions;
using Grpc.Net.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Player.Contracts.V1;
using Player.Infrastructure.Persistence;
using PlayerEntity = global::Player.Domain.Entities.Player;

namespace Player.Test.Integration.Players;

public sealed class PlayerGrpcServiceIntegrationTests : IAsyncLifetime
{
    private TestDatabase? database;
    private PlayerWebApplicationFactory? factory;
    private GrpcChannel? channel;

    public async ValueTask InitializeAsync()
    {
        database = await TestDatabase.CreateAsync($"player_test_grpc_{Guid.NewGuid():N}");
        factory = new PlayerWebApplicationFactory(database.ConnectionString);
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        PlayerDbContext context = scope.ServiceProvider.GetRequiredService<PlayerDbContext>();
        context.Players.Add(
            new PlayerEntity
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Ari",
                Health = 100,
                MaxHealth = 100,
                Attack = 12,
            }
        );
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPlayer_ExistingPlayer_ReturnsContractResponse()
    {
        TestServer server =
            factory?.Server ?? throw new InvalidOperationException("Fixture not initialized.");
        channel = GrpcChannel.ForAddress(
            "http://localhost",
            new GrpcChannelOptions { HttpHandler = server.CreateHandler() }
        );
        var client = new PlayerService.PlayerServiceClient(channel);

        GetPlayerResponse response = await client.GetPlayerAsync(
            new GetPlayerRequest { PlayerId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        response.Name.Should().Be("Ari");
        response.Attack.Should().Be(12);
    }

    public async ValueTask DisposeAsync()
    {
        channel?.Dispose();
        factory?.Dispose();
        if (database is not null)
            await database.DisposeAsync();
    }
}
