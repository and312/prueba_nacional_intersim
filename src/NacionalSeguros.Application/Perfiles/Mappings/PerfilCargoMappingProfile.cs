using AutoMapper;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Application.Perfiles.Mappings;

public class PerfilCargoMappingProfile : Profile
{
    public PerfilCargoMappingProfile()
    {
        CreateMap<PerfilCargo, PerfilResponseDto>()
            .ConstructUsing(p => new PerfilResponseDto(
                p.Id,
                p.SolicitudId,
                p.Cargo,
                p.Descripcion,
                p.Version,
                p.Estado != null ? p.Estado.Nombre : string.Empty,
                p.Estado != null ? p.Estado.Codigo : string.Empty,
                p.Solicitud != null && p.Solicitud.Solicitante != null && p.Solicitud.Solicitante.Area != null ? p.Solicitud.Solicitante.Area.Nombre : string.Empty,
                p.Solicitud != null && p.Solicitud.Solicitante != null ? p.Solicitud.Solicitante.Correo : string.Empty,
                p.CreatedDate,
                p.ModifiedDate,
                p.Solicitud != null ? p.Solicitud.Codigo : string.Empty,
                p.PdfUrl,
                p.Activo
            ));

        CreateMap<PerfilAuditoria, PerfilAuditoriaResponseDto>();
    }
}
