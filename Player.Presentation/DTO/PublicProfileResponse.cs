using Player.Application.Features.PlayerUseCase.GetPublicProfile;

namespace Player.Presentation.DTO;

/// <summary>Public, cacheable profile (US-PLAYER-06). Add a field here only if it is public.</summary>
public sealed record PublicProfileResponse(
    Guid PlayerId,
    string PublicHandle,
    string DisplayName,
    int NewGamePlusLevel,
    IReadOnlyList<PublicProfileHeroResponse> Heroes,
    PublicProfileCountersResponse Counters
)
{
    public static PublicProfileResponse From(PublicProfile profile) =>
        new(
            profile.PlayerId,
            profile.PublicHandle,
            profile.DisplayName,
            profile.NewGamePlusLevel,
            profile
                .Heroes.Select(hero => new PublicProfileHeroResponse(
                    hero.Name,
                    hero.ClassCode,
                    hero.ClassName,
                    hero.Level
                ))
                .ToList(),
            new PublicProfileCountersResponse(
                profile.Counters.HeroCount,
                profile.Counters.HighestHeroLevel,
                profile.Counters.CompletedSessionCount
            )
        );
}

public sealed record PublicProfileHeroResponse(
    string Name,
    string ClassCode,
    string ClassName,
    int Level
);

public sealed record PublicProfileCountersResponse(
    int HeroCount,
    int HighestHeroLevel,
    int CompletedSessionCount
);
