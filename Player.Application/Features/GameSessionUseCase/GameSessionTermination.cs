using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Application.Features.GameSessionUseCase;

/// <summary>
/// Single place that moves a session to a terminal state and releases its heroes.
/// Meant to be shared by hero deletion (US-PLAYER-10) and abandon/close (US-PLAYER-16/17).
/// </summary>
public static class GameSessionTermination
{
    public const string LobbyStatus = "Lobby";
    public const string ActiveStatus = "Active";
    public const string AbandonedStatus = "Abandoned";
    public const string LeftMemberStatus = "Left";

    public static void Close(
        IGameSessionRepository repository,
        GameSession session,
        string terminalStatus,
        string cause,
        string actor,
        DateTimeOffset now
    )
    {
        if (session.EndedAt is not null)
            return;

        string sourceStatus = session.Status;
        session.Status = terminalStatus;
        session.TerminationReason = cause;
        session.EndedAt = now;
        session.LastActiveAt = now;

        IEnumerable<GameSessionMember> presentMembers = session.Members.Where(candidate =>
            candidate.LeftAt is null
        );
        foreach (GameSessionMember member in presentMembers)
        {
            member.MemberStatus = LeftMemberStatus;
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
