using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.AprobarPerfilCargo;

public class AprobarPerfilCargoCommandHandler : IRequestHandler<AprobarPerfilCargoCommand, Result<PerfilResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AprobarPerfilCargoCommandHandler(
        IPerfilCargoRepository perfilCargoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PerfilResponseDto>> Handle(AprobarPerfilCargoCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilCargoRepository.GetByIdAsync(request.PerfilCargoId);
        if (perfil == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.NotFound", $"El perfil de cargo con ID {request.PerfilCargoId} no existe."));
        }

        perfil.Aprobar(request.ModifiedBy);
        
        // Propagar comentario de transición para el trigger de base de datos
        _unitOfWork.TransitionComment = "Aprobacion del profesiograma de cargo por analista de RRHH.";
        
        _perfilCargoRepository.Update(perfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var res = await _perfilCargoRepository.GetByIdAsync(perfil.Id);

        var responseDto = _mapper.Map<PerfilResponseDto>(res ?? perfil);
        return Result.Success(responseDto);
    }
}
