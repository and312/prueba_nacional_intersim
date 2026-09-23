using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class AgentExecutionConfiguration : IEntityTypeConfiguration<AgentExecution>
{
    public void Configure(EntityTypeBuilder<AgentExecution> builder)
    {
        builder.ToTable("AgentExecutions");

        builder.HasKey(e => e.ExecutionId);

        builder.Property(e => e.ExecutionId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.AgenteId)
            .IsRequired();

        builder.Property(e => e.PromptVersionId)
            .IsRequired();

        builder.Property(e => e.UsuarioId)
            .IsRequired();

        builder.Property(e => e.FechaInicio)
            .IsRequired();

        builder.Property(e => e.FechaFin)
            .IsRequired();

        builder.Property(e => e.DuracionMs)
            .IsRequired();

        builder.Property(e => e.InputJson)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(e => e.OutputJson)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(e => e.ResultadoStatus)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(e => e.TokensInput)
            .IsRequired();

        builder.Property(e => e.TokensOutput)
            .IsRequired();

        builder.Property(e => e.CostoEstimado)
            .IsRequired()
            .HasColumnType("DECIMAL(10,5)");

        builder.Property(e => e.CorrelationId)
            .IsRequired();
    }
}
