using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Commands.RegistrarPostulante;

public class RegistrarPostulanteCommandHandler : IRequestHandler<RegistrarPostulanteCommand, Result<int>>
{
    private readonly IPostulanteRepository _postulanteRepository;
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarPostulanteCommandHandler(
        IPostulanteRepository postulanteRepository,
        IVacanteRepository vacanteRepository,
        IUnitOfWork unitOfWork)
    {
        _postulanteRepository = postulanteRepository ?? throw new ArgumentNullException(nameof(postulanteRepository));
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<int>> Handle(RegistrarPostulanteCommand request, CancellationToken cancellationToken)
    {
        var vacante = await _vacanteRepository.GetByIdAsync(request.VacanteId);
        if (vacante == null)
        {
            return Result.Failure<int>(new Error("Vacante.NotFound", $"La vacante con ID {request.VacanteId} no existe."));
        }

        // Obtener o crear postulante
        var postulante = await _postulanteRepository.GetByCorreoAsync(request.Correo);
        if (postulante == null)
        {
            postulante = new Postulante(
                nombres: "Pendiente",
                apellidos: "Pendiente",
                correo: request.Correo,
                documentoIdentidad: "PENDIENTE",
                origen: "Web",
                createdBy: request.CreatedBy
            );
            await _postulanteRepository.AddPostulanteAsync(postulante);
        }

        // Verificar si ya tiene una postulación activa para esta vacante
        var postulacionExistente = await _postulanteRepository.GetPostulacionByIdsAsync(postulante.Id, vacante.Id);
        if (postulacionExistente != null)
        {
            return Result.Failure<int>(new Error("Postulacion.AlreadyExists", $"El candidato ya se encuentra postulado a la vacante {vacante.Id}."));
        }

        var estadoInicial = await _postulanteRepository.GetEstadoByCodigoAsync("POS-REG");
        if (estadoInicial == null)
        {
            return Result.Failure<int>(new Error("Estado.NotFound", "El estado inicial del pipeline 'POS-REG' no está parametrizado."));
        }

        var postulacion = new Postulacion(
            postulanteId: postulante.Id,
            vacanteId: vacante.Id,
            estadoPipelineId: estadoInicial.Id,
            pretensionSalarial: 0,
            createdBy: request.CreatedBy
        );

        // Vincular relación en caso de que postulante sea nuevo
        postulacion.GetType().GetProperty("Postulante")?.SetValue(postulacion, postulante);

        await _postulanteRepository.AddPostulacionAsync(postulacion);

        _unitOfWork.TransitionComment = $"Registro inicial de postulante y carga de CV '{request.CvFileName}' para procesamiento asincrono.";
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(postulante.Id);
    }
}
