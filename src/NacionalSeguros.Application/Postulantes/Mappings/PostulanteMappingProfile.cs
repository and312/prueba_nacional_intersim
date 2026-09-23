using AutoMapper;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Application.Postulantes.Mappings;

public class PostulanteMappingProfile : Profile
{
    public PostulanteMappingProfile()
    {
        CreateMap<Postulante, PostulanteResponseDto>()
            .ConstructUsing(src => new PostulanteResponseDto(
                src.Id,
                src.Correo,
                "Registrado",
                src.CreatedDate
            ));

        CreateMap<Postulacion, PostulanteResponseDto>()
            .ConstructUsing(src => new PostulanteResponseDto(
                src.PostulanteId,
                src.Postulante != null ? src.Postulante.Correo : string.Empty,
                src.EstadoPipeline != null ? src.EstadoPipeline.Nombre : "Registrado",
                src.FechaPostulacion
            ));
    }
}
