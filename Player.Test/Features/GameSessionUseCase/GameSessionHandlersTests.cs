using FluentAssertions;
using Moq;
using Player.Application.Features.GameSessionUseCase;
using Player.Application.Features.GameSessionUseCase.ChangeSessionHero;
using Player.Application.Features.GameSessionUseCase.CreateSoloLobby;
using Player.Application.Features.GameSessionUseCase.StartSession;
using Player.Application.Ports;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;

namespace Player.Test.Features.GameSessionUseCase;

public sealed class GameSessionHandlersTests
{
    [Fact]
    public async Task CreateSoloLobby_CreatesALobbyWithoutCreatingADungeonRun()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        var repository = CreateRepository(playerId, heroId);
        var handler = new CreateSoloLobbyCommandHandler(repository.Object, new FixedClock());

        GameSessionSnapshot result = await handler.Handle(
            new CreateSoloLobbyCommand(playerId, heroId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.State.Should().Be("Lobby");
        result.DungeonRunId.Should().BeNull();
        result.CreatorPlayerId.Should().Be(playerId);
        result.Members.Should().ContainSingle(member => member.Id == heroId);
        repository.Verify(
            x =>
                x.Add(
                    It.Is<GameSession>(session =>
                        session.Status == "Lobby"
                        && session.Transitions.Single().TargetStatus == "Lobby"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task StartSession_ByCreator_CreatesTheDungeonRunAndLocksTheLobby()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        var repository = CreateRepository(playerId, heroId);
        GameSession session = Lobby(sessionId, playerId, heroId);
        repository
            .Setup(x => x.GetByIdForUpdateAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        var dungeon = new Mock<IDungeonClient>();
        dungeon
            .Setup(x =>
                x.StartRunAsync(
                    It.IsAny<Guid>(),
                    sessionId,
                    It.Is<IReadOnlyList<DungeonParticipant>>(party =>
                        party.Single().HeroId == heroId
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new DungeonRun(Guid.NewGuid(), "seed"));
        var handler = new StartSessionCommandHandler(
            repository.Object,
            dungeon.Object,
            new FixedClock()
        );

        GameSessionSnapshot result = await handler.Handle(
            new StartSessionCommand(playerId, sessionId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.State.Should().Be("Active");
        result.DungeonRunId.Should().NotBeNull();
        repository.Verify(
            x =>
                x.AddTransition(
                    It.Is<GameSessionTransition>(transition =>
                        transition.SourceStatus == "Lobby" && transition.TargetStatus == "Active"
                    )
                ),
            Times.Once
        );
        dungeon.Verify(
            x =>
                x.StartRunAsync(
                    It.IsAny<Guid>(),
                    sessionId,
                    It.IsAny<IReadOnlyList<DungeonParticipant>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task StartSession_ByNonCreator_IsRejected()
    {
        Guid creatorId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        var repository = CreateRepository(creatorId, heroId);
        repository
            .Setup(x => x.GetByIdForUpdateAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Lobby(sessionId, creatorId, heroId));
        var handler = new StartSessionCommandHandler(
            repository.Object,
            Mock.Of<IDungeonClient>(),
            new FixedClock()
        );

        Func<Task> action = () =>
            handler.Handle(
                new StartSessionCommand(Guid.NewGuid(), sessionId, Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Theory]
    [InlineData(nameof(HttpRequestException))]
    [InlineData(nameof(TimeoutException))]
    [InlineData(nameof(DungeonUnavailableException))]
    public async Task StartSession_WhenDungeonIsUnavailable_LeavesTheLobbyRetryable(string outage)
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        var repository = CreateRepository(playerId, heroId);
        GameSession session = Lobby(sessionId, playerId, heroId);
        repository
            .Setup(x => x.GetByIdForUpdateAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        var dungeon = new Mock<IDungeonClient>();
        dungeon
            .Setup(x =>
                x.StartRunAsync(
                    It.IsAny<Guid>(),
                    sessionId,
                    It.IsAny<IReadOnlyList<DungeonParticipant>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(Outage(outage));
        var handler = new StartSessionCommandHandler(
            repository.Object,
            dungeon.Object,
            new FixedClock()
        );

        Func<Task> action = () =>
            handler.Handle(
                new StartSessionCommand(playerId, sessionId, Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<InvalidOperationException>();
        session.Status.Should().Be("Lobby");
        session.DungeonRunId.Should().BeNull();
    }

    [Fact]
    public async Task ChangeSessionHero_ByCreator_ReplacesTheLobbyRoster()
    {
        Guid playerId = Guid.NewGuid();
        Guid previousHeroId = Guid.NewGuid();
        Guid replacementHeroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        var repository = CreateRepository(playerId, previousHeroId);
        GameSession lobby = Lobby(sessionId, playerId, previousHeroId);
        repository
            .Setup(x => x.GetByIdForUpdateAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lobby);
        repository
            .Setup(x =>
                x.GetActiveHeroForUpdateAsync(
                    replacementHeroId,
                    playerId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new Hero
                {
                    Id = replacementHeroId,
                    PlayerId = playerId,
                    Name = "Morgana",
                    ClassCode = "mage",
                    Level = 2,
                }
            );
        repository
            .Setup(x => x.HasOpenSessionAsync(replacementHeroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new ChangeSessionHeroCommandHandler(repository.Object, new FixedClock());

        GameSessionSnapshot result = await handler.Handle(
            new ChangeSessionHeroCommand(playerId, sessionId, replacementHeroId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.Members.Should().ContainSingle(member => member.Id == replacementHeroId);
        repository.Verify(
            x =>
                x.RemoveMember(It.Is<GameSessionMember>(member => member.HeroId == previousHeroId)),
            Times.Once
        );
        repository.Verify(
            x =>
                x.AddMember(It.Is<GameSessionMember>(member => member.HeroId == replacementHeroId)),
            Times.Once
        );
    }

    [Fact]
    public async Task ChangeSessionHero_ByNonCreator_IsRejected()
    {
        Guid creatorId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        var repository = CreateRepository(creatorId, heroId);
        repository
            .Setup(x => x.GetByIdForUpdateAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Lobby(sessionId, creatorId, heroId));
        var handler = new ChangeSessionHeroCommandHandler(repository.Object, new FixedClock());

        Func<Task> action = () =>
            handler.Handle(
                new ChangeSessionHeroCommand(Guid.NewGuid(), sessionId, heroId, Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );

        await action.Should().ThrowAsync<ConflictException>();
    }

    private static Mock<IGameSessionRepository> CreateRepository(Guid playerId, Guid heroId)
    {
        var repository = new Mock<IGameSessionRepository>();
        repository
            .Setup(x => x.GetIdempotencyKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyKey?)null);
        repository
            .Setup(x =>
                x.GetActiveHeroForUpdateAsync(heroId, playerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new Hero
                {
                    Id = heroId,
                    PlayerId = playerId,
                    Name = "Merlin",
                    ClassCode = "mage",
                    Level = 1,
                }
            );
        repository
            .Setup(x => x.HasOpenSessionAsync(heroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        return repository;
    }

    private static GameSession Lobby(Guid sessionId, Guid playerId, Guid heroId) =>
        new()
        {
            Id = sessionId,
            CreatorPlayerId = playerId,
            Status = "Lobby",
            Mode = "Solo",
            Members =
            [
                new GameSessionMember
                {
                    HeroId = heroId,
                    MemberStatus = "Active",
                    Hero = new Hero
                    {
                        Id = heroId,
                        PlayerId = playerId,
                        Name = "Merlin",
                        ClassCode = "mage",
                        Level = 1,
                    },
                },
            ],
            Transitions =
            [
                new GameSessionTransition { SourceStatus = "None", TargetStatus = "Lobby" },
            ],
        };

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);
    }

    private static Exception Outage(string name) =>
        name switch
        {
            nameof(TimeoutException) => new TimeoutException(),
            nameof(DungeonUnavailableException) => new DungeonUnavailableException(
                "Dungeon is down.",
                new HttpRequestException()
            ),
            _ => new HttpRequestException(),
        };
}
