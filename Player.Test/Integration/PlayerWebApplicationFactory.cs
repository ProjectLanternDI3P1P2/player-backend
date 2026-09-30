using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Player.Application.Ports;
using Player.Infrastructure.Persistence;

namespace Player.Test.Integration;

public sealed class PlayerWebApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    private IDungeonClient dungeonClient = new UnavailableDungeonClient();

    public void SetDungeonClient(IDungeonClient client)
    {
        dungeonClient = client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = connectionString,
                    ["RabbitMq:Enabled"] = "false",
                }
            )
        );
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<PlayerDbContext>>();
            services.RemoveAll<PlayerDbContext>();
            services.RemoveAll<IOptions<DatabaseOptions>>();
            services.AddSingleton<IOptions<DatabaseOptions>>(
                Options.Create(new DatabaseOptions { DefaultConnection = connectionString })
            );
            services.AddDbContext<PlayerDbContext>(options => options.UseNpgsql(connectionString));
            services.RemoveAll<IDungeonClient>();
            services.AddSingleton<IDungeonClient>(new DelegatingDungeonClient(() => dungeonClient));
        });
    }

    private sealed class UnavailableDungeonClient : IDungeonClient
    {
        public Task<DungeonRun> StartRunAsync(
            Guid sessionId,
            Guid heroId,
            CancellationToken cancellationToken
        ) => Task.FromException<DungeonRun>(new HttpRequestException());
    }

    private sealed class DelegatingDungeonClient(Func<IDungeonClient> current) : IDungeonClient
    {
        public Task<DungeonRun> StartRunAsync(
            Guid sessionId,
            Guid heroId,
            CancellationToken cancellationToken
        ) => current().StartRunAsync(sessionId, heroId, cancellationToken);
    }
}
