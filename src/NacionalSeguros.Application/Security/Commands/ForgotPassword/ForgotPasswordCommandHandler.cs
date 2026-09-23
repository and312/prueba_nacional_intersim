using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Notifications;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Correo))
        {
            return Result.Failure(new Error("EMAIL_REQUIRED", "El correo electrónico es obligatorio."));
        }

        var usuario = await _usuarioRepository.GetByCorreoAsync(request.Correo.Trim());
        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure(new Error("USER_NOT_FOUND", "El correo electrónico ingresado no pertenece a ningún usuario registrado."));
        }

        // Generar token seguro
        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var expiration = DateTime.UtcNow.AddMinutes(15);

        // Guardar token en el usuario
        usuario.SetResetPasswordToken(token, expiration);
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generar enlace
        string frontendUrl = request.FrontendBaseUrl ?? "http://localhost:4200";
        var resetLink = $"{frontendUrl}/auth/reset-password?userId={usuario.Id}&token={token}";

        // Crear cuerpo del correo HTML
        string nombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}".Trim();
        if (string.IsNullOrEmpty(nombreUsuario)) nombreUsuario = usuario.Nombre;

        var body = $@"
        <div style=""font-family: 'Segoe UI', Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #ffffff;"">
            <div style=""text-align: center; margin-bottom: 24px; padding-bottom: 20px; border-bottom: 2px solid #003b63;"">
                <span style=""font-size: 24px; font-weight: bold; color: #003b63;"">🌳 Nacional Seguros - SIR</span>
            </div>
            <div style=""color: #333333; line-height: 1.6;"">
                <p>Hola, <strong>{nombreUsuario}</strong>,</p>
                <p>Hemos recibido una solicitud para restablecer la contraseña de acceso a tu cuenta en el Sistema Inteligente de Reclutamiento (SIR).</p>
                <p>Para continuar con el proceso, haz clic en el siguiente botón:</p>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{resetLink}"" style=""background-color: #003b63; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">Restablecer Contraseña</a>
                </div>

                <p style=""font-size: 13px; color: #666666;"">O copia y pega la siguiente dirección en tu navegador:</p>
                <p style=""font-size: 12px; font-family: monospace; background-color: #f5f5f5; padding: 10px; border-radius: 4px; word-break: break-all; color: #003b63;"">{resetLink}</p>
                
                <p style=""margin-top: 20px;"">⏳ <strong>Nota de Seguridad:</strong> Este enlace expirará en 15 minutos (a las {expiration.ToLocalTime().ToString("HH:mm:ss")} hora local).</p>
                <hr style=""border: 0; border-top: 1px solid #eeeeee; margin: 25px 0;"" />
                <p style=""font-size: 12px; color: #999999;"">⚠️ <strong>Advertencia de Seguridad:</strong> Si tú no solicitaste este cambio, por favor ignora este correo. Tu contraseña actual seguirá siendo segura y no se realizarán cambios en tu cuenta.</p>
            </div>
        </div>";

        await _emailService.SendEmailAsync(usuario.Correo, "Restablecer tu contraseña - Nacional Seguros SIR", body);

        return Result.Success();
    }
}
