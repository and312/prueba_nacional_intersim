using Microsoft.Extensions.DependencyInjection;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Repositories;

namespace NacionalSeguros.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // Registrar Repositorios del Módulo de Seguridad
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISesionRepository, SesionRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IPermisoRepository, PermisoRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IIntegracionRepository, IntegracionRepository>();
        services.AddScoped<IApiKeyAuditoriaRepository, ApiKeyAuditoriaRepository>();
        services.AddScoped<IHistorialApiKeyRepository, HistorialApiKeyRepository>();
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<IGerenciaRepository, GerenciaRepository>();

        // Registrar Repositorios del Módulo de Catálogos Maestros
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IParametroRepository, ParametroRepository>();
        services.AddScoped<IEstadoRepository, EstadoRepository>();
        services.AddScoped<ISlaRepository, SlaRepository>();

        // Registrar Repositorios del Módulo de Gestión de Solicitudes (Módulo 03)
        services.AddScoped<ISolicitudRepository, SolicitudRepository>();
        services.AddScoped<ISolicitudResumenRepository, SolicitudResumenRepository>();
        services.AddScoped<ISolicitudDocumentoRepository, SolicitudDocumentoRepository>();

        // Registrar Repositorios del Módulo de Gestión de Perfiles (Módulo 04)
        services.AddScoped<IPerfilCargoRepository, PerfilCargoRepository>();
        services.AddScoped<ITipoObservacionRepository, TipoObservacionRepository>();

        // Registrar Repositorios del Módulo de Gestión de Vacantes (Módulo 05)
        services.AddScoped<IVacanteRepository, VacanteRepository>();

        // Registrar Repositorios del Módulo de Gestión de Postulantes (Módulo 06)
        services.AddScoped<IPostulanteRepository, PostulanteRepository>();

        // Registrar Repositorios del Módulo de Matching Inteligente (Módulo 07)
        services.AddScoped<IMatchingRepository, MatchingRepository>();
        services.AddScoped<Domain.Services.IIAProvider, Services.MockIAProvider>();

        // Registrar Interceptores de Contexto de Base de Datos
        services.AddHttpContextAccessor();
        services.AddScoped<Interceptors.SetSessionContextInterceptor>();
        services.AddScoped<Interceptors.PublishDomainEventsInterceptor>();

        return services;
    }
}
