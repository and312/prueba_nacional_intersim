using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Areas;

public record CrearAreaCommand(
    string Codigo,
    string Nombre,
    int GerenciaId,
    string? Responsable) : IRequest<Result<AreaResponseDto>>;
