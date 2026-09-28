using Player.Domain.Repositories;
using PlayerEntity = global::Player.Domain.Entities.Player;

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
