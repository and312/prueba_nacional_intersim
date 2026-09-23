using System;
using System.Linq;
using AutoMapper;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Application.Solicitudes.Mappings;

public class SolicitudMappingProfile : Profile
{
    public SolicitudMappingProfile()
    {
        CreateMap<Solicitud, SolicitudResponseDto>()
            .ConstructUsing(s => new SolicitudResponseDto(
                s.Id,
                s.Cargo,
                s.Solicitante != null && s.Solicitante.Area != null ? s.Solicitante.Area.Nombre : "Sin Área",
                s.SolicitanteId,
                s.DecisorId,
                s.Seniority,
                s.Prioridad,
                s.Funciones,
                s.Estado != null ? s.Estado.Nombre : string.Empty,
                s.CreatedDate,
                s.Regional != null ? new CatalogValueDto(s.Regional.Id, s.Regional.Codigo, s.Regional.Nombre) : null,
                s.TipoSolicitud != null ? new CatalogValueDto(s.TipoSolicitud.Id, s.TipoSolicitud.Codigo, s.TipoSolicitud.Nombre) : null,
                s.ModalidadTrabajo != null ? new CatalogValueDto(s.ModalidadTrabajo.Id, s.ModalidadTrabajo.Codigo, s.ModalidadTrabajo.Nombre) : null,
                s.Solicitante != null ? s.Solicitante.Nombre : null,
                s.Solicitante != null ? s.Solicitante.Correo : null,
                s.Solicitante != null ? s.Solicitante.Cargo : null,
                s.Decisor != null ? s.Decisor.Nombre : null,
                s.Estado != null ? s.Estado.Codigo : null,
                s.CanalOrigen,
                s.Codigo,
                s.Motivo,
                s.CantidadVacantes,
                s.Observaciones,

                // Perfil Requerido
                s.ObjetivoCargo,
                s.FormacionAcademica,
                s.ExperienciaMinima,
                s.ExperienciaIndispensable,
                s.ConocimientosTecnicos,
                s.HerramientasSistemas,
                s.CompetenciasClave,
                s.DisponibilidadRequerida,
                s.CriteriosExcluyentes,
                s.CriteriosDeseables,

                null, // CompletitudPorcentaje (calculado o asignado por query handler)
                null, // CamposDetectados
                null, // CamposEsperados
                null, // ProfileSummary
                null, // CaptureState
                null, // PdfDocumentUrl
                null, // JustificacionRechazo
                s.Solicitante != null ? s.Solicitante.UltimaInteraccionN8N : null,
                s.Solicitante != null ? s.Solicitante.Telefono : null,
                s.Comentarios != null && s.Comentarios.Any() ? s.Comentarios.OrderByDescending(c => c.Fecha).First().Texto : null,
                s.ModifiedBy ?? (s.Solicitante != null ? s.Solicitante.Correo : "SYSTEM"),
                s.ModifiedDate,
                System.Array.Empty<ObservacionResponseDto>(),
                System.Array.Empty<SolicitudComentarioResponseDto>(),
                null,
                null
            ))
            .ForMember(dest => dest.Comentarios, opt => opt.Ignore())
            .ForMember(dest => dest.ObservacionesRrhh, opt => opt.Ignore())
            .ForMember(dest => dest.UltimaObservacionRRHH, opt => opt.Ignore())
            .ForMember(dest => dest.Regional, opt => opt.Ignore())
            .ForMember(dest => dest.TipoSolicitud, opt => opt.Ignore())
            .ForMember(dest => dest.ModalidadTrabajo, opt => opt.Ignore());

        CreateMap<StateHistory, StateHistoryResponseDto>()
            .ConstructUsing(sh => new StateHistoryResponseDto(
                sh.Id,
                sh.EntidadId,
                sh.EstadoAnterior != null ? sh.EstadoAnterior.Nombre : string.Empty,
                sh.EstadoNuevo != null ? sh.EstadoNuevo.Nombre : string.Empty,
                sh.Usuario != null ? sh.Usuario.Nombre : "Sistema",
                sh.Fecha,
                sh.Comentario ?? string.Empty,
                sh.Rol ?? (sh.Usuario != null && sh.Usuario.Roles.Any() ? string.Join(", ", sh.Usuario.Roles.Select(r => r.Nombre)) : "Sistema"),
                null,
                null,
                sh.Iteracion,
                sh.CorrelationId
            ))
            .ForMember(dest => dest.EstadoAnterior, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoNuevo, opt => opt.Ignore());
    }
}
