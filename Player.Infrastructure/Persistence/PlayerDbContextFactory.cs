using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Player.Infrastructure.Persistence;

/// <summary>Creates the context for EF Core commands without starting the HTTP application.</summary>
public sealed class PlayerDbContextFactory : IDesignTimeDbContextFactory<PlayerDbContext>
{
    public PlayerDbContext CreateDbContext(string[] args)
    {
        string configurationDirectory = FindPresentationConfigurationDirectory();
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(configurationDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required to create EF Core migrations."
            );

        DbContextOptions<PlayerDbContext> options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PlayerDbContext(options);
    }

    private static string FindPresentationConfigurationDirectory()
    {
        for (
            DirectoryInfo? directory = new(Directory.GetCurrentDirectory());
            directory is not null;
            directory = directory.Parent
        )
        {
            string presentationDirectory = Path.Combine(directory.FullName, "Player.Presentation");
            if (File.Exists(Path.Combine(presentationDirectory, "appsettings.json")))
            {
                return presentationDirectory;
            }
        }

        throw new InvalidOperationException(
            "Could not find Player.Presentation/appsettings.json from the current directory."
        );
    }
}
