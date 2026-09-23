using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.RegenerarPdf;

public record RegenerarPdfCommand(
    int PerfilCargoId,
    string UserEmail
) : IRequest<Result>;
