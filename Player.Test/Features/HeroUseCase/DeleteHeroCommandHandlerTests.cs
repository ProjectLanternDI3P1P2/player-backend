using FluentAssertions;
using Moq;
using Player.Application.Features.GameSessionUseCase;
using Player.Application.Features.HeroUseCase.DeleteHero;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Features.HeroUseCase;

public sealed class DeleteHeroCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    private readonly Guid playerId = Guid.NewGuid();
    private readonly Guid heroId = Guid.NewGuid();
    private readonly Mock<IHeroRepository> heroRepository = new();
    private readonly Mock<IGameSessionRepository> sessionRepository = new();

    [Fact]
    public async Task Handle_FreeHero_SoftDeletesItAndClearsTheSelection()
    {
        PlayerEntity player = SetupPlayer(selectedHeroId: heroId);
        Hero hero = SetupHero();
        SetupOpenSessions();

        DeleteHeroResult result = await Handle();

        hero.IsDeleted.Should().BeTrue();
        hero.DeletedAt.Should().Be(Now);
        player.SelectedHeroId.Should().BeNull();
        result.AlreadyDeleted.Should().BeFalse();
        result.DeletedAt.Should().Be(Now);
        result.ClosedSessions.Should().BeEmpty();
        sessionRepository.Verify(
            x => x.AddTransition(It.IsAny<GameSessionTransition>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_AnotherHeroSelected_KeepsTheSelection()
    {
        Guid otherHeroId = Guid.NewGuid();
        PlayerEntity player = SetupPlayer(selectedHeroId: otherHeroId);
        SetupHero();
        SetupOpenSessions();

        await Handle();

        player.SelectedHeroId.Should().Be(otherHeroId);
    }

    [Fact]
    public async Task Handle_HeroWaitingInALobby_ClosesTheLobbyBeforeDeletingTheHero()
    {
        SetupPlayer();
        Hero hero = SetupHero();
        Hero teammate = new() { Id = Guid.NewGuid(), Name = "Teammate" };
        GameSession lobby = Session("Lobby", hero, teammate);
        SetupOpenSessions(lobby);

        DeleteHeroResult result = await Handle();

        lobby.Status.Should().Be("Abandoned");
        lobby.TerminationReason.Should().Be("HeroDeleted");
        lobby.EndedAt.Should().Be(Now);
        lobby
            .Members.Should()
            .OnlyContain(member => member.MemberStatus == "Left" && member.LeftAt == Now);
        hero.IsDeleted.Should().BeTrue();
        result.ClosedSessions.Should().ContainSingle();
        GameSessionSnapshot closed = result.ClosedSessions.Single();
        closed.SessionId.Should().Be(lobby.Id);
        closed.State.Should().Be("Abandoned");
        closed.Members.Should().HaveCount(2);
        sessionRepository.Verify(
            x =>
                x.AddTransition(
                    It.Is<GameSessionTransition>(transition =>
                        transition.SessionId == lobby.Id
                        && transition.SourceStatus == "Lobby"
                        && transition.TargetStatus == "Abandoned"
                        && transition.Cause == "HeroDeleted"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_HeroPlayingARunningSession_IsRefusedWithoutAnyEffect()
    {
        PlayerEntity player = SetupPlayer(selectedHeroId: heroId);
        Hero hero = SetupHero();
        GameSession running = Session("Active", hero);
        SetupOpenSessions(running);

        Func<Task> act = Handle;

        await act.Should().ThrowAsync<ConflictException>();
        hero.IsDeleted.Should().BeFalse();
        player.SelectedHeroId.Should().Be(heroId);
        running.Status.Should().Be("Active");
        running.EndedAt.Should().BeNull();
        sessionRepository.Verify(
            x => x.AddTransition(It.IsAny<GameSessionTransition>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_AlreadyDeletedHero_IsIdempotent()
    {
        DateTimeOffset deletedAt = Now.AddHours(-1);
        PlayerEntity player = SetupPlayer();
        Hero hero = SetupHero();
        hero.IsDeleted = true;
        hero.DeletedAt = deletedAt;
        SetupOpenSessions();

        DeleteHeroResult result = await Handle();

        result.AlreadyDeleted.Should().BeTrue();
        result.DeletedAt.Should().Be(deletedAt);
        result.ClosedSessions.Should().BeEmpty();
        hero.DeletedAt.Should().Be(deletedAt);
        player.SelectedHeroId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UnknownOrForeignHero_ThrowsNotFound()
    {
        SetupPlayer();
        SetupOpenSessions();
        heroRepository
            .Setup(x =>
                x.GetByIdAndPlayerIdForUpdateAsync(heroId, playerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Hero?)null);

        Func<Task> act = Handle;

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_UnknownPlayer_ThrowsNotFound()
    {
        heroRepository
            .Setup(x => x.GetPlayerForUpdateAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerEntity?)null);

        Func<Task> act = Handle;

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private Task<DeleteHeroResult> Handle() =>
        new DeleteHeroCommandHandler(
            heroRepository.Object,
            sessionRepository.Object,
            new FixedClock()
        ).Handle(new DeleteHeroCommand(playerId, heroId), TestContext.Current.CancellationToken);

    private PlayerEntity SetupPlayer(Guid? selectedHeroId = null)
    {
        var player = new PlayerEntity { Id = playerId, SelectedHeroId = selectedHeroId };
        heroRepository
            .Setup(x => x.GetPlayerForUpdateAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
        return player;
    }

    private Hero SetupHero()
    {
        var hero = new Hero
        {
            Id = heroId,
            PlayerId = playerId,
            Name = "Brann",
            ClassCode = "warrior",
            Level = 1,
            CreatedAt = Now.AddDays(-3),
        };
        heroRepository
            .Setup(x =>
                x.GetByIdAndPlayerIdForUpdateAsync(heroId, playerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(hero);
        return hero;
    }

    private void SetupOpenSessions(params GameSession[] sessions) =>
        sessionRepository
            .Setup(x =>
                x.ListOpenSessionsForHeroForUpdateAsync(heroId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(sessions);

    private GameSession Session(string status, params Hero[] heroes)
    {
        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            CreatorPlayerId = playerId,
            Status = status,
            Mode = heroes.Length > 1 ? "Group" : "Solo",
        };
        foreach (Hero hero in heroes)
        {
            session.Members.Add(
                new GameSessionMember
                {
                    SessionId = session.Id,
                    HeroId = hero.Id,
                    Hero = hero,
                    GameSession = session,
                    MemberStatus = "Active",
                    JoinedAt = Now.AddMinutes(-5),
                }
            );
        }
        return session;
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => Now;
    }
}
