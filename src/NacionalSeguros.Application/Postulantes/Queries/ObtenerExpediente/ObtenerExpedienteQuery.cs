using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Queries.ObtenerExpediente;

public record ObtenerExpedienteQuery(int PostulanteId) : IRequest<Result<PostulanteExpedienteDto>>;
