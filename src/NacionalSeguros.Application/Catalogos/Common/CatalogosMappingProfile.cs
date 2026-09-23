using AutoMapper;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Application.Catalogos.Common;

public class CatalogosMappingProfile : Profile
{
    public CatalogosMappingProfile()
    {
        CreateMap<Catalogo, CatalogoResponse>()
            .ForMember(dest => dest.CatalogoId, opt => opt.MapFrom(src => src.Id));

        CreateMap<Parametro, ParametroResponse>()
            .ForMember(dest => dest.ParametroId, opt => opt.MapFrom(src => src.Id));

        CreateMap<Estado, EstadoResponse>()
            .ForMember(dest => dest.EstadoId, opt => opt.MapFrom(src => src.Id));

        CreateMap<Sla, SlaResponse>()
            .ForMember(dest => dest.SlaId, opt => opt.MapFrom(src => src.Id));
    }
}
