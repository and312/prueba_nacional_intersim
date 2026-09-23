using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;
using NacionalSeguros.Application.Perfiles.Commands.CrearPerfilCargo;

namespace NacionalSeguros.Application.Perfiles.Commands.GenerarPerfil;

public class GenerarPerfilCommandHandler : IRequestHandler<GenerarPerfilCommand, Result>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IServiceProvider _serviceProvider;

    public GenerarPerfilCommandHandler(
        ISolicitudRepository solicitudRepository,
        IPerfilCargoRepository perfilCargoRepository,
        IServiceProvider _serviceProvider)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        this._serviceProvider = _serviceProvider ?? throw new ArgumentNullException(nameof(_serviceProvider));
    }

    public async Task<Result> Handle(GenerarPerfilCommand request, CancellationToken cancellationToken)
    {
        var latestPerfil = await _perfilCargoRepository.GetLatestBySolicitudIdAsync(request.SolicitudId);
        if (latestPerfil != null && latestPerfil.Estado?.Codigo != "PERF-PEN-GEN")
        {
            return Result.Failure(new Error("Perfil.AlreadyExists", "Ya existe un perfil activo o en validación en proceso para esta solicitud."));
        }

        var solicitud = await _solicitudRepository.GetByIdAsync(request.SolicitudId);
        if (solicitud == null)
        {
            return Result.Failure(new Error("Solicitud.NotFound", $"La solicitud con ID {request.SolicitudId} no existe."));
        }

        // Restricción: No se puede generar perfil si la solicitud no está Aprobada (SOL-APR)
        if (solicitud.Estado?.Codigo != "SOL-APR")
        {
            return Result.Failure(new Error("Solicitud.NotApproved", "No se puede generar un perfil de cargo si la solicitud no se encuentra en estado Aprobada."));
        }

        // Simular que el agente de automatización (n8n/WhatsApp) consume el endpoint de callback
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(1500); // Demora para simular la inferencia de LLM en n8n
                using var scope = _serviceProvider.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var requestDto = new PerfilEstructuradoInputDto
                {
                    SolicitudId = request.SolicitudId,
                    EstadoGeneracion = "GENERADO_SIN_FUENTES",
                    FuentesUtilizadas = new List<string> { "Simulador" },
                    Alertas = new List<string>(),
                    PerfilEstructurado = new PerfilEstructuradoContentDto
                    {
                        ObjetivoPrincipalCargo = "Simulado: " + (solicitud.ObjetivoCargo ?? "Objetivo Cargo"),
                        PerfilIdealCandidato = "Simulado: Perfil Ideal",
                        PerfilTipoAltoAjuste = "Simulado: Ajuste Alto",
                        DatosGeneralesCargo = new { cargo = solicitud.Cargo },
                        PerfilRequerido = new { xp = "3 años" },
                        HerramientasSistemas = new { tools = "Office" },
                        FiltrosClaveSeleccion = new { filter = "Ninguno" },
                        ConocimientosTecnicosRequeridos = new List<object> { "Simulado" },
                        FuncionesPrincipalesCargo = new List<object> { "Simulado" },
                        CompetenciasClave = new List<object> { "Simulado" },
                        IndicadoresExitoCargo = new List<object>(),
                        MatrizPonderacion = new List<object>()
                    }
                };

                await sender.Send(new CrearPerfilCargoCommand(requestDto, "AgentePerfil"));
            }
            catch
            {
                // Evitar fugas de excepciones en hilos de background
            }
        });

        return Result.Success();
    }
}
