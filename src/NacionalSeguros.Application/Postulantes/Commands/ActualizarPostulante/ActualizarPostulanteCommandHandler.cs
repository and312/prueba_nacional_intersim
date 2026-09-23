using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Commands.ActualizarPostulante;

public class ActualizarPostulanteCommandHandler : IRequestHandler<ActualizarPostulanteCommand, Result>
{
    private readonly IPostulanteRepository _postulanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarPostulanteCommandHandler(IPostulanteRepository postulanteRepository, IUnitOfWork unitOfWork)
    {
        _postulanteRepository = postulanteRepository ?? throw new ArgumentNullException(nameof(postulanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(ActualizarPostulanteCommand request, CancellationToken cancellationToken)
    {
        var postulante = await _postulanteRepository.GetByIdAsync(request.PostulanteId);
        if (postulante == null)
        {
            return Result.Failure(new Error("Postulante.NotFound", $"El postulante con ID {request.PostulanteId} no existe."));
        }

        postulante.ActualizarDatos(request.Nombres, request.Apellidos, request.DocumentoIdentidad, request.ModifiedBy);
        _postulanteRepository.UpdatePostulante(postulante);

        _unitOfWork.TransitionComment = $"Datos del postulante actualizados tras el parseo del CV o edición manual.";

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
