using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Scoring : Entity<int>
{
    // Requerido por EF Core
    protected Scoring()
    {
    }

    public Scoring(
        int postulanteId,
        int vacanteId,
        int scoreSkills,
        int scoreExperiencia,
        decimal scoreFinal,
        string justificacionText,
        long executionId)
    {
        if (postulanteId <= 0) throw new ArgumentException("El ID del postulante debe ser mayor a 0.", nameof(postulanteId));
        if (vacanteId <= 0) throw new ArgumentException("El ID de la vacante debe ser mayor a 0.", nameof(vacanteId));
        if (scoreSkills < 0 || scoreSkills > 100) throw new ArgumentException("El score de skills debe estar entre 0 y 100.", nameof(scoreSkills));
        if (scoreExperiencia < 0 || scoreExperiencia > 100) throw new ArgumentException("El score de experiencia debe estar entre 0 y 100.", nameof(scoreExperiencia));
        if (scoreFinal < 0 || scoreFinal > 100) throw new ArgumentException("El score final debe estar entre 0 y 100.", nameof(scoreFinal));

        PostulanteId = postulanteId;
        VacanteId = vacanteId;
        ScoreSkills = scoreSkills;
        ScoreExperiencia = scoreExperiencia;
        ScoreFinal = scoreFinal;
        JustificacionText = justificacionText ?? throw new ArgumentNullException(nameof(justificacionText));
        ExecutionId = executionId;
        CreatedDate = DateTime.UtcNow;
    }

    public int PostulanteId { get; private set; }
    public int VacanteId { get; private set; }
    public int ScoreSkills { get; private set; }
    public int ScoreExperiencia { get; private set; }
    public decimal ScoreFinal { get; private set; }
    public string JustificacionText { get; private set; } = string.Empty;
    public long ExecutionId { get; private set; }
    public DateTime CreatedDate { get; private set; }

    // Propiedades de navegación
    public Postulante Postulante { get; private set; } = null!;
    public Vacante Vacante { get; private set; } = null!;
    public AgentExecution AgentExecution { get; private set; } = null!;
}
