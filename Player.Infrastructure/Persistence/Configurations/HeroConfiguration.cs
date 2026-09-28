using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class HeroConfiguration : IEntityTypeConfiguration<Hero>
{
    public void Configure(EntityTypeBuilder<Hero> builder)
    {
        builder.ToTable("hero");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PlayerId).HasColumnName("player_id");
        builder.Property(x => x.ClassCode).HasColumnName("class_code").HasMaxLength(50);
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
        builder.Property(x => x.Level).HasColumnName("level");
        builder.Property(x => x.Strength).HasColumnName("strength");
        builder.Property(x => x.Endurance).HasColumnName("endurance");
        builder.Property(x => x.Agility).HasColumnName("agility");
        builder.Property(x => x.Intelligence).HasColumnName("intelligence");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder
            .HasOne(x => x.Player)
            .WithMany(x => x.Heroes)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(x => x.HeroClass)
            .WithMany(x => x.Heroes)
            .HasForeignKey(x => x.ClassCode)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PlayerId, x.Name }).IsUnique();
    }
}
