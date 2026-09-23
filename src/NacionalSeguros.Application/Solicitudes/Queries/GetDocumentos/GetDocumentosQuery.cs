using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.GetDocumentos;

public record GetDocumentosQuery(
    int SolicitudId,
    string? TipoDocumento
) : IRequest<Result<List<SolicitudDocumentoDto>>>;
