using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Matching : Entity<int>
{
    // Requerido por EF Core
    protected Matching()
    {
    }

    public Matching(
        int postulanteId,
        int vacanteId,
        decimal scoreCoincidencia,
        string coincidenciasText,
        string brechasText,
        long executionId)
    {
        if (postulanteId <= 0) throw new ArgumentException("El ID del postulante debe ser mayor a 0.", nameof(postulanteId));
        if (vacanteId <= 0) throw new ArgumentException("El ID de la vacante debe ser mayor a 0.", nameof(vacanteId));
        if (scoreCoincidencia < 0 || scoreCoincidencia > 100) throw new ArgumentException("El score de coincidencia debe estar entre 0 y 100.", nameof(scoreCoincidencia));

        PostulanteId = postulanteId;
        VacanteId = vacanteId;
        ScoreCoincidencia = scoreCoincidencia;
        CoincidenciasText = coincidenciasText ?? throw new ArgumentNullException(nameof(coincidenciasText));
        BrechasText = brechasText ?? throw new ArgumentNullException(nameof(brechasText));
        ExecutionId = executionId;
        CreatedDate = DateTime.UtcNow;
    }

    public int PostulanteId { get; private set; }
    public int VacanteId { get; private set; }
    public decimal ScoreCoincidencia { get; private set; }
    public string CoincidenciasText { get; private set; } = string.Empty;
    public string BrechasText { get; private set; } = string.Empty;
    public long ExecutionId { get; private set; }
    public DateTime CreatedDate { get; private set; }

    // Propiedades de navegación
    public Postulante Postulante { get; private set; } = null!;
    public Vacante Vacante { get; private set; } = null!;
    public AgentExecution AgentExecution { get; private set; } = null!;
}
