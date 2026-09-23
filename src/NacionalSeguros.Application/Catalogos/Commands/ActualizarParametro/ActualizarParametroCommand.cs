using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.ActualizarParametro;

public record ActualizarParametroCommand(int Id, string Valor, int? ParametroIdPadre, string ModifiedBy) : IRequest<Result<ParametroResponse>>;
