using MediatR;
using Player.Domain.Entities;
using Player.Domain.Repositories;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Application.Features.PlayerUseCase.GetPublicProfile;

public sealed class GetPublicProfileQueryHandler(IPlayerRepository repository)
    : IRequestHandler<GetPublicProfileQuery, PublicProfile?>
{
    private const string ActiveAccountStatus = "Active";

    public async Task<PublicProfile?> Handle(
        GetPublicProfileQuery request,
        CancellationToken cancellationToken
    )
    {
        PlayerEntity? player = await FindAsync(request, cancellationToken);
        if (player is null || !IsPubliclyVisible(player))
            return null;

        List<Hero> heroes = player
            .Heroes.Where(hero => !hero.IsDeleted)
            .OrderByDescending(hero => hero.Level)
            .ThenBy(hero => hero.Name, StringComparer.Ordinal)
            .ToList();
        int completedSessionCount = await repository.CountCompletedSessionsAsync(
            player.Id,
            cancellationToken
        );

        return new PublicProfile(
            player.Id,
            player.PublicHandle,
            player.DisplayName,
            player.NewGamePlusLevel,
            heroes
                .Select(hero => new PublicProfileHero(
                    hero.Name,
                    hero.ClassCode,
                    hero.HeroClass.Label,
                    hero.Level
                ))
                .ToList(),
            new PublicProfileCounters(
                heroes.Count,
                heroes.Count == 0 ? 0 : heroes.Max(hero => hero.Level),
                completedSessionCount
            )
        );
    }

    private Task<PlayerEntity?> FindAsync(
        GetPublicProfileQuery request,
        CancellationToken cancellationToken
    )
    {
        if (request.PlayerId is Guid playerId)
            return repository.GetWithActiveHeroesByIdAsync(playerId, cancellationToken);

        string? publicHandle = request.PublicHandle?.ToLowerInvariant();
        // A malformed handle cannot exist: answer like an unknown player, without a lookup.
        if (!PlayerPublicHandle.IsWellFormed(publicHandle))
            return Task.FromResult<PlayerEntity?>(null);

        return repository.GetWithActiveHeroesByPublicHandleAsync(publicHandle, cancellationToken);
    }

    // Anonymized or non-active accounts are indistinguishable from unknown ones.
    private static bool IsPubliclyVisible(PlayerEntity player) =>
        player.AnonymizedAt is null
        && string.Equals(player.AccountStatus, ActiveAccountStatus, StringComparison.Ordinal);
}
