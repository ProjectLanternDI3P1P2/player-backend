using FluentAssertions;
using Moq;
using Player.Application.Features.GameSessionUseCase;
using Player.Domain.Entities;
using Player.Domain.Enums;
using Player.Domain.Repositories;

namespace Player.Test.Features.GameSessionUseCase;

public sealed class GameSessionTerminationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(GameSessionStatus.Completed, "Victory")]
    [InlineData(GameSessionStatus.Failed, "Defeat")]
    [InlineData(GameSessionStatus.Abandoned, "PlayerAbandoned")]
    [InlineData(GameSessionStatus.Failed, "DungeonFailure")]
    [InlineData(GameSessionStatus.Abandoned, "Repair")]
    public void Close_WithAnyTerminalPath_ReleasesEveryHero(
        GameSessionStatus terminalStatus,
        string cause
    )
    {
        var repository = new Mock<IGameSessionRepository>();
        GameSession session = ActiveSession(Guid.NewGuid(), Guid.NewGuid());

        GameSessionTermination.Close(
            repository.Object,
            session,
            terminalStatus,
            cause,
            "system",
            Now
        );

        session.Status.Should().Be(terminalStatus);
        session.TerminationReason.Should().Be(cause);
        session.EndedAt.Should().Be(Now);
        session
            .Members.Should()
            .OnlyContain(member =>
                member.MemberStatus == GameSessionMemberStatus.Left && member.LeftAt == Now
            );
        repository.Verify(
            x =>
                x.AddTransition(
                    It.Is<GameSessionTransition>(transition =>
                        transition.SourceStatus == GameSessionStatus.Active
                        && transition.TargetStatus == terminalStatus
                        && transition.Cause == cause
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public void Close_WhenTheSessionAlreadyEnded_DoesNothing()
    {
        var repository = new Mock<IGameSessionRepository>();
        GameSession session = ActiveSession(Guid.NewGuid());
        GameSessionTermination.Close(
            repository.Object,
            session,
            GameSessionStatus.Completed,
            "Victory",
            "system",
            Now
        );

        GameSessionTermination.Close(
            repository.Object,
            session,
            GameSessionStatus.Abandoned,
            "Repair",
            "system",
            Now.AddMinutes(5)
        );

        session.Status.Should().Be(GameSessionStatus.Completed);
        repository.Verify(x => x.AddTransition(It.IsAny<GameSessionTransition>()), Times.Once);
    }

    [Theory]
    [InlineData(GameSessionStatus.None)]
    [InlineData(GameSessionStatus.Lobby)]
    [InlineData(GameSessionStatus.Pending)]
    [InlineData(GameSessionStatus.Started)]
    [InlineData(GameSessionStatus.Active)]
    public void Close_WithANonTerminalStatus_IsRejectedAndKeepsTheHeroesEngaged(
        GameSessionStatus status
    )
    {
        var repository = new Mock<IGameSessionRepository>();
        GameSession session = ActiveSession(Guid.NewGuid());

        Action action = () =>
            GameSessionTermination.Close(
                repository.Object,
                session,
                status,
                "Cause",
                "system",
                Now
            );

        action.Should().Throw<ArgumentException>();
        session
            .Members.Should()
            .OnlyContain(member => member.MemberStatus == GameSessionMemberStatus.Active);
        session.EndedAt.Should().BeNull();
    }

    private static GameSession ActiveSession(params Guid[] heroIds) =>
        new()
        {
            Id = Guid.NewGuid(),
            Status = GameSessionStatus.Active,
            Mode = "Solo",
            Members = heroIds
                .Select(heroId => new GameSessionMember
                {
                    HeroId = heroId,
                    MemberStatus = GameSessionMemberStatus.Active,
                })
                .ToList(),
        };
}
