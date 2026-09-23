using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.PublicarVacante;

public class PublicarVacanteCommandHandler : IRequestHandler<PublicarVacanteCommand, Result>
{
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublicarVacanteCommandHandler(IVacanteRepository vacanteRepository, IUnitOfWork unitOfWork)
    {
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(PublicarVacanteCommand request, CancellationToken cancellationToken)
    {
        var vacante = await _vacanteRepository.GetByIdAsync(request.VacanteId);
        if (vacante == null)
        {
            return Result.Failure(new Error("Vacante.NotFound", $"La vacante con ID {request.VacanteId} no existe."));
        }

        var estadoPublicada = await _vacanteRepository.GetEstadoByCodigoAsync("VAC-PUB");
        if (estadoPublicada == null)
        {
            return Result.Failure(new Error("Estado.NotFound", "El estado 'VAC-PUB' (Publicada) no está parametrizado."));
        }

        try
        {
            vacante.Publicar(estadoPublicada, request.ModifiedBy);
            _vacanteRepository.Update(vacante);

            _unitOfWork.TransitionComment = $"Vacante publicada en los canales especificados.";

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Vacante.InvalidTransition", ex.Message));
        }
    }
}
