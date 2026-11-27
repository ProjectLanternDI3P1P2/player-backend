using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Player.Application.Abstractions;
using Player.Application.Features.GameSessionUseCase;
using Player.Application.Features.GameSessionUseCase.CreateSoloLobby;
using Player.Domain.Entities;
using Player.Domain.Enums;
using Player.Domain.Exceptions;
using Player.Infrastructure.Persistence;
using Player.Infrastructure.Persistence.Configurations;
using Player.Infrastructure.Persistence.Repositories;
using Player.Infrastructure.PipelineBehavior;

namespace Player.Test.Integration.Players;

/// <summary>A hero takes part in a single active session, proven against PostgreSQL.</summary>
[Collection(PlayerEndpointCollection.Name)]
public sealed class SingleActiveSessionIntegrationTests(PlayerEndpointFixture fixture)
{
    [Fact]
    public async Task CreateSoloLobby_WhenTheHeroIsAlreadyInASession_IsRefusedWithTheOngoingSessionId()
    {
        (Guid playerId, Guid heroId) = await SeedHeroAsync();
        GameSessionSnapshot first = await CreateLobbyAsync(playerId, heroId);

        Func<Task> secondStart = () => CreateLobbyAsync(playerId, heroId);

        (await secondStart.Should().ThrowAsync<HeroAlreadyInSessionException>())
            .Which.ActiveSessionId.Should()
            .Be(first.SessionId);
    }

    [Fact]
    public async Task CreateSoloLobby_WithConcurrentRequestsForTheSameHero_CreatesExactlyOneSession()
    {
        (Guid playerId, Guid heroId) = await SeedHeroAsync();

        CreateLobbyOutcome[] outcomes = await Task.WhenAll(
            Enumerable
                .Range(0, 8)
                .Select(_ => Task.Run(() => TryCreateLobbyAsync(playerId, heroId)))
        );

        CreateLobbyOutcome[] created = outcomes
            .Where(outcome => outcome.Snapshot is not null)
            .ToArray();
        created.Should().ContainSingle();
        outcomes
            .Where(outcome => outcome.Snapshot is null)
            .Select(outcome => outcome.Failure)
            .Should()
            .AllSatisfy(failure =>
                failure
                    .Should()
                    .BeOfType<HeroAlreadyInSessionException>()
                    .Which.ActiveSessionId.Should()
                    .Be(created[0].Snapshot!.SessionId)
            );
        await fixture.AssertAsync(context =>
            context
                .GameSessionMembers.Count(member =>
                    member.HeroId == heroId && member.MemberStatus == GameSessionMemberStatus.Active
                )
                .Should()
                .Be(1)
        );
    }

    [Fact]
    public async Task Database_WhenAHeroBecomesActiveInASecondSession_RejectsItWithTheUniqueConstraint()
    {
        (Guid playerId, Guid heroId) = await SeedHeroAsync();
        await CreateLobbyAsync(playerId, heroId);

        Func<Task> action = () =>
            fixture.InScopeAsync(async services =>
            {
                PlayerDbContext context = services.GetRequiredService<PlayerDbContext>();
                context.GameSessions.Add(OpenSessionWithActiveHero(playerId, heroId));
                await context.SaveChangesAsync();
                return 0;
            });

        DbUpdateException exception = (await action.Should().ThrowAsync<DbUpdateException>()).Which;
        exception
            .InnerException.Should()
            .BeOfType<PostgresException>()
            .Which.ConstraintName.Should()
            .Be(GameSessionMemberConfiguration.ActiveHeroIndexName);
    }

    [Fact]
    public async Task CommandTransaction_WhenTheConstraintIsViolated_ReportsTheOngoingSession()
    {
        (Guid playerId, Guid heroId) = await SeedHeroAsync();
        GameSessionSnapshot first = await CreateLobbyAsync(playerId, heroId);

        Func<Task> action = () =>
            fixture.InScopeAsync(async services =>
            {
                PlayerDbContext context = services.GetRequiredService<PlayerDbContext>();
                var behavior = new CommandTransactionBehavior<RaceCommand, Unit>(context);
                return await behavior.Handle(
                    new RaceCommand(),
                    _ =>
                    {
                        context.GameSessions.Add(OpenSessionWithActiveHero(playerId, heroId));
                        return Task.FromResult(Unit.Value);
                    },
                    CancellationToken.None
                );
            });

        (await action.Should().ThrowAsync<HeroAlreadyInSessionException>())
            .Which.ActiveSessionId.Should()
            .Be(first.SessionId);
    }

