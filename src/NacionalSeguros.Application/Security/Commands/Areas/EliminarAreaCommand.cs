using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Areas;

public record EliminarAreaCommand(int AreaId) : IRequest<Result>;
