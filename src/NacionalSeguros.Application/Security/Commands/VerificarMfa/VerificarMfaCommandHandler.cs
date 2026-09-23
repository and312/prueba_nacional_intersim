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

namespace NacionalSeguros.Application.Security.Commands.VerificarMfa;

public class VerificarMfaCommandHandler : IRequestHandler<VerificarMfaCommand, Result<LoginResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISesionRepository _sesionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMfaService _mfaService;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;

    public VerificarMfaCommandHandler(
        IUsuarioRepository usuarioRepository,
        ISesionRepository sesionRepository,
        IUnitOfWork unitOfWork,
        IMfaService mfaService,
        IJwtService jwtService,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _sesionRepository = sesionRepository;
        _unitOfWork = unitOfWork;
        _mfaService = mfaService;
        _jwtService = jwtService;
        _auditService = auditService;
    }

    public async Task<Result<LoginResponseDto>> Handle(VerificarMfaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByCorreoAsync(request.Correo);

        if (usuario == null || usuario.IsDeleted || usuario.Estado == UsuarioEstado.Inactivo)
        {
            return Result.Failure<LoginResponseDto>(new Error("USER_NOT_FOUND", "Usuario no encontrado o inactivo."));
        }

        if (string.IsNullOrEmpty(usuario.MfaSecreto))
        {
            return Result.Failure<LoginResponseDto>(new Error("MFA_NOT_CONFIGURED", "MFA no está configurado para este usuario."));
        }

        bool esValido = _mfaService.ValidateCode(usuario.MfaSecreto, request.CodigoOtp);

        if (!esValido)
        {
            // Registrar intento de MFA fallido
            await _auditService.LogActionAsync(
                usuarioId: usuario.Id,
                usuarioNombre: usuario.Nombre,
                rol: usuario.Roles.FirstOrDefault()?.Nombre ?? "Ninguno",
                modulo: "Seguridad",
                entidad: "Usuarios",
                entidadId: usuario.Id,
                accion: "Validación MFA fallida",
                estadoAnterior: null,
                estadoNuevo: null,
                canal: "API",
                correlationId: null);

            return Result.Failure<LoginResponseDto>(new Error("INVALID_MFA_CODE", "El código OTP ingresado es inválido o ha expirado."));
        }

        // Si MFA estaba pendiente de confirmación (MfaHabilitado == false), activarlo formalmente
        if (!usuario.MfaHabilitado)
        {
            usuario.ConfirmarMfa(usuario.Nombre);
            _usuarioRepository.Update(usuario);
        }

        // Emitir tokens JWT y crear sesión activa
        string accessToken = _jwtService.GenerateToken(usuario);
        string refreshToken = _jwtService.GenerateRefreshToken();

        var sesion = new Sesion(usuario.Id, refreshToken, DateTime.UtcNow.AddDays(7));
        await _sesionRepository.AddAsync(sesion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar login/MFA exitoso
        await _auditService.LogActionAsync(
            usuarioId: usuario.Id,
            usuarioNombre: usuario.Nombre,
            rol: usuario.Roles.FirstOrDefault()?.Nombre ?? "Ninguno",
            modulo: "Seguridad",
            entidad: "Sesiones",
            entidadId: (int)sesion.Id,
            accion: "MFA verificado con éxito",
            estadoAnterior: null,
            estadoNuevo: $"SesionId: {sesion.Id}",
            canal: "API",
            correlationId: null);

        return Result.Success(new LoginResponseDto
        {
            Token = accessToken,
            ExpiraEnSegundos = 3600, // 1 hora
            MfaRequerido = false,
            RefreshToken = refreshToken,
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
