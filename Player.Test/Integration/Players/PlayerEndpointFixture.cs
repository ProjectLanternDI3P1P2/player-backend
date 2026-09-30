using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Player.Application.Ports;
using Player.Infrastructure.Persistence;

namespace Player.Test.Integration.Players;

public sealed class PlayerEndpointFixture : IAsyncLifetime
{
    private TestDatabase? database;
    private PlayerWebApplicationFactory? factory;

    public HttpClient HttpClient =>
        factory?.CreateClient() ?? throw new InvalidOperationException("Fixture not initialized.");

    public WebSocketClient CreateWebSocketClient() =>
        factory?.Server.CreateWebSocketClient()
        ?? throw new InvalidOperationException("Fixture not initialized.");

    public async Task SeedAsync(Func<PlayerDbContext, Task> seed)
    {
        if (factory is null)
            throw new InvalidOperationException("Fixture not initialized.");

        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        PlayerDbContext context = scope.ServiceProvider.GetRequiredService<PlayerDbContext>();
        await seed(context);
        await context.SaveChangesAsync();
    }

    public void SetDungeonClient(IDungeonClient client)
    {
        factory?.SetDungeonClient(client);
    }

    public async Task AssertAsync(Action<PlayerDbContext> assertion)
    {
        if (factory is null)
            throw new InvalidOperationException("Fixture not initialized.");

        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        PlayerDbContext context = scope.ServiceProvider.GetRequiredService<PlayerDbContext>();
        assertion(context);
    }

    public async ValueTask InitializeAsync()
    {
        database = await TestDatabase.CreateAsync($"player_test_players_{Guid.NewGuid():N}");
        factory = new PlayerWebApplicationFactory(database.ConnectionString);
        await database.ResetAsync();
    }

    public async ValueTask DisposeAsync()
    {
        factory?.Dispose();
        if (database is not null)
            await database.DisposeAsync();
    }
}
