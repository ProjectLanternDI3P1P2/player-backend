using Player.Domain.Entities;

namespace Player.Application.Features.GameSessionUseCase;

public sealed record GameSessionSnapshot(
    Guid SessionId,
    Guid CreatorPlayerId,
    string State,
    string Mode,
    IReadOnlyList<SessionHero> Members,
    Guid? DungeonRunId,
    string? DungeonSeed,
    string? FailureReason,
    bool AlreadyExists
)
{
    public static GameSessionSnapshot From(GameSession session, bool alreadyExists = false) =>
        new(
            session.Id,
            session.CreatorPlayerId,
            session.Status,
            session.Mode,
            session.Members.Select(member => SessionHero.From(member.Hero)).ToArray(),
            session.DungeonRunId,
            session.DungeonSeed,
            session.TerminationReason,
            alreadyExists
        );
}

public sealed record SessionHero(Guid Id, string Name, string ClassCode, int Level)
{
    public static SessionHero From(Hero hero) =>
        new(hero.Id, hero.Name, hero.ClassCode, hero.Level);
}
