using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Gerencia> Gerencias => Set<Gerencia>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Catalogo> Catalogos => Set<Catalogo>();
    public DbSet<Parametro> Parametros => Set<Parametro>();
    public DbSet<Estado> Estados => Set<Estado>();
    public DbSet<Sla> Slas => Set<Sla>();
    public DbSet<Regional> Regionales => Set<Regional>();
    public DbSet<TipoSolicitudEntity> TiposSolicitud => Set<TipoSolicitudEntity>();
    public DbSet<ModalidadTrabajo> ModalidadesTrabajo => Set<ModalidadTrabajo>();
    public DbSet<AreaCargo> AreasCargo => Set<AreaCargo>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<SolicitudComentario> SolicitudComentarios => Set<SolicitudComentario>();
    public DbSet<StateHistory> StateHistories => Set<StateHistory>();
    public DbSet<PerfilCargo> PerfilesCargo => Set<PerfilCargo>();
    public DbSet<PerfilObservacion> PerfilObservaciones => Set<PerfilObservacion>();
    public DbSet<ResumenEjecutivo> ResumenEjecutivos => Set<ResumenEjecutivo>();
    public DbSet<TipoObservacion> TiposObservacion => Set<TipoObservacion>();
    public DbSet<Vacante> Vacantes => Set<Vacante>();
    public DbSet<Postulante> Postulantes => Set<Postulante>();
    public DbSet<Postulacion> Postulaciones => Set<Postulacion>();
    public DbSet<AgentExecution> AgentExecutions => Set<AgentExecution>();
    public DbSet<Matching> Matchings => Set<Matching>();
    public DbSet<Scoring> Scorings => Set<Scoring>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Integracion> Integraciones => Set<Integracion>();
    public DbSet<ApiKeyPermiso> ApiKeyPermisos => Set<ApiKeyPermiso>();
    public DbSet<ApiKeyAuditoria> ApiKeyAuditorias => Set<ApiKeyAuditoria>();
    public DbSet<HistorialApiKey> HistorialApiKeys => Set<HistorialApiKey>();
    public DbSet<SolicitudResumen> SolicitudResumenes => Set<SolicitudResumen>();
    public DbSet<SolicitudDocumento> SolicitudDocumentos => Set<SolicitudDocumento>();
    public DbSet<AgentEvent> AgentEvents => Set<AgentEvent>();
    public DbSet<WSSession> WSSessions => Set<WSSession>();
    public DbSet<PerfilSeccion> PerfilSecciones => Set<PerfilSeccion>();
    public DbSet<PerfilAuditoria> PerfilAuditorias => Set<PerfilAuditoria>();
    public DbSet<PerfilEstructurado> PerfilEstructurados => Set<PerfilEstructurado>();
    public DbSet<PostulanteInterno> PostulantesInternos => Set<PostulanteInterno>();
    public DbSet<PostulanteExterno> PostulantesExternos => Set<PostulanteExterno>();
    public DbSet<MatchingEjecucion> MatchingEjecuciones => Set<MatchingEjecucion>();
    public DbSet<MatchingResultado> MatchingResultados => Set<MatchingResultado>();
    public DbSet<EstrategiaInterna> EstrategiaInternas => Set<EstrategiaInterna>();
    public DbSet<EstrategiaExterna> EstrategiaExternas => Set<EstrategiaExterna>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(utcConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableUtcConverter);
                }
            }
        }
    }
}
