namespace Player.Test.Integration.Players;

public sealed class PlayerEndpointFixture : IAsyncLifetime
{
    private TestDatabase? database;
    private PlayerWebApplicationFactory? factory;

    public HttpClient HttpClient =>
        factory?.CreateClient() ?? throw new InvalidOperationException("Fixture not initialized.");

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
