using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using NacionalSeguros.Application.Perfiles.Commands.AprobarPerfilCargo;
using NacionalSeguros.Application.Perfiles.Commands.CrearPerfilCargo;
using NacionalSeguros.Application.Perfiles.Commands.ActualizarPerfilCargo;
using NacionalSeguros.Application.Perfiles.Commands.ObservarPerfilCargo;
using NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfil;
using NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfilPorId;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;
using Xunit;
using NacionalSeguros.Contracts.Requests;

namespace NacionalSeguros.Tests.Perfiles;

public class PerfilesTests
{
    private readonly Mock<IPerfilCargoRepository> _perfilRepoMock;
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public PerfilesTests()
    {
        _perfilRepoMock = new Mock<IPerfilCargoRepository>();
        _solicitudRepoMock = new Mock<ISolicitudRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
    }

    // ============================================================================
    // DOMAIN ENTITY TESTS
    // ============================================================================

    [Fact]
    public void PerfilCargo_Constructor_Should_Set_Properties_Correctly()
    {
        // Arrange & Act
        var perfil = new PerfilCargo(
            solicitudId: 10,
            cargo: "Desarrollador Java",
            descripcion: "{ \"Skills\": [\"Java 17\", \"Spring Boot\"] }",
            version: 1,
            estadoId: 1, // SOL-BOR
            createdBy: "admin@nacionalseguros.com.bo"
        );

        // Assert
        perfil.SolicitudId.Should().Be(10);
        perfil.Cargo.Should().Be("Desarrollador Java");
        perfil.Descripcion.Should().Contain("Java 17");
        perfil.Version.Should().Be(1);
        perfil.EstadoId.Should().Be(1);
        perfil.CreatedBy.Should().Be("admin@nacionalseguros.com.bo");
        perfil.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void PerfilCargo_Actualizar_Should_Modify_Properties_When_Borrador_Or_Observado()
    {
        // Arrange
        var perfil = new PerfilCargo(10, "Java Developer", "Desc anterior", 1, 1, "admin"); // EstadoId = 1 (Borrador)

        // Act
        perfil.Actualizar("Senior Java Developer", "Desc nueva", "editor@nacionalseguros.com.bo");

        // Assert
        perfil.Cargo.Should().Be("Senior Java Developer");
        perfil.Descripcion.Should().Be("Desc nueva");
        perfil.ModifiedBy.Should().Be("editor@nacionalseguros.com.bo");
        perfil.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public void PerfilCargo_Actualizar_Should_Throw_Exception_When_Not_Borrador_Or_Observado()
    {
        // Arrange
        var perfil = new PerfilCargo(10, "Java Developer", "Desc anterior", 1, 3, "admin"); // EstadoId = 3 (Aprobada)

        // Act
        Action act = () => perfil.Actualizar("New Cargo", "New Desc", "editor");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Solo se pueden modificar perfiles en estado Borrador u Observado.");
    }

    [Fact]
    public void PerfilCargo_Aprobar_Should_Set_State_To_Aprobado()
    {
        // Arrange
        var perfil = new PerfilCargo(10, "Java Developer", "Desc", 1, 2, "admin"); // EstadoId = 2 (En Validacion)

        // Act
        perfil.Aprobar("approver@nacionalseguros.com.bo");

        // Assert
        perfil.EstadoId.Should().Be(3); // SOL-APR (Aprobada)
        perfil.ModifiedBy.Should().Be("approver@nacionalseguros.com.bo");
    }

    // ============================================================================
    // COMMAND HANDLER TESTS
    // ============================================================================

    [Fact]
    public async Task CrearPerfilCargoCommandHandler_Should_Create_Perfil_And_Increment_Version()
    {
        // Arrange
        var solicitudMock = new Mock<Solicitud>(); // or instantiate directly
        var solicitud = new Solicitud(
            cargo: "Java Developer",
            solicitanteId: 1,
            regionalId: 1,
            tipoSolicitudId: 2,
            modalidadTrabajoId: 3,
            seniority: "Senior",
            prioridad: "Alta",
            funciones: "Funciones",
            estadoId: 3
        ) { Id = 10 };

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(solicitud);
        _perfilRepoMock.Setup(r => r.GetLatestBySolicitudIdAsync(10))
            .ReturnsAsync(new PerfilCargo(10, "Java Developer", "v1", 1, 1, "admin"));
        
        _perfilRepoMock.Setup(r => r.GetEstadoByCodigoAsync("PERF-REV-RRHH"))
            .ReturnsAsync(new Estado("PERF-REV-RRHH", "En Revision RRHH", "Solicitud") { Id = 2 });

        var handler = new CrearPerfilCargoCommandHandler(
            _perfilRepoMock.Object, _solicitudRepoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);

        var requestDto = new PerfilEstructuradoInputDto
        {
            SolicitudId = 10,
            EstadoGeneracion = "GENERADO",
            FuentesUtilizadas = new List<string>(),
            Alertas = new List<string>(),
            PerfilEstructurado = new PerfilEstructuradoContentDto
            {
                ObjetivoPrincipalCargo = "Test",
                PerfilIdealCandidato = "Test",
                PerfilTipoAltoAjuste = "Test",
                DatosGeneralesCargo = new { cargo = "Java Developer" },
                PerfilRequerido = new { xp = "3 años" },
                HerramientasSistemas = new { tools = "Java" },
                FiltrosClaveSeleccion = new { filter = "Ninguno" },
                ConocimientosTecnicosRequeridos = new List<object> { "Java" },
                FuncionesPrincipalesCargo = new List<object>(),
                CompetenciasClave = new List<object>(),
                IndicadoresExitoCargo = new List<object>(),
                MatrizPonderacion = new List<object>()
            }
        };

        var command = new CrearPerfilCargoCommand(requestDto, "n8n_agent");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _perfilRepoMock.Verify(r => r.Update(It.Is<PerfilCargo>(p => p.Version == 1)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AprobarPerfilCargoCommandHandler_Should_Approve_And_Save()
    {
        // Arrange
        var perfil = new PerfilCargo(10, "Java", "Desc", 1, 2, "admin") { Id = 1 };
        _perfilRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(perfil);

        var handler = new AprobarPerfilCargoCommandHandler(
            _perfilRepoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);

        var command = new AprobarPerfilCargoCommand(1, "approver@nacionalseguros.com.bo");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfil.EstadoId.Should().Be(3); // SOL-APR
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
