using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.CerrarVacante;

public class CerrarVacanteCommandHandler : IRequestHandler<CerrarVacanteCommand, Result>
{
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CerrarVacanteCommandHandler(IVacanteRepository vacanteRepository, IUnitOfWork unitOfWork)
    {
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(CerrarVacanteCommand request, CancellationToken cancellationToken)
    {
        var vacante = await _vacanteRepository.GetByIdAsync(request.VacanteId);
        if (vacante == null)
        {
            return Result.Failure(new Error("Vacante.NotFound", $"La vacante con ID {request.VacanteId} no existe."));
        }

        var estadoContratada = await _vacanteRepository.GetEstadoByCodigoAsync("VAC-CON");
        if (estadoContratada == null)
        {
            return Result.Failure(new Error("Estado.NotFound", "El estado 'VAC-CON' (Contratada) no está parametrizado."));
        }

        try
        {
            vacante.Cerrar(estadoContratada, request.ModifiedBy);
            _vacanteRepository.Update(vacante);

            _unitOfWork.TransitionComment = $"Vacante cerrada. Candidato contratado: {request.PostulanteContratadoId}.";

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Vacante.InvalidTransition", ex.Message));
        }
    }
}
