namespace Player.Application.Features.HeroUseCase.ListHeroes;

/// <param name="ActiveSession">The open session holding the hero, if any: a lobby can be
/// closed by a deletion, a running dungeon cannot.</param>
public sealed record HeroSummary(
    Guid Id,
    string Name,
    string ClassCode,
    int Level,
    int MaximumHealth,
    DateTimeOffset CreatedAt,
    bool IsSelected,
    HeroActiveSession? ActiveSession = null
)
{
    /// <summary>
    /// Kept for the existing clients. Derived from <see cref="ActiveSession"/> so the two can
    /// never disagree.
    /// </summary>
    public bool IsEngagedInActiveSession => ActiveSession is not null;
}

/// <param name="State">The session status: <c>Lobby</c> or <c>Active</c>.</param>
public sealed record HeroActiveSession(Guid Id, string State);
