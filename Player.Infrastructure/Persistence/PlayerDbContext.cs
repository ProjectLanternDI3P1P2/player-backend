using Microsoft.EntityFrameworkCore;
using Player.Domain.Entities;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence;

public class PlayerDbContext(DbContextOptions<PlayerDbContext> options) : DbContext(options)
{
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<HeroClass> HeroClasses => Set<HeroClass>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Hero> Heroes => Set<Hero>();
    public DbSet<HeroSkill> HeroSkills => Set<HeroSkill>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<MatchmakingQueueEntry> MatchmakingQueueEntries => Set<MatchmakingQueueEntry>();
    public DbSet<MatchmakingGroup> MatchmakingGroups => Set<MatchmakingGroup>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<GameSessionMember> GameSessionMembers => Set<GameSessionMember>();
    public DbSet<GameSessionTransition> GameSessionTransitions => Set<GameSessionTransition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applique toutes les configurations d'entités automatiquement
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlayerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
