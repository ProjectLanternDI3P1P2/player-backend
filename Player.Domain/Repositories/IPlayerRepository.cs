using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Domain.Repositories;

public interface IPlayerRepository
{
    /// <summary>Read-only; the player whatever its status, with its non-deleted heroes and their class.</summary>
    Task<PlayerEntity?> GetWithActiveHeroesByIdAsync(
        Guid playerId,
        CancellationToken cancellationToken
    );

    /// <summary>Same as <see cref="GetWithActiveHeroesByIdAsync"/>, by exact (lowercase) public handle.</summary>
    Task<PlayerEntity?> GetWithActiveHeroesByPublicHandleAsync(
        string publicHandle,
        CancellationToken cancellationToken
    );

    /// <summary>Completed game sessions joined by any hero of the player, deleted heroes included.</summary>
    Task<int> CountCompletedSessionsAsync(Guid playerId, CancellationToken cancellationToken);
}
