using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.AprobarPerfilRRHH;

public class AprobarPerfilRRHHCommandHandler : IRequestHandler<AprobarPerfilRRHHCommand, Result<PerfilResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AprobarPerfilRRHHCommandHandler(
        IPerfilCargoRepository perfilCargoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PerfilResponseDto>> Handle(AprobarPerfilRRHHCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilCargoRepository.GetByIdAsync(request.PerfilCargoId);
        if (perfil == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.NotFound", $"El perfil de cargo con ID {request.PerfilCargoId} no existe."));
        }

        var latestPerfil = await _perfilCargoRepository.GetLatestBySolicitudIdAsync(perfil.SolicitudId);
        if (latestPerfil == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.NotFound", $"No se encontró la versión más reciente del perfil para la solicitud {perfil.SolicitudId}."));
        }

        // Solo permitir aprobar si el último estado es EnRevisionRRHH o PerfilCorregidoRRHH o sus equivalentes PERF
        if (latestPerfil.Estado?.Codigo != "EnRevisionRRHH" && 
            latestPerfil.Estado?.Codigo != "PerfilCorregidoRRHH" &&
            latestPerfil.Estado?.Codigo != "PERF-REV-RRHH" &&
            latestPerfil.Estado?.Codigo != "PERF-COR-RRHH" &&
            latestPerfil.Estado?.Codigo != "PERF-RES-GEN" &&
            latestPerfil.Estado?.Codigo != "PERF-OBS-AREA" &&
            latestPerfil.Estado?.Codigo != "ObservadoAreaSol" &&
            latestPerfil.Estado?.Codigo != "ObservadoSolicitante")
        {
            if (latestPerfil.Estado?.Codigo == "PERF-REV-AREA" || latestPerfil.Estado?.Codigo == "EnRevisionAreaSolicitante")
            {
                var responseDtoIdemp = _mapper.Map<PerfilResponseDto>(latestPerfil);
                return Result.Success(responseDtoIdemp);
            }

            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.InvalidState", $"No se puede aprobar el perfil en su estado actual '{latestPerfil.Estado?.Nombre}'."));
        }

        var estadoArea = await _perfilCargoRepository.GetEstadoByCodigoAsync("PERF-REV-AREA")
            ?? await _perfilCargoRepository.GetEstadoByCodigoAsync("EnRevisionAreaSolicitante");
        if (estadoArea == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("Estado.NotFound", "El estado 'PERF-REV-AREA' o 'EnRevisionAreaSolicitante' no está parametrizado."));
        }

        // Registrar comentario para la trazabilidad automática en StateHistory
        _unitOfWork.TransitionComment = !string.IsNullOrWhiteSpace(request.Comentario) 
            ? request.Comentario 
            : "Aprobación de perfil por RRHH y envío al Área Solicitante";

        latestPerfil.AprobarRRHH(estadoArea.Id, request.UserEmail);
        _perfilCargoRepository.Update(latestPerfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Pre-cargar relaciones para mapear al DTO
        var res = await _perfilCargoRepository.GetByIdAsync(latestPerfil.Id);
        var responseDto = _mapper.Map<PerfilResponseDto>(res ?? latestPerfil);

        return Result.Success(responseDto);
    }
}
