using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Player.Domain.Entities;
using Player.Domain.Enums;
using Player.Infrastructure.Persistence;
using Player.Infrastructure.Persistence.Seeding;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Integration.Development;

public sealed class DevelopmentDatabaseEndpointTests : IAsyncLifetime
{
    private TestDatabase? database;
    private PlayerWebApplicationFactory? factory;

    [Fact]
    public async Task ResetAndSeed_WhenNotDevelopment_IsNotExposed()
    {
        using var testingFactory = new PlayerWebApplicationFactory(database!.ConnectionString);

        HttpResponseMessage response = await testingFactory
            .CreateClient()
            .PostAsync(
                "/api/player/v1/development/database/reset",
                null,
                TestContext.Current.CancellationToken
            );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetAndSeed_WhenDevelopment_TruncatesDataAndRestoresDemoDataset()
    {
        await using (AsyncServiceScope scope = factory!.Services.CreateAsyncScope())
        {
            PlayerDbContext context = scope.ServiceProvider.GetRequiredService<PlayerDbContext>();
            context.AddRange(
                new PlayerEntity
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "temporary-player",
                    AccountStatus = "Active",
                    CreatedAt = DateTimeOffset.UtcNow,
                },
                new GameSession
                {
                    Id = Guid.NewGuid(),
                    Status = GameSessionStatus.Lobby,
                    Mode = "Solo",
                }
            );
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        HttpResponseMessage response = await factory!
            .CreateClient()
            .PostAsync(
                "/api/player/v1/development/database/reset",
                null,
                TestContext.Current.CancellationToken
            );

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using AsyncServiceScope assertionScope = factory.Services.CreateAsyncScope();
        PlayerDbContext assertionContext =
            assertionScope.ServiceProvider.GetRequiredService<PlayerDbContext>();
        (await assertionContext.Players.CountAsync(TestContext.Current.CancellationToken))
            .Should()
            .Be(11);
        (await assertionContext.Heroes.CountAsync(TestContext.Current.CancellationToken))
            .Should()
            .Be(13);
        (await assertionContext.HeroSkills.CountAsync(TestContext.Current.CancellationToken))
            .Should()
            .Be(3);
        (await assertionContext.GameSessions.CountAsync(TestContext.Current.CancellationToken))
            .Should()
            .Be(0);
        (
            await assertionContext.Players.AnyAsync(
                player => player.DisplayName == "temporary-player",
                TestContext.Current.CancellationToken
            )
        )
            .Should()
            .BeFalse();
        (
            await assertionContext.Players.SingleAsync(
                player => player.Id == PlayerFakeDataGenerator.DemoPlayerId,
                TestContext.Current.CancellationToken
            )
        )
            .SelectedHeroId.Should()
            .Be(PlayerFakeDataGenerator.DemoMageId);
    }

    public async ValueTask InitializeAsync()
    {
        database = await TestDatabase.CreateAsync($"player_test_development_{Guid.NewGuid():N}");
        factory = new PlayerWebApplicationFactory(database.ConnectionString, "Development");
    }

    public async ValueTask DisposeAsync()
    {
        factory?.Dispose();
        if (database is not null)
            await database.DisposeAsync();
    }
}
