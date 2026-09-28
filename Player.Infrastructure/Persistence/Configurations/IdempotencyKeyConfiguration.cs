using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_key");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasColumnName("key");
        builder.Property(x => x.PlayerId).HasColumnName("player_id");
        builder.Property(x => x.Scope).HasColumnName("scope").HasMaxLength(100);
        builder
            .Property(x => x.RequestFingerprint)
            .HasColumnName("request_fingerprint")
            .HasMaxLength(128);
        builder.Property(x => x.ProducedResourceId).HasColumnName("produced_resource_id");
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder
            .HasIndex(x => new
            {
                x.PlayerId,
                x.Scope,
                x.Key,
            })
            .IsUnique();
        builder
            .HasOne(x => x.Player)
            .WithMany(x => x.IdempotencyKeys)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
