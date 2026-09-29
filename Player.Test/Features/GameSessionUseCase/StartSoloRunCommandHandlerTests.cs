using FluentAssertions;
using Moq;
using Player.Application.Features.GameSessionUseCase.StartSoloRun;
using Player.Application.Ports;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;

namespace Player.Test.Features.GameSessionUseCase;

public sealed class StartSoloRunCommandHandlerTests
{
    [Fact]
    public async Task Handle_AvailableDungeon_CreatesAnActiveSessionWithTheDungeonReference()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid dungeonRunId = Guid.NewGuid();
        var repository = CreateRepository(playerId, heroId);
        var dungeon = new Mock<IDungeonClient>();
        dungeon
            .Setup(x => x.StartRunAsync(It.IsAny<Guid>(), heroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DungeonRun(dungeonRunId, "dungeon-seed"));
        var handler = new StartSoloRunCommandHandler(
            repository.Object,
            dungeon.Object,
            new FixedClock()
        );

        StartSoloRunResult result = await handler.Handle(
            new StartSoloRunCommand(playerId, heroId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result
            .Should()
            .BeEquivalentTo(
                new
                {
                    Hero = new { Id = heroId },
                    State = "Active",
                    DungeonRunId = dungeonRunId,
                    DungeonSeed = "dungeon-seed",
                    FailureReason = (string?)null,
                    AlreadyExists = false,
                }
            );
        repository.Verify(
            x =>
                x.Add(
                    It.Is<GameSession>(session =>
                        session.Mode == "Solo"
                        && session.Members.Single().HeroId == heroId
                        && session.Transitions.Count == 2
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_UnavailableDungeon_CreatesAnExplicitFailedSession()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        var repository = CreateRepository(playerId, heroId);
        var dungeon = new Mock<IDungeonClient>();
        dungeon
            .Setup(x => x.StartRunAsync(It.IsAny<Guid>(), heroId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());
        var handler = new StartSoloRunCommandHandler(
            repository.Object,
            dungeon.Object,
            new FixedClock()
        );

        StartSoloRunResult result = await handler.Handle(
            new StartSoloRunCommand(playerId, heroId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.State.Should().Be("Failed");
        result.FailureReason.Should().Be("DungeonUnavailable");
        result.DungeonRunId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_HeroWithAnActiveSession_RejectsTheRequest()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        var repository = CreateRepository(playerId, heroId, hasActiveSession: true);
        var handler = new StartSoloRunCommandHandler(
            repository.Object,
            Mock.Of<IDungeonClient>(),
            new FixedClock()
        );

        Func<Task> action = () =>
            handler.Handle(
                new StartSoloRunCommand(playerId, heroId, Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ReplayedRequest_ReturnsTheRecordedSession()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        Guid idempotencyKey = Guid.NewGuid();
        var repository = new Mock<IGameSessionRepository>();
        repository
            .Setup(x => x.GetIdempotencyKeyAsync(idempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new IdempotencyKey
                {
                    Key = idempotencyKey,
                    PlayerId = playerId,
                    Scope = "game-session.start-solo",
                    RequestFingerprint = Fingerprint(heroId),
                    ProducedResourceId = sessionId,
                }
            );
        repository
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new GameSession
                {
                    Id = sessionId,
                    Status = "Active",
                    Mode = "Solo",
                    DungeonRunId = Guid.NewGuid(),
                    DungeonSeed = "seed",
                    Members =
                    [
                        new GameSessionMember
                        {
                            HeroId = heroId,
                            Hero = new Hero
                            {
                                Id = heroId,
                                Name = "Merlin",
                                ClassCode = "mage",
                                Level = 1,
                            },
                        },
                    ],
                }
            );
        var handler = new StartSoloRunCommandHandler(
            repository.Object,
            Mock.Of<IDungeonClient>(),
            new FixedClock()
        );

        StartSoloRunResult result = await handler.Handle(
            new StartSoloRunCommand(playerId, heroId, idempotencyKey),
            TestContext.Current.CancellationToken
        );

        result.SessionId.Should().Be(sessionId);
        result.AlreadyExists.Should().BeTrue();
    }

    private static Mock<IGameSessionRepository> CreateRepository(
        Guid playerId,
        Guid heroId,
        bool hasActiveSession = false
    )
    {
        var repository = new Mock<IGameSessionRepository>();
        repository
            .Setup(x => x.GetIdempotencyKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyKey?)null);
        repository
            .Setup(x =>
                x.GetActiveHeroForUpdateAsync(heroId, playerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new Hero { Id = heroId, PlayerId = playerId });
        repository
            .Setup(x => x.HasActiveSessionAsync(heroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasActiveSession);
        return repository;
    }

    private static string Fingerprint(Guid heroId) =>
        Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(heroId.ToString("N"))
            )
        );

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 9, 29, 12, 0, 0, TimeSpan.Zero);
    }
}
