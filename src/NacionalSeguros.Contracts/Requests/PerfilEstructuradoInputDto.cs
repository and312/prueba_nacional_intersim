using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Requests;

public class PerfilEstructuradoInputDto
{
    public int SolicitudId { get; set; }
    public PerfilEstructuradoContentDto PerfilEstructurado { get; set; } = null!;
    public List<string> FuentesUtilizadas { get; set; } = new();
    public List<string> Alertas { get; set; } = new();
    public string EstadoGeneracion { get; set; } = string.Empty;
}

public class PerfilEstructuradoContentDto
{
    public object DatosGeneralesCargo { get; set; } = null!;
    public string ObjetivoPrincipalCargo { get; set; } = string.Empty;
    public object PerfilRequerido { get; set; } = null!;
    public List<object> ConocimientosTecnicosRequeridos { get; set; } = new();
    public object HerramientasSistemas { get; set; } = null!;
    public List<object> FuncionesPrincipalesCargo { get; set; } = new();
    public List<object> CompetenciasClave { get; set; } = new();
    public List<object> IndicadoresExitoCargo { get; set; } = new();
    public string PerfilIdealCandidato { get; set; } = string.Empty;
    public object FiltrosClaveSeleccion { get; set; } = null!;
    public List<object> MatrizPonderacion { get; set; } = new();
    public string PerfilTipoAltoAjuste { get; set; } = string.Empty;
}
