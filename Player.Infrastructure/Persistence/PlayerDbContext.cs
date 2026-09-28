using PlayerEntity = global::Player.Domain.Entities.Player;
using Microsoft.EntityFrameworkCore;

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
