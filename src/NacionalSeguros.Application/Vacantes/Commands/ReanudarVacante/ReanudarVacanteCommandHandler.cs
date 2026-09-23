using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.ReanudarVacante;

public class ReanudarVacanteCommandHandler : IRequestHandler<ReanudarVacanteCommand, Result>
{
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReanudarVacanteCommandHandler(IVacanteRepository vacanteRepository, IUnitOfWork unitOfWork)
    {
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(ReanudarVacanteCommand request, CancellationToken cancellationToken)
    {
        var vacante = await _vacanteRepository.GetByIdAsync(request.VacanteId);
        if (vacante == null)
        {
            return Result.Failure(new Error("Vacante.NotFound", $"La vacante con ID {request.VacanteId} no existe."));
        }

        var estadoReanudada = await _vacanteRepository.GetEstadoByCodigoAsync("VAC-PUB"); // Físicamente reanuda a Publicada
        if (estadoReanudada == null)
        {
            return Result.Failure(new Error("Estado.NotFound", "El estado 'VAC-PUB' no está parametrizado."));
        }

        try
        {
            vacante.Reanudar(estadoReanudada, request.ModifiedBy);
            _vacanteRepository.Update(vacante);

            _unitOfWork.TransitionComment = $"Vacante reanudada. Justificacion: {request.Justificacion}.";

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Vacante.InvalidTransition", ex.Message));
        }
    }
}
