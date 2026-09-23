using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.ConfigurarMfa;

public class ConfigurarMfaCommandHandler : IRequestHandler<ConfigurarMfaCommand, Result<ConfigurarMfaResponse>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMfaService _mfaService;
    private readonly IAuditService _auditService;

    public ConfigurarMfaCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IMfaService mfaService,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _mfaService = mfaService;
        _auditService = auditService;
    }

    public async Task<Result<ConfigurarMfaResponse>> Handle(ConfigurarMfaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);

        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure<ConfigurarMfaResponse>(new Error("USER_NOT_FOUND", "El usuario no existe."));
        }

        // Generar clave secreta TOTP Base32
        string secreto = _mfaService.GenerateSecretKey();
        string qrUri = _mfaService.GetQrCodeUri(usuario.Correo, secreto);

        // Guardamos el secreto pero MfaHabilitado sigue en false hasta que se verifique el primer código OTP.
        usuario.ConfigurarMfa(secreto, usuario.Nombre);

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar
        await _auditService.LogActionAsync(
            usuarioId: usuario.Id,
            usuarioNombre: usuario.Nombre,
            rol: usuario.Roles.FirstOrDefault()?.Nombre ?? "Ninguno",
            modulo: "Seguridad",
            entidad: "Usuarios",
            entidadId: usuario.Id,
            accion: "Configurar MFA inicial",
            estadoAnterior: "MFA Deshabilitado",
            estadoNuevo: "MFA Secreto generado (Pendiente verificación)",
            canal: "API",
            correlationId: null);

        return Result.Success(new ConfigurarMfaResponse(secreto, qrUri));
    }
}
