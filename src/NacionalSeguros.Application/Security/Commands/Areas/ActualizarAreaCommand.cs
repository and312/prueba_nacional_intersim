using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Areas;

public record ActualizarAreaCommand(
    int AreaId,
    string Nombre,
    int GerenciaId,
    string? Responsable,
    string Estado) : IRequest<Result<AreaResponseDto>>;
