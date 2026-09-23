using System.Linq;
using AutoMapper;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Enums;

namespace NacionalSeguros.Application.Security.Common;

public class SecurityMappingProfile : Profile
{
    public SecurityMappingProfile()
    {
        CreateMap<Usuario, UsuarioResponseDto>()
            .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.Nombres ?? src.Nombre))
            .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.Apellidos ?? string.Empty))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Estado == UsuarioEstado.Activo))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.AreaId, opt => opt.MapFrom(src => src.AreaId))
            .ForMember(dest => dest.AreaNombre, opt => opt.MapFrom(src => src.Area != null ? src.Area.Nombre : string.Empty))
            .ForMember(dest => dest.Cargo, opt => opt.MapFrom(src => src.Cargo))
            .ForMember(dest => dest.Gerencia, opt => opt.MapFrom(src => src.Gerencia))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono))
            .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => src.Extension))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
            .ForMember(dest => dest.FotografiaUrl, opt => opt.MapFrom(src => src.FotografiaUrl))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(r => r.Nombre).ToList()))
            .ForMember(dest => dest.Permisos, opt => opt.MapFrom(src => src.Roles.SelectMany(r => r.Permisos).Select(p => p.Codigo).Distinct().ToList()));

        CreateMap<Rol, RolResponseDto>()
            .ForMember(dest => dest.RolId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => !src.IsDeleted))
            .ForMember(dest => dest.CantidadUsuarios, opt => opt.MapFrom(src => src.Usuarios.Count(u => !u.IsDeleted)))
            .ForMember(dest => dest.Permisos, opt => opt.MapFrom(src => src.Permisos.Select(p => p.Codigo).ToList()));

        CreateMap<Area, AreaResponseDto>()
            .ForMember(dest => dest.AreaId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.GerenciaId, opt => opt.MapFrom(src => src.GerenciaId))
            .ForMember(dest => dest.Gerencia, opt => opt.MapFrom(src => src.Gerencia != null ? src.Gerencia.Nombre : string.Empty))
            .ForMember(dest => dest.CantidadUsuarios, opt => opt.MapFrom(src => src.Usuarios.Count(u => !u.IsDeleted)))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate));

        CreateMap<Gerencia, GerenciaResponseDto>()
            .ForMember(dest => dest.GerenciaId, opt => opt.MapFrom(src => src.Id));
    }
}
