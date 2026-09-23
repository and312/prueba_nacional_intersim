using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.CancelarSolicitud;

public class CancelarSolicitudCommandHandler : IRequestHandler<CancelarSolicitudCommand, Result<SolicitudResponseDto>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CancelarSolicitudCommandHandler(
        ISolicitudRepository solicitudRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<SolicitudResponseDto>> Handle(CancelarSolicitudCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByCorreoAsync(request.ModifiedBy);
        if (usuario == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Usuario.NotFound", $"El usuario '{request.ModifiedBy}' no existe."));
        }

        var solicitud = await _solicitudRepository.GetByIdAsync(request.Id);
        if (solicitud == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Id} no existe."));
        }

        var estadoCancelado = await _solicitudRepository.GetEstadoByCodigoAsync("SOL-CAN");
        if (estadoCancelado == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Estado.NotFound", "El estado 'SOL-CAN' no existe en el catálogo."));
        }

        try
        {
            // Registrar el motivo de cancelación como un comentario de la solicitud
            solicitud.AgregarComentario(usuario.Id, $"Cancelada: {request.Motivo}");
            
            // Transitar estado
            solicitud.Transitar(estadoCancelado, decisorId: null, modificadoPor: usuario.Correo);

            _unitOfWork.TransitionComment = $"Cancelada: {request.Motivo}";
            _solicitudRepository.Update(solicitud);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var res = await _solicitudRepository.GetByIdAsync(solicitud.Id);

            var responseDto = _mapper.Map<SolicitudResponseDto>(res ?? solicitud);
            return Result.Success(responseDto);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("INVALID_STATE_TRANSITION", ex.Message));
        }
    }
}