    [Theory]
    [InlineData(GameSessionStatus.Completed, "Victory")]
    [InlineData(GameSessionStatus.Failed, "Defeat")]
    [InlineData(GameSessionStatus.Abandoned, "PlayerAbandoned")]
    [InlineData(GameSessionStatus.Failed, "DungeonFailure")]
    [InlineData(GameSessionStatus.Abandoned, "Repair")]
    public async Task CreateSoloLobby_AfterTheSessionReachedATerminalState_AcceptsTheHeroAgain(
        GameSessionStatus terminalStatus,
        string cause
    )
    {
        (Guid playerId, Guid heroId) = await SeedHeroAsync();
        GameSessionSnapshot first = await CreateLobbyAsync(playerId, heroId);
        await fixture.InScopeAsync(async services =>
        {
            PlayerDbContext context = services.GetRequiredService<PlayerDbContext>();
            GameSession session = await context
                .GameSessions.Include(x => x.Members)
                .SingleAsync(x => x.Id == first.SessionId);
            GameSessionTermination.Close(
                new GameSessionRepository(context),
                session,
                terminalStatus,
                cause,
                "system",
                DateTimeOffset.UtcNow
            );
            await context.SaveChangesAsync();
            return 0;
        });

        GameSessionSnapshot second = await CreateLobbyAsync(playerId, heroId);

        second.SessionId.Should().NotBe(first.SessionId);
    }

    private static GameSession OpenSessionWithActiveHero(Guid playerId, Guid heroId) =>
        new()
        {
            Id = Guid.NewGuid(),
            CreatorPlayerId = playerId,
            Status = GameSessionStatus.Lobby,
            Mode = "Solo",
            Members =
            [
                new GameSessionMember
                {
                    HeroId = heroId,
                    MemberStatus = GameSessionMemberStatus.Active,
                    JoinedAt = DateTimeOffset.UtcNow,
                },
            ],
        };

    private Task<GameSessionSnapshot> CreateLobbyAsync(Guid playerId, Guid heroId) =>
        fixture.InScopeAsync(services =>
            services
                .GetRequiredService<ISender>()
                .Send<GameSessionSnapshot>(
                    new CreateSoloLobbyCommand(playerId, heroId, null, Guid.NewGuid())
                )
        );

    private async Task<CreateLobbyOutcome> TryCreateLobbyAsync(Guid playerId, Guid heroId)
    {
        try
        {
            return new CreateLobbyOutcome(await CreateLobbyAsync(playerId, heroId), null);
        }
        catch (Exception exception)
        {
            return new CreateLobbyOutcome(null, exception);
        }
    }

    private async Task<(Guid PlayerId, Guid HeroId)> SeedHeroAsync()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        string classCode = $"single-session-{heroId:N}";
        await fixture.InScopeAsync(async services =>
        {
            PlayerDbContext context = services.GetRequiredService<PlayerDbContext>();
            context.Players.Add(
                new Player.Domain.Entities.Player
                {
                    Id = playerId,
                    DisplayName = "single-session-player",
                    AccountStatus = "Active",
                    CreatedAt = DateTimeOffset.UtcNow,
                }
            );
            context.HeroClasses.Add(
                new HeroClass
                {
                    Code = classCode,
                    Label = "Mage",
                    BaseHealth = 45,
                }
            );
            context.Heroes.Add(
                new Hero
                {
                    Id = heroId,
                    PlayerId = playerId,
                    ClassCode = classCode,
                    Name = "Merlin",
                    Level = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                }
            );
            await context.SaveChangesAsync();
            return 0;
        });
        return (playerId, heroId);
    }

    private sealed record CreateLobbyOutcome(GameSessionSnapshot? Snapshot, Exception? Failure);

    private sealed record RaceCommand : ICommand;
}
