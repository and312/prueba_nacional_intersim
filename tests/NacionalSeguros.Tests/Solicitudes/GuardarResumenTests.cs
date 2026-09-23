using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Application.Solicitudes.Commands.GuardarResumen;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;
using Xunit;

namespace NacionalSeguros.Tests.Solicitudes;

public class GuardarResumenTests
{
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock;
    private readonly Mock<ISolicitudResumenRepository> _solicitudResumenRepoMock;
    private readonly Mock<IParametroRepository> _parametroRepoMock;
    private readonly Mock<IMatchingRepository> _matchingRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly GuardarResumenCommandHandler _handler;

    public GuardarResumenTests()
    {
        _solicitudRepoMock = new Mock<ISolicitudRepository>();
        _solicitudResumenRepoMock = new Mock<ISolicitudResumenRepository>();
        _parametroRepoMock = new Mock<IParametroRepository>();
        _matchingRepoMock = new Mock<IMatchingRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _auditServiceMock = new Mock<IAuditService>();

        _handler = new GuardarResumenCommandHandler(
            _solicitudRepoMock.Object,
            _solicitudResumenRepoMock.Object,
            _parametroRepoMock.Object,
            _matchingRepoMock.Object,
            _unitOfWorkMock.Object,
            _auditServiceMock.Object
        );

        // Setup parameters catalog fallback
        _parametroRepoMock.Setup(r => r.GetByCatalogoCodigoAsync("CAT-REQ-FIELDS"))
            .ReturnsAsync(new List<Parametro>());

        // Setup default mocks for states
        _solicitudRepoMock.Setup(r => r.GetEstadoByCodigoAsync("SOL-OBS"))
            .ReturnsAsync(new Estado("SOL-OBS", "Observada", "Solicitud") { Id = 3 });
        _solicitudRepoMock.Setup(r => r.GetEstadoByCodigoAsync("SOL-ENV"))
            .ReturnsAsync(new Estado("SOL-ENV", "Enviada", "Solicitud") { Id = 2 });
        _solicitudRepoMock.Setup(r => r.GetEstadoByCodigoAsync("SOL-PEN"))
            .ReturnsAsync(new Estado("SOL-PEN", "Pendiente", "Solicitud") { Id = 4 });
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Solicitud_Does_Not_Exist()
    {
        // Arrange
        var requestDto = new SolicitudResumenRequestDto(
            ProfileSummary: "Resumen",
            CaptureState: "COMPLETADO",
            CaptureConfidence: 100,
            Recommendation: "APROBAR_REVISION",
            MissingFields: new List<string>(),
            Inconsistencias: new List<InconsistenciaDto>(),
            AgentName: "agente",
            AgentVersion: "1.0",
            CorrelationId: Guid.NewGuid(),
            CamposEsperados: 13,
            CamposDetectados: 13,
            CamposFaltantes: 0,
            CompletitudPorcentaje: 100
        );
        var command = new GuardarResumenCommand(1, requestDto);
        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Solicitud?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Solicitud.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Create_Resumen_When_Does_Not_Exist()
    {
        // Arrange
        var solicitud = CreateSolicitud();
        var requestDto = new SolicitudResumenRequestDto(
            ProfileSummary: "Resumen",
            CaptureState: "COMPLETADO",
            CaptureConfidence: 100,
            Recommendation: "APROBAR_REVISION",
            MissingFields: new List<string>(),
            Inconsistencias: new List<InconsistenciaDto>(),
            AgentName: "agente",
            AgentVersion: "1.0",
            CorrelationId: Guid.NewGuid(),
            CamposEsperados: 13,
            CamposDetectados: 13,
            CamposFaltantes: 0,
            CompletitudPorcentaje: 100
        );
        var command = new GuardarResumenCommand(1, requestDto);

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(solicitud);
        _solicitudResumenRepoMock.Setup(r => r.GetBySolicitudIdAsync(1)).ReturnsAsync((SolicitudResumen?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(result.Error.Code + ": " + result.Error.Message);
        _solicitudResumenRepoMock.Verify(r => r.AddAsync(It.IsAny<SolicitudResumen>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Update_Resumen_When_Exists()
    {
        // Arrange
        var solicitud = CreateSolicitud();
        var resumen = new SolicitudResumen(
            solicitudId: 1,
            profileSummary: "Viejo",
            completitudPorcentaje: 50,
            camposDetectados: 5,
            camposEsperados: 10,
            captureState: "INCOMPLETO",
            captureConfidence: 50,
            recommendation: "Rec",
            camposFaltantesJson: "[]",
            inconsistenciasJson: "[]",
            agentName: "agente",
            agentVersion: "1.0",
            correlationId: Guid.NewGuid()
        );
        var requestDto = new SolicitudResumenRequestDto(
            ProfileSummary: "Nuevo",
            CaptureState: "COMPLETADO",
            CaptureConfidence: 100,
            Recommendation: "APROBAR_REVISION",
            MissingFields: new List<string>(),
            Inconsistencias: new List<InconsistenciaDto>(),
            AgentName: "agente",
            AgentVersion: "1.0",
            CorrelationId: Guid.NewGuid(),
            CamposEsperados: 10,
            CamposDetectados: 10,
            CamposFaltantes: 0,
            CompletitudPorcentaje: 100
        );
        var command = new GuardarResumenCommand(1, requestDto);

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(solicitud);
        _solicitudResumenRepoMock.Setup(r => r.GetBySolicitudIdAsync(1)).ReturnsAsync(resumen);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(result.Error.Code + ": " + result.Error.Message);
        resumen.ProfileSummary.Should().Be("Nuevo");
        resumen.CompletitudPorcentaje.Should().Be(100);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private Solicitud CreateSolicitud()
    {
        var estado = new Estado("SOL-REG", "Registrada", "Solicitud") { Id = 1 };
        var sol = new Solicitud(
            cargo: "Desarrollador C#",
            solicitanteId: 1,
            regionalId: 1,
            tipoSolicitudId: 2,
            modalidadTrabajoId: 3,
            seniority: "Junior",
            prioridad: "Media",
            funciones: "Escribir código",
            estadoId: 1
        )
        {
            Id = 1
        };
        sol.GetType().GetProperty("Estado")?.SetValue(sol, estado);
        return sol;
    }
}
