using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands;

public record ActualizarResumenCommand(
    int PerfilId, 
    int SolicitudId, 
    ResumenEjecutivoRolDto ResumenEjecutivoRol, 
    string ModificadoPor, 
    bool EsAutomatizacion
) : IRequest<Result>;

public record RegistrarObservacionPerfilCommand(int PerfilId, int TipoObservacionId, string Comentario, int UsuarioSolicitanteId, string ModificadoPor) : IRequest<Result>;
public record EnviarObservacionesSolicitanteCommand(int PerfilId, int UsuarioSolicitanteId, string ModificadoPor) : IRequest<Result>;

public record AtenderObservacionesPerfilCommand(int PerfilId, int RrhhUsuarioId, string ModificadoPor) : IRequest<Result>;

public record AprobarSolicitantePerfilCommand(int PerfilId, int UsuarioSolicitanteId, string ModificadoPor) : IRequest<Result>;

public record EnviarAreaPerfilCommand(int PerfilId, int RrhhUsuarioId, string ModificadoPor) : IRequest<Result>;

public record AprobarFinalPerfilCommand(int PerfilId, int RrhhUsuarioId, string ModificadoPor) : IRequest<Result>;

// Catalog Commands
public record CrearTipoObservacionCommand(string Codigo, string Nombre, string? Descripcion, string CreadoPor) : IRequest<Result>;
public record ActualizarTipoObservacionCommand(int Id, string Nombre, string? Descripcion, string ModificadoPor) : IRequest<Result>;
public record ActivarTipoObservacionCommand(int Id, string ModificadoPor) : IRequest<Result>;
public record InactivarTipoObservacionCommand(int Id, string ModificadoPor) : IRequest<Result>;

