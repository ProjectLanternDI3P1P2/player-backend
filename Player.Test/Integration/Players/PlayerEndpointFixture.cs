using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Player.Application.Ports;
using Player.Infrastructure.Persistence;

namespace Player.Test.Integration.Players;

public sealed class PlayerEndpointFixture : IAsyncLifetime
{
    private TestDatabase? database;
    private PlayerWebApplicationFactory? factory;
    private string? authenticatedExternalUserId;

    /// <summary>For tests that hold their own database locks next to the API.</summary>
    public string ConnectionString =>
        database?.ConnectionString
        ?? throw new InvalidOperationException("Fixture not initialized.");

    public HttpClient HttpClient
    {
        get
        {
            HttpClient client =
                factory?.CreateClient()
                ?? throw new InvalidOperationException("Fixture not initialized.");
            if (authenticatedExternalUserId is not null)
            {
                client.DefaultRequestHeaders.Add("X-User-Id", authenticatedExternalUserId);
            }

            return client;
        }
    }

    public WebSocketClient CreateWebSocketClient()
    {
        WebSocketClient client =
            factory?.Server.CreateWebSocketClient()
            ?? throw new InvalidOperationException("Fixture not initialized.");
        if (authenticatedExternalUserId is not null)
        {
            client.ConfigureRequest = request =>
                request.Headers["X-User-Id"] = authenticatedExternalUserId;
        }

        return client;
    }

    public void AuthenticateAs(Guid externalUserId)
    {
        authenticatedExternalUserId = externalUserId.ToString();
    }

    public async Task SeedAsync(Func<PlayerDbContext, Task> seed)
    {
        if (factory is null)
            throw new InvalidOperationException("Fixture not initialized.");

        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        PlayerDbContext context = scope.ServiceProvider.GetRequiredService<PlayerDbContext>();
        await seed(context);
        List<Player.Domain.Entities.Player> newPlayers = context
            .ChangeTracker.Entries<Player.Domain.Entities.Player>()
            .Select(entry => entry.Entity)
            .ToList();
        foreach (
            Player.Domain.Entities.Player player in newPlayers.Where(player =>
                player.ExternalUserId is null
            )
        )
        {
            player.ExternalUserId = player.Id.ToString();
        }

        authenticatedExternalUserId = newPlayers.LastOrDefault()?.ExternalUserId;
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
        authenticatedExternalUserId = Guid.NewGuid().ToString();
        await database.ResetAsync();
    }

    public async ValueTask DisposeAsync()
    {
        factory?.Dispose();
        if (database is not null)
            await database.DisposeAsync();
    }
}
