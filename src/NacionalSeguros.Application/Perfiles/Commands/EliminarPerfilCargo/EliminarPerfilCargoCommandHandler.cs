using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.EliminarPerfilCargo;

public class EliminarPerfilCargoCommandHandler : IRequestHandler<EliminarPerfilCargoCommand, Result>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarPerfilCargoCommandHandler(IPerfilCargoRepository perfilCargoRepository, IUnitOfWork unitOfWork)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(EliminarPerfilCargoCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilCargoRepository.GetByIdAsync(request.PerfilCargoId);
        if (perfil == null)
        {
            return Result.Failure(new Error("PerfilCargo.NotFound", $"El perfil con ID {request.PerfilCargoId} no existe."));
        }

        perfil.Eliminar();
        _perfilCargoRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
