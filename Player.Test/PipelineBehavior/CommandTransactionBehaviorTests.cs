using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.Application.Abstractions;
using Player.Infrastructure.Persistence;
using Player.Infrastructure.PipelineBehavior;
using PlayerEntity = global::Player.Domain.Entities.Player;

namespace Player.Test.PipelineBehavior;

public sealed class CommandTransactionBehaviorTests
{
    [Fact]
    public async Task Handle_CommandSucceeds_CommitsChanges()
    {
        string databaseName = Guid.NewGuid().ToString();
        await using var dbContext = CreateInMemoryDbContext(databaseName);
        var behavior = new CommandTransactionBehavior<CreatePlayerCommand, Unit>(dbContext);
        var player = new PlayerEntity
        {
            Id = Guid.NewGuid(),
            DisplayName = "Committed",
            AccountStatus = "Active",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await behavior.Handle(
            new CreatePlayerCommand(player),
            _ =>
            {
                dbContext.Players.Add(player);
                return Task.FromResult(Unit.Value);
            },
            TestContext.Current.CancellationToken
        );

        result.Should().Be(Unit.Value);
        await using var verificationContext = CreateInMemoryDbContext(databaseName);
        (
            await verificationContext.Players.FindAsync(
                [player.Id],
                TestContext.Current.CancellationToken
            )
        )
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task Handle_CommandFails_DoesNotCommitChanges()
    {
        string databaseName = Guid.NewGuid().ToString();
        await using var dbContext = CreateInMemoryDbContext(databaseName);
        var behavior = new CommandTransactionBehavior<CreatePlayerCommand, Unit>(dbContext);
        var player = new PlayerEntity
        {
            Id = Guid.NewGuid(),
            DisplayName = "Not committed",
            AccountStatus = "Active",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        Func<Task> action = async () =>
            await behavior.Handle(
                new CreatePlayerCommand(player),
                _ =>
                {
                    dbContext.Players.Add(player);
                    return Task.FromException<Unit>(
                        new InvalidOperationException("Handler failed.")
                    );
                },
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<InvalidOperationException>();
        await using var verificationContext = CreateInMemoryDbContext(databaseName);
        (
            await verificationContext.Players.FindAsync(
                [player.Id],
                TestContext.Current.CancellationToken
            )
        )
            .Should()
            .BeNull();
    }

    private static PlayerDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new PlayerDbContext(options);
    }

    private sealed record CreatePlayerCommand(PlayerEntity Player) : ICommand;
}
