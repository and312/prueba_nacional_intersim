using AutoMapper;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Application.Vacantes.Mappings;

public class VacanteMappingProfile : Profile
{
    public VacanteMappingProfile()
    {
        CreateMap<Vacante, VacanteResponseDto>()
            .ConstructUsing(src => new VacanteResponseDto(
                src.Id,
                src.SolicitudId,
                src.PerfilCargoId,
                src.Estado != null ? src.Estado.Nombre : string.Empty,
                null // Removida regla de fecha sustituta
            ));
    }
}
