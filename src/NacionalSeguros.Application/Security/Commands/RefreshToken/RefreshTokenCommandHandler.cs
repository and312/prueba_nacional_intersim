using System;
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

namespace NacionalSeguros.Application.Security.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;

    public RefreshTokenCommandHandler(
        ISesionRepository sesionRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IAuditService auditService)
    {
        _sesionRepository = sesionRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _auditService = auditService;
    }

    public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var sesionExistente = await _sesionRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (sesionExistente == null)
        {
            return Result.Failure<LoginResponseDto>(new Error("TOKEN_INVALID", "El Refresh Token no existe."));
        }

        var usuario = await _usuarioRepository.GetByIdAsync(sesionExistente.UsuarioId);
        if (usuario == null || usuario.IsDeleted || usuario.Estado == UsuarioEstado.Inactivo)
        {
            return Result.Failure<LoginResponseDto>(new Error("USER_NOT_FOUND", "Usuario no encontrado o inactivo."));
        }

        // CONTROL DE DETECCIÓN DE REUSO DE REFRESH TOKENS (RTR - Security Breach Mitigation)
        if (!sesionExistente.Activa)
        {
            // Auditar brecha de seguridad grave
            await _auditService.LogActionAsync(
                usuarioId: usuario.Id,
                usuarioNombre: usuario.Nombre,
                rol: usuario.Roles.FirstOrDefault()?.Nombre ?? "Ninguno",
                modulo: "Seguridad",
                entidad: "Sesiones",
                entidadId: (int)sesionExistente.Id,
                accion: "TOKEN_REUSE_DETECTED",
                estadoAnterior: "Inactiva",
                estadoNuevo: "Intento de reuso ilegal detectado. Anulando todas las sesiones activas del usuario.",
                canal: "API",
                correlationId: null);

            // Anular inmediatamente todas las sesiones del usuario
            var sesionesActivas = await _sesionRepository.GetActiveSessionsByUsuarioIdAsync(usuario.Id);
            foreach (var s in sesionesActivas)
            {
                s.Desactivar();
                _sesionRepository.Update(s);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<LoginResponseDto>(new Error("TOKEN_REUSE_DETECTED", "Se detectó un intento de reuso de un token obsoleto. Por seguridad, todas sus sesiones han sido canceladas. Inicie sesión nuevamente."));
        }

        if (sesionExistente.EstaExpirada())
        {
            sesionExistente.Desactivar();
            _sesionRepository.Update(sesionExistente);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<LoginResponseDto>(new Error("TOKEN_EXPIRED", "El token de refresco ha expirado."));
        }

        // Rotar tokens: Inactivar el actual y emitir uno nuevo
        sesionExistente.Desactivar();
        _sesionRepository.Update(sesionExistente);

        string nuevoAccessToken = _jwtService.GenerateToken(usuario);
        string nuevoRefreshToken = _jwtService.GenerateRefreshToken();

        // Expira dinámicamente usando IJwtService
        var nuevaSesion = new Sesion(usuario.Id, nuevoRefreshToken, DateTime.UtcNow.AddDays(_jwtService.RefreshTokenExpiracionDias));
        await _sesionRepository.AddAsync(nuevaSesion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Registrar auditoría de refresco exitoso
        await _auditService.LogActionAsync(
            usuarioId: usuario.Id,
            usuarioNombre: usuario.Nombre,
            rol: usuario.Roles.FirstOrDefault()?.Nombre ?? "Ninguno",
            modulo: "Seguridad",
            entidad: "Sesiones",
            entidadId: (int)nuevaSesion.Id,
            accion: "Refresco de token",
            estadoAnterior: $"SesionId: {sesionExistente.Id}",
            estadoNuevo: $"SesionId: {nuevaSesion.Id}",
            canal: "API",
            correlationId: null);

        return Result.Success(new LoginResponseDto
        {
            Token = nuevoAccessToken,
            ExpiraEnSegundos = _jwtService.TokenExpiracionSegundos,
            MfaRequerido = false,
            RefreshToken = nuevoRefreshToken,
            Usuario = new UsuarioResponseDto
            {
                UsuarioId = usuario.Id,
                Correo = usuario.Correo,
                Nombres = usuario.Nombre,
                TipoAutenticacion = usuario.TipoAutenticacion.ToString(),
                Activo = usuario.Estado == UsuarioEstado.Activo,
                Roles = usuario.Roles.Select(r => r.Nombre).ToList(),
                Permisos = usuario.Roles.SelectMany(r => r.Permisos).Select(p => p.Codigo).Distinct().ToList()
            }
        });
    }
}
