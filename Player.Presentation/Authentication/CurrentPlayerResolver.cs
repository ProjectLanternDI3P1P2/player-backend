using Microsoft.EntityFrameworkCore;
using Player.Domain.Exceptions;
using Player.Domain.Services;
using Player.Infrastructure.Persistence;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Presentation.Authentication;

/// <summary>
/// The gateway validates the JWT and supplies these headers. Route and SignalR
/// payload player IDs are deliberately never used as an authorization source.
/// </summary>
public sealed class CurrentPlayerResolver(PlayerDbContext dbContext, IClock clock)
    : ICurrentPlayerResolver
{
    private const string UserIdHeader = "X-User-Id";
    private const string UsernameHeader = "X-Username";

    public async Task<Guid> ResolveAsync(HttpContext? context, CancellationToken cancellationToken)
    {
        string externalUserId =
            context?.Request.Headers[UserIdHeader].FirstOrDefault()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            throw new UnauthorizedException("The authenticated user identity is missing.");
        }

        PlayerEntity? player = await dbContext.Players.SingleOrDefaultAsync(
            candidate => candidate.ExternalUserId == externalUserId,
            cancellationToken
        );
        if (player is not null)
        {
            return player.Id;
        }

        //TODO: Supprimer ça : Création du player à la création du compte keycloak plutôt qu'à un appel random
        string? username = context?.Request.Headers[UsernameHeader].FirstOrDefault()?.Trim();
        Guid playerId = Guid.NewGuid();
        player = new PlayerEntity
        {
            Id = playerId,
            ExternalUserId = externalUserId,
            DisplayName = string.IsNullOrWhiteSpace(username)
                ? $"Player-{playerId.ToString("N")[..8]}"
                : username[..Math.Min(username.Length, 100)],
            AccountStatus = "Active",
            CreatedAt = clock.UtcNow,
        };
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync(cancellationToken);
        return player.Id;
    }
}
