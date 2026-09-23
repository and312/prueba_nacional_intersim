using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NacionalSeguros.Application.Perfiles.Commands;
using NacionalSeguros.Application.Perfiles.Queries;
using NacionalSeguros.Application.Solicitudes.Commands.RegistrarDocumento;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using Xunit;

namespace NacionalSeguros.Tests.Perfiles;

public class PerfilesSprintTests
{
    private readonly Mock<IPerfilCargoRepository> _perfilRepoMock;
    private readonly Mock<ITipoObservacionRepository> _tipoObsRepoMock;
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock;
    private readonly Mock<ISolicitudDocumentoRepository> _documentoRepoMock;
    private readonly Mock<IEstadoRepository> _estadoRepoMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public PerfilesSprintTests()
    {
        _perfilRepoMock = new Mock<IPerfilCargoRepository>();
        _tipoObsRepoMock = new Mock<ITipoObservacionRepository>();
        _solicitudRepoMock = new Mock<ISolicitudRepository>();
        _documentoRepoMock = new Mock<ISolicitudDocumentoRepository>();
        _estadoRepoMock = new Mock<IEstadoRepository>();
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task ActualizarResumenCommand_Should_Insert_Or_Update_ResumenEjecutivo_And_Transition_State()
    {
        // Arrange
        var perfil = new PerfilCargo(1, "Cargo test", "Desc", 1, 1, "test") { Id = 10 };
        var estadoMock = new Estado("PERF-PEN-GEN", "Pendiente", "Perfil") { Id = 1 };
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil, estadoMock);

        _perfilRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(perfil);
        _perfilRepoMock.Setup(r => r.GetResumenByPerfilCargoIdAsync(10)).ReturnsAsync((ResumenEjecutivo?)null);
        
        var nextEstado = new Estado("PERF-REV-RRHH", "Revision", "Perfil") { Id = 2 };
        _estadoRepoMock.Setup(r => r.GetByCodigoAsync("PERF-REV-RRHH")).ReturnsAsync(nextEstado);

        var handler = new PerfilCommandsHandler(
            _perfilRepoMock.Object, _tipoObsRepoMock.Object, _estadoRepoMock.Object, _usuarioRepoMock.Object, _unitOfWorkMock.Object);

        var rolDto = new ResumenEjecutivoRolDto(
            "Resumen test", "Objetivo", new List<string> { "Func1" }, new List<string> { "Req1" },
            "Formacion", new List<string> { "Hard1" }, new List<string> { "Soft1" },
            "Modalidad", "Ubicacion", "Banda", "Criterios", "Claves", "Valoracion"
        );

        var command = new ActualizarResumenCommand(10, 1, rolDto, "n8n_IA", EsAutomatizacion: true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfil.EstadoId.Should().Be(2); // State should transition to PERF-REV-RRHH
        _perfilRepoMock.Verify(r => r.AddResumenAsync(It.Is<ResumenEjecutivo>(re => re.PerfilCargoId == 10 && re.Resumen == "Resumen test")), Times.Once);
        _perfilRepoMock.Verify(r => r.AddStateHistoryAsync(It.IsAny<StateHistory>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarObservacionPerfilCommand_Should_Register_When_Valid()
    {
        // Arrange
        var perfil = new PerfilCargo(1, "Cargo test", "Desc", 1, 4, "test") { Id = 10 };
        var estadoMock = new Estado("PERF-REV-AREA", "Revision Area", "Perfil") { Id = 4 };
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil, estadoMock);

        var solicitud = new Solicitud("Cargo test", 5, 1, 1, 1, "Senior", "Alta", "Func", 3) { Id = 1 };
        typeof(PerfilCargo).GetProperty("Solicitud")?.SetValue(perfil, solicitud);

        var tipoObs = new TipoObservacion("FORMATO", "Errores de Formato") { Id = 100 };

        _perfilRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(perfil);
        _perfilRepoMock.Setup(r => r.GetMaxObservacionIteracionAsync(10)).ReturnsAsync(1);
        _tipoObsRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(tipoObs);

        var handler = new PerfilCommandsHandler(
            _perfilRepoMock.Object, _tipoObsRepoMock.Object, _estadoRepoMock.Object, _usuarioRepoMock.Object, _unitOfWorkMock.Object);

        var command = new RegistrarObservacionPerfilCommand(10, 100, "Corrección de logo", 5, "Solicitante");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfil.EstadoId.Should().Be(4); // Remains in PERF-REV-AREA until sent
        _perfilRepoMock.Verify(r => r.AddObservacionAsync(It.Is<PerfilObservacion>(o => o.TipoObservacionId == 100 && o.NumeroIteracion == 2)), Times.Once);
    }

    [Fact]
    public async Task EnviarObservacionesSolicitanteCommand_Should_Transition_To_PERF_OBS_AREA()
    {
        // Arrange
        var perfil = new PerfilCargo(1, "Cargo test", "Desc", 1, 4, "test") { Id = 10 };
        var estadoMock = new Estado("PERF-REV-AREA", "Revision Area", "Perfil") { Id = 4 };
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil, estadoMock);

        var solicitud = new Solicitud("Cargo test", 5, 1, 1, 1, "Senior", "Alta", "Func", 3) { Id = 1 };
        typeof(PerfilCargo).GetProperty("Solicitud")?.SetValue(perfil, solicitud);

        _perfilRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(perfil);

        var nextEstado = new Estado("PERF-OBS-AREA", "Observado Area", "Perfil") { Id = 5 };
        _estadoRepoMock.Setup(r => r.GetByCodigoAsync("PERF-OBS-AREA")).ReturnsAsync(nextEstado);

        var handler = new PerfilCommandsHandler(
            _perfilRepoMock.Object, _tipoObsRepoMock.Object, _estadoRepoMock.Object, _usuarioRepoMock.Object, _unitOfWorkMock.Object);

        var command = new EnviarObservacionesSolicitanteCommand(10, 5, "Solicitante");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfil.EstadoId.Should().Be(5);
        _perfilRepoMock.Verify(r => r.AddStateHistoryAsync(It.Is<StateHistory>(s => s.EstadoNuevoId == 5)), Times.Once);
    }

    [Fact]
    public async Task RegistrarObservacionPerfilCommand_Should_Prevent_Observation_For_Other_Solicitante()
    {
        // Arrange
        var perfil = new PerfilCargo(1, "Cargo test", "Desc", 1, 4, "test") { Id = 10 };
        var estadoMock = new Estado("PERF-REV-AREA", "Revision Area", "Perfil") { Id = 4 };
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil, estadoMock);

        var solicitud = new Solicitud("Cargo test", 5, 1, 1, 1, "Senior", "Alta", "Func", 3) { Id = 1 };
        typeof(PerfilCargo).GetProperty("Solicitud")?.SetValue(perfil, solicitud);

        _perfilRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(perfil);

        var handler = new PerfilCommandsHandler(
            _perfilRepoMock.Object, _tipoObsRepoMock.Object, _estadoRepoMock.Object, _usuarioRepoMock.Object, _unitOfWorkMock.Object);

        var command = new RegistrarObservacionPerfilCommand(10, 100, "Observacion ajena", 99, "Otro Solicitante");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Perfil.Forbidden");
    }

    [Fact]
    public async Task RegistrarObservacionPerfilCommand_Should_Prevent_Invalid_State()
    {
        // Arrange
        var perfil = new PerfilCargo(1, "Cargo test", "Desc", 1, 1, "test") { Id = 10 };
        var estadoMock = new Estado("PERF-PEN-GEN", "Pendiente", "Perfil") { Id = 1 };
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil, estadoMock);

        var solicitud = new Solicitud("Cargo test", 5, 1, 1, 1, "Senior", "Alta", "Func", 3) { Id = 1 };
        typeof(PerfilCargo).GetProperty("Solicitud")?.SetValue(perfil, solicitud);

        _perfilRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(perfil);

        var handler = new PerfilCommandsHandler(
            _perfilRepoMock.Object, _tipoObsRepoMock.Object, _estadoRepoMock.Object, _usuarioRepoMock.Object, _unitOfWorkMock.Object);

        var command = new RegistrarObservacionPerfilCommand(10, 100, "Observacion errada", 5, "Solicitante");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Perfil.InvalidState");
    }

