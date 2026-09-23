using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Commands.TransitarPostulante;

public class TransitarPostulanteCommandHandler : IRequestHandler<TransitarPostulanteCommand, Result>
{
    private readonly IPostulanteRepository _postulanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransitarPostulanteCommandHandler(IPostulanteRepository postulanteRepository, IUnitOfWork unitOfWork)
    {
        _postulanteRepository = postulanteRepository ?? throw new ArgumentNullException(nameof(postulanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(TransitarPostulanteCommand request, CancellationToken cancellationToken)
    {
        var postulacion = await _postulanteRepository.GetPostulacionByIdsAsync(request.PostulanteId, request.VacanteId);
        if (postulacion == null)
        {
            return Result.Failure(new Error("Postulacion.NotFound", $"La postulacion del candidato {request.PostulanteId} para la vacante {request.VacanteId} no existe."));
        }

        var nuevoEstado = await _postulanteRepository.GetEstadoByIdAsync(request.NuevoEstadoId);
        if (nuevoEstado == null)
        {
            return Result.Failure(new Error("Estado.NotFound", $"El estado con ID {request.NuevoEstadoId} no existe."));
        }

        try
        {
            postulacion.Transitar(nuevoEstado, request.Changer);
            _postulanteRepository.UpdatePostulacion(postulacion);

            string comentario = $"Transicion de estado del postulante en el pipeline a '{nuevoEstado.Nombre}'.";
            if (!string.IsNullOrWhiteSpace(request.JustificacionText))
            {
                comentario += $" Justificacion: {request.JustificacionText}.";
            }
            if (!string.IsNullOrWhiteSpace(request.MotivoDescarteCodigo))
            {
                comentario += $" Motivo Descarte: {request.MotivoDescarteCodigo}.";
            }

            _unitOfWork.TransitionComment = comentario;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Postulante.InvalidTransition", ex.Message));
        }
    }
}
