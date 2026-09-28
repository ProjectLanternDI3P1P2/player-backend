using PlayerEntity = global::Player.Domain.Entities.Player;

namespace Player.Domain.Repositories;

public interface IPlayerRepository
{
    Task<PlayerEntity?> GetPlayerByIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task AddPlayerAsync(PlayerEntity player, CancellationToken cancellationToken);
}
