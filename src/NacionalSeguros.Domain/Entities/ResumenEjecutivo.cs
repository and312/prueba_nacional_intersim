using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class ResumenEjecutivo : Entity<int>
{
    protected ResumenEjecutivo()
    {
    }

    public ResumenEjecutivo(
        int perfilCargoId,
        string resumen,
        string objetivoCargo,
        string funcionesPrincipales,
        string requisitosMinimos,
        string formacionExperiencia,
        string hardSkills,
        string softSkills,
        string modalidad,
        string ubicacion,
        string bandaSalarial,
        string criteriosEvaluacion,
        string caracteristicasClave,
        string valoracionPerfil,
        string createdBy)
    {
        PerfilCargoId = perfilCargoId;
        Resumen = resumen ?? throw new ArgumentNullException(nameof(resumen));
        ObjetivoCargo = objetivoCargo ?? throw new ArgumentNullException(nameof(objetivoCargo));
        FuncionesPrincipales = funcionesPrincipales ?? throw new ArgumentNullException(nameof(funcionesPrincipales));
        RequisitosMinimos = requisitosMinimos ?? throw new ArgumentNullException(nameof(requisitosMinimos));
        FormacionExperiencia = formacionExperiencia ?? throw new ArgumentNullException(nameof(formacionExperiencia));
        HardSkills = hardSkills ?? throw new ArgumentNullException(nameof(hardSkills));
        SoftSkills = softSkills ?? throw new ArgumentNullException(nameof(softSkills));
        Modalidad = modalidad ?? throw new ArgumentNullException(nameof(modalidad));
        Ubicacion = ubicacion ?? throw new ArgumentNullException(nameof(ubicacion));
        BandaSalarial = bandaSalarial ?? throw new ArgumentNullException(nameof(bandaSalarial));
        CriteriosEvaluacion = criteriosEvaluacion ?? throw new ArgumentNullException(nameof(criteriosEvaluacion));
        CaracteristicasClave = caracteristicasClave ?? throw new ArgumentNullException(nameof(caracteristicasClave));
        ValoracionPerfil = valoracionPerfil ?? throw new ArgumentNullException(nameof(valoracionPerfil));
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
    }

    public int PerfilCargoId { get; private set; }
    public string Resumen { get; private set; } = string.Empty;
    public string ObjetivoCargo { get; private set; } = string.Empty;
    public string FuncionesPrincipales { get; private set; } = string.Empty;
    public string RequisitosMinimos { get; private set; } = string.Empty;
    public string FormacionExperiencia { get; private set; } = string.Empty;
    public string HardSkills { get; private set; } = string.Empty;
    public string SoftSkills { get; private set; } = string.Empty;
    public string Modalidad { get; private set; } = string.Empty;
    public string Ubicacion { get; private set; } = string.Empty;
    public string BandaSalarial { get; private set; } = string.Empty;
    public string CriteriosEvaluacion { get; private set; } = string.Empty;
    public string CaracteristicasClave { get; private set; } = string.Empty;
    public string ValoracionPerfil { get; private set; } = string.Empty;

    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }

    public PerfilCargo PerfilCargo { get; private set; } = null!;

    public void Actualizar(
        string resumen,
        string objetivoCargo,
        string funcionesPrincipales,
        string requisitosMinimos,
        string formacionExperiencia,
        string hardSkills,
        string softSkills,
        string modalidad,
        string ubicacion,
        string bandaSalarial,
        string criteriosEvaluacion,
        string caracteristicasClave,
        string valoracionPerfil,
        string modifiedBy)
    {
        Resumen = resumen ?? throw new ArgumentNullException(nameof(resumen));
        ObjetivoCargo = objetivoCargo ?? throw new ArgumentNullException(nameof(objetivoCargo));
        FuncionesPrincipales = funcionesPrincipales ?? throw new ArgumentNullException(nameof(funcionesPrincipales));
        RequisitosMinimos = requisitosMinimos ?? throw new ArgumentNullException(nameof(requisitosMinimos));
        FormacionExperiencia = formacionExperiencia ?? throw new ArgumentNullException(nameof(formacionExperiencia));
        HardSkills = hardSkills ?? throw new ArgumentNullException(nameof(hardSkills));
        SoftSkills = softSkills ?? throw new ArgumentNullException(nameof(softSkills));
        Modalidad = modalidad ?? throw new ArgumentNullException(nameof(modalidad));
        Ubicacion = ubicacion ?? throw new ArgumentNullException(nameof(ubicacion));
        BandaSalarial = bandaSalarial ?? throw new ArgumentNullException(nameof(bandaSalarial));
        CriteriosEvaluacion = criteriosEvaluacion ?? throw new ArgumentNullException(nameof(criteriosEvaluacion));
        CaracteristicasClave = caracteristicasClave ?? throw new ArgumentNullException(nameof(caracteristicasClave));
        ValoracionPerfil = valoracionPerfil ?? throw new ArgumentNullException(nameof(valoracionPerfil));
        ModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        ModifiedDate = DateTime.UtcNow;
    }
}
