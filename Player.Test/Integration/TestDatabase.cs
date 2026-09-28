using Microsoft.EntityFrameworkCore;
using Npgsql;
using Player.Infrastructure.Persistence;
using Respawn;

namespace Player.Test.Integration;

public sealed class TestDatabase : IAsyncDisposable
{
    private const string EnvironmentVariable = "PLAYER_TEST_DATABASE_CONNECTION";
    private readonly string databaseName;
    private readonly Respawner respawner;

    private TestDatabase(string databaseName, string connectionString, Respawner respawner)
    {
        this.databaseName = databaseName;
        ConnectionString = connectionString;
        this.respawner = respawner;
    }

    public string ConnectionString { get; }

    public static async Task<TestDatabase> CreateAsync(string databaseName)
    {
        string admin =
            Environment.GetEnvironmentVariable(EnvironmentVariable)
            ?? "Host=localhost;Port=5433;Database=player;Username=player";
        await using var connection = new NpgsqlConnection(admin);
        await connection.OpenAsync();
        await using var existsCommand = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @databaseName",
            connection
        );
        existsCommand.Parameters.AddWithValue("databaseName", databaseName);
        if (await existsCommand.ExecuteScalarAsync() is null)
        {
            await using var createCommand = new NpgsqlCommand(
                $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"")}\"",
                connection
            );
            await createCommand.ExecuteNonQueryAsync();
        }
        string connectionString = new NpgsqlConnectionStringBuilder(admin)
        {
            Database = databaseName,
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        await using (var context = new PlayerDbContext(options))
            await context.Database.MigrateAsync();
        await using var testConnection = new NpgsqlConnection(connectionString);
        await testConnection.OpenAsync();
        return new TestDatabase(
            databaseName,
            connectionString,
            await Respawner.CreateAsync(
                testConnection,
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres,
                    SchemasToInclude = ["public"],
                }
            )
        );
    }

    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(
            Environment.GetEnvironmentVariable(EnvironmentVariable)
                ?? "Host=localhost;Port=5433;Database=player;Username=player"
        );
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{databaseName.Replace("\"", "\"\"")}\" WITH (FORCE)",
            connection
        );
        await command.ExecuteNonQueryAsync();
    }
}
