using Player.Application.Features.GameSessionUseCase;

namespace Player.Application.Features.HeroUseCase.DeleteHero;

/// <param name="AlreadyDeleted">True when the hero was deleted by an earlier request (replay).</param>
/// <param name="ClosedSessions">Lobbies closed before the deletion, to notify their members.</param>
public sealed record DeleteHeroResult(
    Guid HeroId,
    DateTimeOffset DeletedAt,
    bool AlreadyDeleted,
    IReadOnlyList<GameSessionSnapshot> ClosedSessions
);
