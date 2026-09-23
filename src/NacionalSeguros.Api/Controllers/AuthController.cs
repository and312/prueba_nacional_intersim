using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Security.Commands.Login;
using NacionalSeguros.Application.Security.Commands.Logout;
using NacionalSeguros.Application.Security.Commands.RefreshToken;
using NacionalSeguros.Application.Security.Commands.CambiarPassword;
using NacionalSeguros.Application.Security.Commands.VerificarMfa;
using NacionalSeguros.Application.Security.Commands.ForgotPassword;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public AuthController(ISender sender, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _sender = sender;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand(request.Correo, request.Clave, request.TipoAutenticacion);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return Unauthorized(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Las credenciales proporcionadas no son válidas.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("log-error")]
    public IActionResult LogError([FromBody] System.Text.Json.JsonElement request)
    {
        try
        {
            var msg = request.GetProperty("message").GetString();
            var path = @"c:\Users\DELL XPS\Desktop\INTERSIM\nacional\frontend\browser-errors.log";
            System.IO.File.AppendAllText(path, $"[{DateTime.Now}] {msg}\n");
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] TokenRefreshRequestDto request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo cerrar la sesión.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Refresh([FromBody] TokenRefreshRequestDto request)
    {
        var command = new RefreshTokenCommand(request.TokenExpirado, request.RefreshToken);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Code == "TOKEN_REUSE_DETECTED")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto
                {
                    Code = result.Error.Code,
                    Message = result.Error.Message,
                    Detail = "Ataque o reuso de token detectado. Todas las sesiones activas del usuario han sido anuladas preventivamente.",
                    CorrelationId = GetCorrelationId()
                });
            }

            return Unauthorized(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo refrescar el token de acceso.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("mfa")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyMfa([FromBody] MfaVerifyRequestDto request)
    {
        var command = new VerificarMfaCommand(request.Correo, request.CodigoOtp);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return Unauthorized(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "El código de verificación del segundo factor es incorrecto.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
        {
            return Unauthorized();
        }

        var command = new CambiarPasswordCommand(usuarioId, request.ClaveActual, request.NuevaClave);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo cambiar la contraseña local del usuario.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var frontendUrl = _configuration["FrontendBaseUrl"] ?? "http://localhost:4200";
        var command = new ForgotPasswordCommand(request.Correo, frontendUrl);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo procesar la solicitud de recuperación.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("validate-reset-token")]
    public async Task<IActionResult> ValidateResetToken([FromBody] ValidateResetTokenRequestDto request)
    {
        var command = new ValidateResetTokenCommand(request.UserId, request.Token);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "El enlace de recuperación no es válido o ha expirado.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmRequestDto request)
    {
        var command = new ResetPasswordConfirmCommand(request.UserId, request.Token, request.NuevaContrasena);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo restablecer la contraseña.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    private string GetCorrelationId()
    {
        if (HttpContext.Items.TryGetValue("X-Correlation-ID", out var cid) && cid != null)
        {
            return cid.ToString()!;
        }
        return Guid.NewGuid().ToString();
    }
}