    [Fact]
    public async Task AprobarSolicitantePerfilCommand_Should_Transition_To_AprobadoArea()
    {
        // Arrange
        var perfil = new PerfilCargo(1, "Cargo test", "Desc", 1, 4, "test") { Id = 10 };
        var estadoMock = new Estado("PERF-REV-AREA", "Revision Area", "Perfil") { Id = 4 };
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil, estadoMock);

        var solicitud = new Solicitud("Cargo test", 5, 1, 1, 1, "Senior", "Alta", "Func", 3) { Id = 1 };
        typeof(PerfilCargo).GetProperty("Solicitud")?.SetValue(perfil, solicitud);

        _perfilRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(perfil);
        var nextEstado = new Estado("PERF-APR-AREA", "Aprobado Area", "Perfil") { Id = 7 };
        _estadoRepoMock.Setup(r => r.GetByCodigoAsync("PERF-APR-AREA")).ReturnsAsync(nextEstado);

        var handler = new PerfilCommandsHandler(
            _perfilRepoMock.Object, _tipoObsRepoMock.Object, _estadoRepoMock.Object, _usuarioRepoMock.Object, _unitOfWorkMock.Object);

        var command = new AprobarSolicitantePerfilCommand(10, 5, "Solicitante");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfil.EstadoId.Should().Be(7);
        _perfilRepoMock.Verify(r => r.AddStateHistoryAsync(It.Is<StateHistory>(s => s.EstadoNuevoId == 7)), Times.Once);
    }

    [Fact]
    public async Task AtenderObservacionesPerfilCommand_Should_Mark_Atendidas_And_Transition()
    {
        // Arrange
        var perfil = new PerfilCargo(1, "Cargo test", "Desc", 1, 5, "test") { Id = 10 };
        var estadoMock = new Estado("PERF-OBS-AREA", "Observado Area", "Perfil") { Id = 5 };
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil, estadoMock);

        var obs = new PerfilObservacion(10, 100, "Comentario", 5, 1);
        var obsList = new List<PerfilObservacion> { obs };

        _perfilRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(perfil);
        _perfilRepoMock.Setup(r => r.GetPendientesByPerfilIdAsync(10)).ReturnsAsync(obsList);

        var nextEstado = new Estado("PERF-COR-RRHH", "En Correccion", "Perfil") { Id = 6 };
        _estadoRepoMock.Setup(r => r.GetByCodigoAsync("PERF-COR-RRHH")).ReturnsAsync(nextEstado);

        var handler = new PerfilCommandsHandler(
            _perfilRepoMock.Object, _tipoObsRepoMock.Object, _estadoRepoMock.Object, _usuarioRepoMock.Object, _unitOfWorkMock.Object);

        var command = new AtenderObservacionesPerfilCommand(10, 9, "RRHH");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfil.EstadoId.Should().Be(6);
        obs.EstadoObservacion.Should().Be("Atendida");
        obs.AtendidaPorUsuarioId.Should().Be(9);
    }

    [Fact]
    public async Task AprobarFinalPerfilCommand_Should_Transition_To_FinalState()
    {
        // Arrange
        var perfil = new PerfilCargo(1, "Cargo test", "Desc", 1, 5, "test") { Id = 10 };
        var estadoMock = new Estado("PERF-REV-RRHH", "Revision RRHH", "Perfil") { Id = 5 };
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil, estadoMock);

        _perfilRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(perfil);
        var nextEstado = new Estado("PERF-APR-FIN", "Aprobado Final", "Perfil") { Id = 8 };
        _estadoRepoMock.Setup(r => r.GetByCodigoAsync("PERF-APR-FIN")).ReturnsAsync(nextEstado);
        _perfilRepoMock.Setup(r => r.WasApprovedByAreaAsync(10)).ReturnsAsync(true);

        var handler = new PerfilCommandsHandler(
            _perfilRepoMock.Object, _tipoObsRepoMock.Object, _estadoRepoMock.Object, _usuarioRepoMock.Object, _unitOfWorkMock.Object);

        var command = new AprobarFinalPerfilCommand(10, 9, "RRHH");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfil.EstadoId.Should().Be(8);
    }

    [Fact]
    public async Task RegistrarDocumentoCommandHandler_Should_Replace_In_Place_When_Duplicate()
    {
        // Arrange
        var solicitud = new Solicitud("Cargo test", 5, 1, 1, 1, "Senior", "Alta", "Func", 3) { Id = 100 };
        _solicitudRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(solicitud);

        var originalCreatedDate = new DateTime(2026, 1, 1);
        var existingDoc = new SolicitudDocumento(100, "RESUMEN_EJECUTIVO_PDF", "old.pdf", "Disk", "/path/old.pdf", null, null, null);
        typeof(SolicitudDocumento).GetProperty("CreatedDate")?.SetValue(existingDoc, originalCreatedDate);

        _documentoRepoMock.Setup(r => r.GetBySolicitudIdAsync(100, "RESUMEN_EJECUTIVO_PDF"))
            .ReturnsAsync(new List<SolicitudDocumento> { existingDoc });

        var handler = new RegistrarDocumentoCommandHandler(
            _solicitudRepoMock.Object, _documentoRepoMock.Object, _unitOfWorkMock.Object);

        var dto = new SolicitudDocumentoDto("RESUMEN_EJECUTIVO_PDF", "new.pdf", "Disk", "/path/new.pdf", "http://new", "Admin", Guid.NewGuid());
        var command = new RegistrarDocumentoCommand(100, dto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingDoc.FileName.Should().Be("new.pdf");
        existingDoc.StoragePath.Should().Be("/path/new.pdf");
        existingDoc.CreatedDate.Should().Be(originalCreatedDate); // Keeps the original creation date intact!
        _documentoRepoMock.Verify(r => r.AddAsync(It.IsAny<SolicitudDocumento>()), Times.Never); // Does NOT add a new record!
    }

    [Fact]
    public async Task ListarPerfilesQuery_Should_Limit_To_Solicitante_When_Provided()
    {
        // Arrange
        var perfil1 = new PerfilCargo(10, "Cargo 1", "Desc", 1, 1, "test") { Id = 1 };
        var solicitud1 = new Solicitud("Cargo 1", 5, 1, 1, 1, "Senior", "Alta", "Func", 3) { Id = 10 };
        typeof(Solicitud).GetProperty("Codigo")?.SetValue(solicitud1, "SOL-001");
        var estado1 = new Estado("PERF-PEN-GEN", "Pendiente", "Perfil") { Id = 1 };
        typeof(PerfilCargo).GetProperty("Solicitud")?.SetValue(perfil1, solicitud1);
        typeof(PerfilCargo).GetProperty("Estado")?.SetValue(perfil1, estado1);

        var perfilesList = new List<PerfilCargo> { perfil1 };
        _perfilRepoMock.Setup(r => r.ListBySolicitanteIdAsync(5)).ReturnsAsync(perfilesList);

        var handler = new PerfilQueriesHandler(_perfilRepoMock.Object, _documentoRepoMock.Object);
        var query = new ListarPerfilesQuery(5);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().SolicitanteId.Should().Be(5);
        _perfilRepoMock.Verify(r => r.ListBySolicitanteIdAsync(5), Times.Once);
        _perfilRepoMock.Verify(r => r.ListAsync(), Times.Never);
    }
}
