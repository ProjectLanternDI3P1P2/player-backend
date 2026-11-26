using Player.Domain.Entities;
using Player.Domain.Enums;
using Player.Domain.Repositories;

namespace Player.Application.Features.GameSessionUseCase;

/// <summary>
/// Single place that moves a session to a terminal state (victory, defeat, abandonment,
/// failure, repair) and releases its heroes so that they can join a new session.
/// Meant to be shared by hero deletion (US-PLAYER-10) and abandon/close (US-PLAYER-16/17).
/// </summary>
public static class GameSessionTermination
{
    public static void Close(
        IGameSessionRepository repository,
        GameSession session,
        GameSessionStatus terminalStatus,
        string cause,
        string actor,
        DateTimeOffset now
    )
    {
        if (!terminalStatus.IsTerminal())
            throw new ArgumentException(
                $"'{terminalStatus}' is not a terminal session status.",
                nameof(terminalStatus)
            );
        if (session.EndedAt is not null)
            return;

        GameSessionStatus sourceStatus = session.Status;
        session.Status = terminalStatus;
        session.TerminationReason = cause;
        session.EndedAt = now;
        session.LastActiveAt = now;

        IEnumerable<GameSessionMember> presentMembers = session.Members.Where(candidate =>
            candidate.LeftAt is null
        );
        foreach (GameSessionMember member in presentMembers)
        {
            member.MemberStatus = GameSessionMemberStatus.Left;
            member.LeftAt = now;
        }

        repository.AddTransition(
            new GameSessionTransition
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                SourceStatus = sourceStatus,
                TargetStatus = terminalStatus,
                Cause = cause,
                Actor = actor,
                OccurredAt = now,
            }
        );
    }
}
