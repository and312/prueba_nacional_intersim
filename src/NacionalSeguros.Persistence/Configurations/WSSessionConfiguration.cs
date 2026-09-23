using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class WSSessionConfiguration : IEntityTypeConfiguration<WSSession>
{
    public void Configure(EntityTypeBuilder<WSSession> builder)
    {
        builder.ToTable("WSSessions");

        builder.HasKey(ws => ws.Id);
        builder.Property(ws => ws.Id).HasColumnOrder(0);

        builder.Property(ws => ws.ChannelIdentifier)
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(ws => ws.NormalizedIdentifier)
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnOrder(2);

        builder.HasIndex(ws => ws.NormalizedIdentifier)
            .IsUnique();

        builder.Property(ws => ws.ActiveAgent)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(ws => ws.SessionStatus)
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(ws => ws.TemporaryDataJson)
            .HasColumnOrder(5);

        builder.Property(ws => ws.PendingFieldsJson)
            .HasColumnOrder(6);

        builder.Property(ws => ws.PerfilSessionDataJson)
            .IsRequired(false)
            .HasColumnOrder(7);

        builder.Property(ws => ws.LastInteractionAt)
            .IsRequired()
            .HasColumnOrder(8);

        builder.Property(ws => ws.CreatedAt)
            .IsRequired()
            .HasColumnOrder(9);

        builder.Property(ws => ws.UpdatedAt)
            .HasColumnOrder(10);
    }
}
