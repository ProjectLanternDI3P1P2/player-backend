namespace Player.Application.Features.PlayerUseCase.GetPublicProfile;

/// <summary>Only data a visitor may see: never the external identity, status or dates.</summary>
public sealed record PublicProfile(
    Guid PlayerId,
    string PublicHandle,
    string DisplayName,
    int NewGamePlusLevel,
    IReadOnlyList<PublicProfileHero> Heroes,
    PublicProfileCounters Counters
);

public sealed record PublicProfileHero(string Name, string ClassCode, string ClassName, int Level);

public sealed record PublicProfileCounters(
    int HeroCount,
    int HighestHeroLevel,
    int CompletedSessionCount
);
