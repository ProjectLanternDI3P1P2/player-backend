namespace Player.Presentation.DTO;

public sealed record DeleteHeroResponse(
    Guid HeroId,
    bool Deleted,
    DateTimeOffset DeletedAt,
    bool AlreadyDeleted,
    IReadOnlyList<Guid> ClosedSessionIds
);
