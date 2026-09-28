using PlayerEntity = global::Player.Domain.Entities.Player;
using Player.Domain.Repositories;

namespace Player.Infrastructure.Persistence.Repositories;

public sealed class PlayerRepository(PlayerDbContext dbContext) : IPlayerRepository
{
    public async Task AddPlayerAsync(PlayerEntity player, CancellationToken cancellationToken)
    {
        await dbContext.Players.AddAsync(player, cancellationToken);
    }

    public async Task<PlayerEntity?> GetPlayerByIdAsync(
        Guid playerId,
        CancellationToken cancellationToken
    )
    {
        return await dbContext.Players.FindAsync([playerId], cancellationToken);
    }
}
