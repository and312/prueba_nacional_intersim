using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class AgentEventConfiguration : IEntityTypeConfiguration<AgentEvent>
{
    public void Configure(EntityTypeBuilder<AgentEvent> builder)
    {
        builder.ToTable("AgentEvents");

        builder.HasKey(ae => ae.Id);

        builder.Property(ae => ae.ChannelType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ae => ae.ChannelIdentifier)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ae => ae.EventType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ae => ae.Description)
            .HasMaxLength(500);

        builder.Property(ae => ae.EventSource)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ae => ae.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(ae => ae.RelatedEntityId)
            .HasMaxLength(100);

        builder.Property(ae => ae.CreatedAt)
            .IsRequired();
    }
}
