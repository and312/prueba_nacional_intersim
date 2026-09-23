using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.PausarVacante;

public class PausarVacanteCommandHandler : IRequestHandler<PausarVacanteCommand, Result>
{
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PausarVacanteCommandHandler(IVacanteRepository vacanteRepository, IUnitOfWork unitOfWork)
    {
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(PausarVacanteCommand request, CancellationToken cancellationToken)
    {
        var vacante = await _vacanteRepository.GetByIdAsync(request.VacanteId);
        if (vacante == null)
        {
            return Result.Failure(new Error("Vacante.NotFound", $"La vacante con ID {request.VacanteId} no existe."));
        }

        var estadoPausada = await _vacanteRepository.GetEstadoByCodigoAsync("VAC-CER"); // Físicamente mapeada a Cerrada
        if (estadoPausada == null)
        {
            return Result.Failure(new Error("Estado.NotFound", "El estado 'VAC-CER' no está parametrizado."));
        }

        try
        {
            vacante.Pausar(estadoPausada, request.ModifiedBy);
            _vacanteRepository.Update(vacante);

            _unitOfWork.TransitionComment = $"Vacante pausada. Justificacion: {request.Justificacion}.";

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Vacante.InvalidTransition", ex.Message));
        }
    }
}
