using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.CambiarPassword;

public record CambiarPasswordCommand(
    int UsuarioId,
    string ClaveActual,
    string NuevaClave) : IRequest<Result>;
