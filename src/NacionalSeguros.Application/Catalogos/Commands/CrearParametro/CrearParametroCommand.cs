using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Commands.CrearParametro;

public record CrearParametroCommand(int CatalogoId, string Codigo, string Valor, int? ParametroIdPadre, string CreatedBy) : IRequest<Result<ParametroResponse>>;
