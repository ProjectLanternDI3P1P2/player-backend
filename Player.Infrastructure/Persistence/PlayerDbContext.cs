using Microsoft.EntityFrameworkCore;
using PlayerEntity = global::Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence;

public class PlayerDbContext(DbContextOptions<PlayerDbContext> options) : DbContext(options)
{
    public virtual DbSet<PlayerEntity> Players { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applique toutes les configurations d'entités automatiquement
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlayerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