public class PerfilCommandsHandler
    : IRequestHandler<ActualizarResumenCommand, Result>,
      IRequestHandler<RegistrarObservacionPerfilCommand, Result>,
      IRequestHandler<EnviarObservacionesSolicitanteCommand, Result>,
      IRequestHandler<AtenderObservacionesPerfilCommand, Result>,
      IRequestHandler<AprobarSolicitantePerfilCommand, Result>,
      IRequestHandler<EnviarAreaPerfilCommand, Result>,
      IRequestHandler<AprobarFinalPerfilCommand, Result>,
      IRequestHandler<CrearTipoObservacionCommand, Result>,
      IRequestHandler<ActualizarTipoObservacionCommand, Result>,
      IRequestHandler<ActivarTipoObservacionCommand, Result>,
      IRequestHandler<InactivarTipoObservacionCommand, Result>
{
    private readonly IPerfilCargoRepository _perfilRepository;
    private readonly ITipoObservacionRepository _tipoObservacionRepository;
    private readonly IEstadoRepository _estadoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PerfilCommandsHandler(
        IPerfilCargoRepository perfilRepository,
        ITipoObservacionRepository tipoObservacionRepository,
        IEstadoRepository estadoRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork)
    {
        _perfilRepository = perfilRepository ?? throw new ArgumentNullException(nameof(perfilRepository));
        _tipoObservacionRepository = tipoObservacionRepository ?? throw new ArgumentNullException(nameof(tipoObservacionRepository));
        _estadoRepository = estadoRepository ?? throw new ArgumentNullException(nameof(estadoRepository));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    private async Task RegistrarHistorialEstado(int perfilId, int estadoAnteriorId, int estadoNuevoId, int? usuarioId, string? comentario, string? rol = null)
    {
        var history = new StateHistory(
            "Perfil",
            perfilId,
            estadoAnteriorId,
            estadoNuevoId,
            usuarioId ?? 1, // Fallback al Administrador General si es un proceso automático del sistema
            comentario,
            Guid.NewGuid(),
            null,
            rol
        );
        await _perfilRepository.AddStateHistoryAsync(history);
    }

    public async Task<Result> Handle(ActualizarResumenCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilRepository.GetByIdAsync(request.PerfilId);

        if (perfil == null)
            return Result.Failure(new Error("Perfil.NotFound", "No se encontró el perfil."));

        // Validar que coincida la SolicitudId
        if (perfil.SolicitudId != request.SolicitudId)
            return Result.Failure(new Error("Perfil.Validation", "La solicitud no coincide con el perfil especificado."));

        var rol = request.ResumenEjecutivoRol;
        string funcionesJson = JsonSerializer.Serialize(rol.FuncionesPrincipales);
        string requisitosJson = JsonSerializer.Serialize(rol.RequisitosMinimos);
        string hardSkillsJson = JsonSerializer.Serialize(rol.HardSkills);
        string softSkillsJson = JsonSerializer.Serialize(rol.SoftSkills);

        var resumen = await _perfilRepository.GetResumenByPerfilCargoIdAsync(perfil.Id);
        if (resumen == null)
        {
            resumen = new ResumenEjecutivo(
                perfil.Id,
                rol.Resumen,
                rol.ObjetivoCargo,
                funcionesJson,
                requisitosJson,
                rol.FormacionExperiencia,
                hardSkillsJson,
                softSkillsJson,
                rol.Modalidad,
                rol.Ubicacion,
                rol.BandaSalarial,
                rol.CriteriosEvaluacion,
                rol.CaracteristicasClave,
                rol.ValoracionPerfil,
                request.ModificadoPor
            );
            await _perfilRepository.AddResumenAsync(resumen);
        }
        else
        {
            resumen.Actualizar(
                rol.Resumen,
                rol.ObjetivoCargo,
                funcionesJson,
                requisitosJson,
                rol.FormacionExperiencia,
                hardSkillsJson,
                softSkillsJson,
                rol.Modalidad,
                rol.Ubicacion,
                rol.BandaSalarial,
                rol.CriteriosEvaluacion,
                rol.CaracteristicasClave,
                rol.ValoracionPerfil,
                request.ModificadoPor
            );
            _perfilRepository.UpdateResumen(resumen);
        }

        if (request.EsAutomatizacion)
        {
            int oldEstado = perfil.EstadoId;
            int newEstado = perfil.EstadoId;
            string comment = "Resumen Ejecutivo generado por el AGENTE";
            
            if (perfil.Estado?.Codigo == "PERF-PEN-GEN")
            {
                var nextEstado = await _estadoRepository.GetByCodigoAsync("PERF-REV-RRHH");
                if (nextEstado != null)
                {
                    newEstado = nextEstado.Id;
                    perfil.CambiarEstado(newEstado, request.ModificadoPor);
                    comment = "Generación inicial de resumen ejecutivo por Automatización";
                }
            }

            var systemUser = await _usuarioRepository.GetByCorreoAsync("system@nacionalseguros.com.bo");
            int? systemUserId = systemUser?.Id;
            await RegistrarHistorialEstado(perfil.Id, oldEstado, newEstado, systemUserId, comment, "System");
        }
        else if (!request.EsAutomatizacion)
        {
            // RRHH actualizando resumen. Si estaba en revisión RRHH o en corrección RRHH, pasamos a Resumen Ejecutivo Generado
            if (perfil.Estado?.Codigo == "PERF-REV-RRHH" || perfil.Estado?.Codigo == "PERF-COR-RRHH")
            {
                var nextEstado = await _estadoRepository.GetByCodigoAsync("PERF-RES-GEN");
                if (nextEstado != null)
                {
                    int oldEstado = perfil.EstadoId;
                    perfil.CambiarEstado(nextEstado.Id, request.ModificadoPor);
                    var user = await _usuarioRepository.GetByCorreoAsync(request.ModificadoPor);
                    await RegistrarHistorialEstado(perfil.Id, oldEstado, nextEstado.Id, user?.Id, "Borrador de resumen aprobado por RRHH", "RRHH");
                }
            }
        }

        _perfilRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(RegistrarObservacionPerfilCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilRepository.GetByIdWithDetailsAsync(request.PerfilId);

        if (perfil == null)
            return Result.Failure(new Error("Perfil.NotFound", "No se encontró el perfil."));

        if (perfil.Estado?.Codigo != "PERF-REV-AREA" && perfil.Estado?.Codigo != "PERF-OBS-AREA")
            return Result.Failure(new Error("Perfil.InvalidState", "El perfil debe estar en revisión del área o ya observado para registrar observaciones."));

        // Validar que el usuario sea el solicitante de la solicitud o un rol RRHH/Administrador
        if (perfil.Solicitud.SolicitanteId != request.UsuarioSolicitanteId)
        {
            var user = await _usuarioRepository.GetByIdAsync(request.UsuarioSolicitanteId);
            bool isRrhhOrAdmin = user != null && (user.Roles.Any(r => r.Nombre == "RRHH" || r.Nombre == "Administrador") || user.Area?.Nombre == "Recursos Humanos");
            if (!isRrhhOrAdmin)
            {
                return Result.Failure(new Error("Perfil.Forbidden", "Solo el solicitante asignado al área o personal de RRHH/Administrador puede registrar observaciones."));
            }
        }

        // Validar tipo de observación
        var tipoObs = await _tipoObservacionRepository.GetByIdAsync(request.TipoObservacionId);
        if (tipoObs == null || tipoObs.Estado != "Activo")
            return Result.Failure(new Error("TipoObservacion.Invalid", "El tipo de observación no existe o no está activo."));

        // Determinar iteración actual
        int iteracion = await _perfilRepository.GetMaxObservacionIteracionAsync(perfil.Id) + 1;

        var observacion = new PerfilObservacion(
            perfil.Id,
            request.TipoObservacionId,
            request.Comentario,
            request.UsuarioSolicitanteId,
            iteracion
        );

        await _perfilRepository.AddObservacionAsync(observacion);

        // Al agregar observaciones en borrador, NO se cambia el estado del perfil.
        // El perfil permanece en PERF-REV-AREA hasta que el solicitante presione "Enviar observaciones".

        _perfilRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(EnviarObservacionesSolicitanteCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilRepository.GetByIdWithDetailsAsync(request.PerfilId);

        if (perfil == null)
            return Result.Failure(new Error("Perfil.NotFound", "No se encontró el perfil."));

        if (perfil.Estado?.Codigo != "PERF-OBS-AREA" && perfil.Estado?.Codigo != "PERF-REV-AREA")
            return Result.Failure(new Error("Perfil.InvalidState", "El perfil debe estar en revisión u observado por el área para enviar las observaciones."));

        if (perfil.Solicitud.SolicitanteId != request.UsuarioSolicitanteId)
        {
            var user = await _usuarioRepository.GetByIdAsync(request.UsuarioSolicitanteId);
            bool isRrhhOrAdmin = user != null && (user.Roles.Any(r => r.Nombre == "RRHH" || r.Nombre == "Administrador") || user.Area?.Nombre == "Recursos Humanos");
            if (!isRrhhOrAdmin)
            {
                return Result.Failure(new Error("Perfil.Forbidden", "Solo el solicitante asignado al área o personal de RRHH/Administrador puede enviar las observaciones."));
            }
        }

        var pendientes = await _perfilRepository.GetPendientesByPerfilIdAsync(perfil.Id);
        if (pendientes == null || !pendientes.Any())
        {
            return Result.Failure(new Error("Perfil.NoPendingObservations", "Debe registrar al menos una observación antes de enviar a RRHH."));
        }

        // Cambiar estado a PERF-OBS-AREA ("Observado por el Área Solicitante") al hacer clic en "Enviar observaciones"
        var nextEstado = await _estadoRepository.GetByCodigoAsync("PERF-OBS-AREA");
        if (nextEstado != null && perfil.EstadoId != nextEstado.Id)
        {
            int oldEstado = perfil.EstadoId;
            int iteracion = await _perfilRepository.GetMaxObservacionIteracionAsync(perfil.Id);
            
            string obsDetail = $"Se registraron {pendientes.Count()} observaciones sobre las secciones:\n" +
                               string.Join("\n", pendientes.Select(o => $"• {o.TipoObservacion?.Nombre ?? "Observación"}: {o.Comentario}"));

            perfil.CambiarEstado(nextEstado.Id, request.ModificadoPor);
            await RegistrarHistorialEstado(perfil.Id, oldEstado, nextEstado.Id, request.UsuarioSolicitanteId, obsDetail, "Solicitante");
        }

        _perfilRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(AtenderObservacionesPerfilCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilRepository.GetByIdWithDetailsAsync(request.PerfilId);

        if (perfil == null)
            return Result.Failure(new Error("Perfil.NotFound", "No se encontró el perfil."));

        if (perfil.Estado?.Codigo != "PERF-OBS-AREA")
            return Result.Failure(new Error("Perfil.InvalidState", "Solo se pueden atender observaciones de perfiles en estado Observado."));

        // Marcar observaciones pendientes como atendidas
        var pendientes = await _perfilRepository.GetPendientesByPerfilIdAsync(perfil.Id);

        foreach (var obs in pendientes)
        {
            obs.Atender(request.RrhhUsuarioId);
        }

        // Cambiar estado a PERF-COR-RRHH
        var nextEstado = await _estadoRepository.GetByCodigoAsync("PERF-COR-RRHH");
        if (nextEstado != null)
        {
            int oldEstado = perfil.EstadoId;
            perfil.CambiarEstado(nextEstado.Id, request.ModificadoPor);
            await RegistrarHistorialEstado(perfil.Id, oldEstado, nextEstado.Id, request.RrhhUsuarioId, "Observaciones atendidas e inicio de corrección por RRHH", "RRHH");
        }

        _perfilRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(AprobarSolicitantePerfilCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilRepository.GetByIdWithDetailsAsync(request.PerfilId);

        if (perfil == null)
            return Result.Failure(new Error("Perfil.NotFound", "No se encontró el perfil."));

        if (perfil.Estado?.Codigo != "PERF-REV-AREA")
            return Result.Failure(new Error("Perfil.InvalidState", "El perfil debe estar en revisión del área para su aprobación."));

        if (perfil.Solicitud.SolicitanteId != request.UsuarioSolicitanteId)
        {
            var user = await _usuarioRepository.GetByIdAsync(request.UsuarioSolicitanteId);
            bool isRrhhOrAdmin = user != null && (user.Roles.Any(r => r.Nombre == "RRHH" || r.Nombre == "Administrador") || user.Area?.Nombre == "Recursos Humanos");
            if (!isRrhhOrAdmin)
            {
                return Result.Failure(new Error("Perfil.Forbidden", "Solo el solicitante asignado o personal de RRHH/Administrador puede aprobar el perfil."));
            }
        }

        var pendientesAprobar = await _perfilRepository.GetPendientesByPerfilIdAsync(perfil.Id);
        if (pendientesAprobar != null && pendientesAprobar.Any())
        {
            return Result.Failure(new Error("Perfil.HasPendingObservations", "Tiene observaciones registradas pendientes por enviar a RRHH."));
        }

        // Cambiar estado a PERF-APR-AREA
        var nextEstado = await _estadoRepository.GetByCodigoAsync("PERF-APR-AREA");
        if (nextEstado != null)
        {
            int oldEstado = perfil.EstadoId;
            perfil.CambiarEstado(nextEstado.Id, request.ModificadoPor);
            await RegistrarHistorialEstado(perfil.Id, oldEstado, nextEstado.Id, request.UsuarioSolicitanteId, "Aprobado por el Área Solicitante", "Solicitante");
        }

        _perfilRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(EnviarAreaPerfilCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[TEST-LOG-IDEMPOTENT] Iniciando EnviarAreaPerfilCommand para PerfilId: {request.PerfilId}, RrhhUsuarioId: {request.RrhhUsuarioId}, ModificadoPor: {request.ModificadoPor}");

        var perfil = await _perfilRepository.GetByIdWithDetailsAsync(request.PerfilId);

        if (perfil == null)
            return Result.Failure(new Error("Perfil.NotFound", "No se encontró el perfil."));

        Console.WriteLine($"[TEST-LOG-IDEMPOTENT] PerfilId: {perfil.Id}, EstadoId: {perfil.EstadoId}, EstadoCodigo: {perfil.Estado?.Codigo}");

        // Regla idempotente:
        // - Si el estado actual es PERF-REV-AREA, retornar éxito sin volver a transicionar
        if (perfil.Estado?.Codigo == "PERF-REV-AREA")
        {
            Console.WriteLine($"[TEST-LOG-IDEMPOTENT] Perfil ya se encuentra en estado PERF-REV-AREA. Retornando éxito de forma idempotente.");
            return Result.Success();
        }

        // - Si está en PERF-RES-GEN, PERF-COR-RRHH o PERF-OBS-AREA, ejecutar la transición normal
        if (perfil.Estado?.Codigo != "PERF-RES-GEN" && perfil.Estado?.Codigo != "PERF-COR-RRHH" && perfil.Estado?.Codigo != "PERF-OBS-AREA")
        {
            return Result.Failure(new Error("Perfil.InvalidState", "El perfil debe estar en estado Resumen Generado, Corrección u Observado para enviarse al área."));
        }

        if (perfil.Estado?.Codigo == "PERF-OBS-AREA")
        {
            var pendientes = await _perfilRepository.GetPendientesByPerfilIdAsync(perfil.Id);
            if (pendientes != null && pendientes.Any())
            {
                return Result.Failure(new Error("Perfil.PendingObservations", "No se puede enviar el perfil al Área Solicitante mientras existan observaciones pendientes por resolver."));
            }
        }

        // Cambiar estado a PERF-REV-AREA
        var nextEstado = await _estadoRepository.GetByCodigoAsync("PERF-REV-AREA");
        if (nextEstado != null)
        {
            int oldEstado = perfil.EstadoId;
            
            string comment = "Perfil estructurado enviado para revisión del Área Solicitante.";
            if (perfil.Estado?.Codigo == "PERF-COR-RRHH")
            {
                int maxIter = await _perfilRepository.GetMaxObservacionIteracionAsync(perfil.Id);
                var allObs = await _perfilRepository.GetObservacionesByPerfilIdAsync(perfil.Id);
                var resolvedObs = allObs
                    .Where(o => o.NumeroIteracion == maxIter && o.EstadoObservacion == "Atendida")
                    .ToList();

                if (resolvedObs.Any())
                {
                    comment = "RRHH subsanó las observaciones registradas:\n" + 
                              string.Join("\n", resolvedObs.Select(o => $"✓ {o.TipoObservacion?.Nombre ?? "Observación"}: {o.Comentario}"));
                }
            }

            perfil.CambiarEstado(nextEstado.Id, request.ModificadoPor);
            perfil.IncrementarVersion();
            await RegistrarHistorialEstado(perfil.Id, oldEstado, nextEstado.Id, request.RrhhUsuarioId, comment, "RRHH");
        }

        _perfilRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(AprobarFinalPerfilCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilRepository.GetByIdWithDetailsAsync(request.PerfilId);

        if (perfil == null)
            return Result.Failure(new Error("Perfil.NotFound", "No se encontró el perfil."));

        if (perfil.Estado?.Codigo != "PERF-APR-AREA" && perfil.Estado?.Codigo != "PERF-REV-RRHH")
            return Result.Failure(new Error("Perfil.InvalidState", "El perfil debe estar aprobado por el área o en revisión de RRHH para su aprobación final."));

        bool aprobadoPorArea = await _perfilRepository.WasApprovedByAreaAsync(perfil.Id);
        if (!aprobadoPorArea)
            return Result.Failure(new Error("Perfil.NotApprovedByArea", "El perfil debe estar aprobado por el área antes del cierre final de RRHH."));

        // Cambiar estado a PERF-APR-FIN
        var nextEstado = await _estadoRepository.GetByCodigoAsync("PERF-APR-FIN");
        if (nextEstado != null)
        {
            int oldEstado = perfil.EstadoId;
            perfil.CambiarEstado(nextEstado.Id, request.ModificadoPor);
            await RegistrarHistorialEstado(perfil.Id, oldEstado, nextEstado.Id, request.RrhhUsuarioId, "Aprobación final del perfil realizada con éxito por RRHH", "RRHH");
        }

        _perfilRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    // Catalog Handlers
    public async Task<Result> Handle(CrearTipoObservacionCommand request, CancellationToken cancellationToken)
    {
        var existing = await _tipoObservacionRepository.GetByCodigoAsync(request.Codigo);
        if (existing != null)
            return Result.Failure(new Error("TipoObservacion.Duplicate", "Ya existe un tipo de observación con ese código."));

        var entity = new TipoObservacion(request.Codigo, request.Nombre, request.Descripcion);
        entity.SetAuditoriaCreacion(request.CreadoPor);

        await _tipoObservacionRepository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(ActualizarTipoObservacionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _tipoObservacionRepository.GetByIdAsync(request.Id);
        if (entity == null)
            return Result.Failure(new Error("TipoObservacion.NotFound", "No se encontró el tipo de observación."));

        entity.Actualizar(request.Nombre, request.Descripcion);
        entity.SetAuditoriaModificacion(request.ModificadoPor);

        _tipoObservacionRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(ActivarTipoObservacionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _tipoObservacionRepository.GetByIdAsync(request.Id);
        if (entity == null)
            return Result.Failure(new Error("TipoObservacion.NotFound", "No se encontró el tipo de observación."));

        entity.Reactivar();
        entity.SetAuditoriaModificacion(request.ModificadoPor);

        _tipoObservacionRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(InactivarTipoObservacionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _tipoObservacionRepository.GetByIdAsync(request.Id);
        if (entity == null)
            return Result.Failure(new Error("TipoObservacion.NotFound", "No se encontró el tipo de observación."));

        entity.SetEstado("Inactivo");
        entity.SetAuditoriaModificacion(request.ModificadoPor);

        _tipoObservacionRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
