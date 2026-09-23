using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.ActualizarVacante;

public class ActualizarVacanteCommandHandler : IRequestHandler<ActualizarVacanteCommand, Result>
{
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarVacanteCommandHandler(IVacanteRepository vacanteRepository, IUnitOfWork unitOfWork)
    {
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(ActualizarVacanteCommand request, CancellationToken cancellationToken)
    {
        var vacante = await _vacanteRepository.GetByIdAsync(request.VacanteId);
        if (vacante == null)
        {
            return Result.Failure(new Error("Vacante.NotFound", $"La vacante con ID {request.VacanteId} no existe."));
        }

        try
        {
            vacante.Actualizar(request.BandaSalarialMin, request.BandaSalarialMax, request.ModifiedBy);
            _vacanteRepository.Update(vacante);

            _unitOfWork.TransitionComment = $"Vacante actualizada. Nueva banda: {request.BandaSalarialMin} - {request.BandaSalarialMax}.";

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(new Error("Vacante.InvalidArguments", ex.Message));
        }
    }
}
