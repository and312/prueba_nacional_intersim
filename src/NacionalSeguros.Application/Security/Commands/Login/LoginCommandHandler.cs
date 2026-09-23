using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISesionRepository _sesionRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IActiveDirectoryService _adService;
    private readonly IAuditService _auditService;

    public LoginCommandHandler(
        IUsuarioRepository usuarioRepository,
        ISesionRepository sesionRepository,
        IRolRepository rolRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IActiveDirectoryService adService,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _sesionRepository = sesionRepository;
        _rolRepository = rolRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _adService = adService;
        _auditService = auditService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Usuario? usuario = await _usuarioRepository.GetByCorreoAsync(request.Correo);

        bool isAuthenticated = false;

        if (request.TipoAutenticacion == "ActiveDirectory")
        {
            isAuthenticated = await _adService.AuthenticateAsync(request.Correo, request.Clave);

            if (isAuthenticated && usuario == null)
            {
                // Auto-aprovisionar usuario corporativo AD
                string nombre = request.Correo.Split('@')[0];
                usuario = new Usuario(nombre, request.Correo, TipoAutenticacion.ActiveDirectory);
                
                // Asignar rol predeterminado de 'Solicitante'
                var roles = await _rolRepository.ListAsync();
                var rolSolicitante = roles.FirstOrDefault(r => r.Nombre.Equals("Solicitante", StringComparison.OrdinalIgnoreCase))
                                     ?? roles.FirstOrDefault();
                if (rolSolicitante != null)
                {
                    usuario.AsignarRol(rolSolicitante);
                }

                await _usuarioRepository.AddAsync(usuario);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            if (usuario != null && usuario.TipoAutenticacion == TipoAutenticacion.Local && usuario.ClaveHash != null)
            {
                if (usuario.ResetPasswordToken != null && usuario.ResetPasswordTokenExpiration > DateTime.UtcNow)
                {
                    return Result.Failure<LoginResponseDto>(new Error("PASSWORD_RESET_PENDING", "Tiene un proceso de restablecimiento de contraseña pendiente. Por favor, revise su correo o complete el proceso."));
                }

                isAuthenticated = _passwordHasher.VerifyPassword(request.Clave, usuario.ClaveHash);
            }
        }

        if (!isAuthenticated || usuario == null || usuario.IsDeleted)
        {
            // Auditar intento fallido
            await _auditService.LogActionAsync(
                usuarioId: usuario?.Id,
                usuarioNombre: usuario?.Nombre ?? request.Correo,
                rol: usuario?.Roles?.FirstOrDefault()?.Nombre ?? "Ninguno",
                modulo: "Seguridad",
                entidad: "Usuarios",
                entidadId: usuario?.Id ?? 0,
                accion: "Login fallido",
                estadoAnterior: null,
                estadoNuevo: null,
                canal: "API",
                correlationId: null);

            return Result.Failure<LoginResponseDto>(new Error("INVALID_CREDENTIALS", "Usuario o contraseña incorrectos."));
        }

        if (usuario.Estado == UsuarioEstado.Inactivo)
        {
            return Result.Failure<LoginResponseDto>(new Error("USER_LOCKED", "La cuenta se encuentra inhabilitada."));
        }

        // Si MFA está habilitado en este usuario, devolver indicador
        if (usuario.MfaHabilitado)
        {
            return Result.Success(new LoginResponseDto
            {
                MfaRequerido = true,
                Usuario = MapToDto(usuario)
            });
        }

        // Si no requiere MFA, generar sesión y tokens
        string accessToken = _jwtService.GenerateToken(usuario);
        string refreshToken = _jwtService.GenerateRefreshToken();

        // Expira dinámicamente usando IJwtService
        var sesion = new Sesion(usuario.Id, refreshToken, DateTime.UtcNow.AddDays(_jwtService.RefreshTokenExpiracionDias));
        await _sesionRepository.AddAsync(sesion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Registrar auditoría de sesión exitosa
        await _auditService.LogActionAsync(
            usuarioId: usuario.Id,
            usuarioNombre: usuario.Nombre,
            rol: usuario.Roles.FirstOrDefault()?.Nombre ?? "Ninguno",
            modulo: "Seguridad",
            entidad: "Sesiones",
            entidadId: (int)sesion.Id,
            accion: "Login exitoso",
            estadoAnterior: null,
            estadoNuevo: $"SesionId: {sesion.Id}",
            canal: "API",
            correlationId: null);

        return Result.Success(new LoginResponseDto
        {
            Token = accessToken,
            ExpiraEnSegundos = _jwtService.TokenExpiracionSegundos,
            MfaRequerido = false,
            RefreshToken = refreshToken,
            Usuario = MapToDto(usuario)
        });
    }

    private UsuarioResponseDto MapToDto(Usuario u)
    {
        return new UsuarioResponseDto
        {
            UsuarioId = u.Id,
            Correo = u.Correo,
            Nombres = string.IsNullOrEmpty(u.Nombres) ? u.Nombre : u.Nombres,
            Apellidos = u.Apellidos ?? string.Empty,
            TipoAutenticacion = u.TipoAutenticacion.ToString(),
            Activo = u.Estado == UsuarioEstado.Activo,
            AreaId = u.AreaId,
            AreaNombre = u.Area?.Nombre ?? string.Empty,
            Cargo = u.Cargo,
            Gerencia = u.Gerencia,
            Telefono = u.Telefono,
            Extension = u.Extension,
            FotografiaUrl = u.FotografiaUrl,
            Roles = u.Roles.Select(r => r.Nombre).ToList(),
            Permisos = u.Roles.SelectMany(r => r.Permisos).Select(p => p.Codigo).Distinct().ToList()
        };
    }
}
