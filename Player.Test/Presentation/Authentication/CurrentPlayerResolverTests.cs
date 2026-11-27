using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Player.Domain.Entities;
using Player.Domain.Services;
using Player.Infrastructure.Persistence;
using Player.Presentation.Authentication;

namespace Player.Test.Presentation.Authentication;

public sealed class CurrentPlayerResolverTests
{
    [Fact]
    public async Task ResolveAsync_UnknownUserWithUsername_CreatesPlayerWithGeneratedPublicHandle()
    {
        await using PlayerDbContext dbContext = CreateInMemoryDbContext();
        CurrentPlayerResolver resolver = CreateResolver(dbContext);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "kc-user-1";
        context.Request.Headers["X-Username"] = "Élodie";

        Guid id = await resolver.ResolveAsync(context, TestContext.Current.CancellationToken);

        var player = await dbContext.Players.SingleAsync(
            candidate => candidate.Id == id,
            TestContext.Current.CancellationToken
        );
        player.PublicHandle.Should().Be(PlayerPublicHandle.Create("Élodie", id));
        player.PublicHandle.Should().StartWith("elodie-");
    }

    [Fact]
    public async Task ResolveAsync_UnknownUserWithoutUsername_DerivesHandleFromFallbackDisplayName()
    {
        await using PlayerDbContext dbContext = CreateInMemoryDbContext();
        CurrentPlayerResolver resolver = CreateResolver(dbContext);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "kc-user-2";

        Guid id = await resolver.ResolveAsync(context, TestContext.Current.CancellationToken);

        var player = await dbContext.Players.SingleAsync(
            candidate => candidate.Id == id,
            TestContext.Current.CancellationToken
        );
        player
            .PublicHandle.Should()
            .Be(PlayerPublicHandle.Create($"Player-{id.ToString("N")[..8]}", id));
    }

    private static CurrentPlayerResolver CreateResolver(PlayerDbContext dbContext) =>
        new(
            dbContext,
            Mock.Of<IClock>(clock =>
                clock.UtcNow == new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero)
            )
        );

    private static PlayerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PlayerDbContext(options);
    }
}
