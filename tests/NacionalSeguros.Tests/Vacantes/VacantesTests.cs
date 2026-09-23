using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using NacionalSeguros.Application.Vacantes.Commands.CancelarVacante;
using NacionalSeguros.Application.Vacantes.Commands.CerrarVacante;
using NacionalSeguros.Application.Vacantes.Commands.CrearVacante;
using NacionalSeguros.Application.Vacantes.Commands.PublicarVacante;
using NacionalSeguros.Application.Vacantes.Commands.ActualizarVacante;
using NacionalSeguros.Application.Vacantes.Commands.PausarVacante;
using NacionalSeguros.Application.Vacantes.Commands.ReanudarVacante;
using NacionalSeguros.Application.Vacantes.Queries.ObtenerVacantePorId;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;
using Xunit;

namespace NacionalSeguros.Tests.Vacantes;

public class VacantesTests
{
    private readonly Mock<IVacanteRepository> _vacanteRepoMock;
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock;
    private readonly Mock<IPerfilCargoRepository> _perfilRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public VacantesTests()
    {
        _vacanteRepoMock = new Mock<IVacanteRepository>();
        _solicitudRepoMock = new Mock<ISolicitudRepository>();
        _perfilRepoMock = new Mock<IPerfilCargoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public void Vacante_Constructor_Should_Set_Properties_Correctly()
    {
        // Arrange & Act
        var vacante = new Vacante(
            perfilCargoId: 1,
            solicitudId: 2,
            estadoId: 10,
            bandaSalarialMin: 5000,
            bandaSalarialMax: 7000,
            createdBy: "admin@nacionalseguros.com.bo"
        );

        // Assert
        vacante.PerfilCargoId.Should().Be(1);
        vacante.SolicitudId.Should().Be(2);
        vacante.EstadoId.Should().Be(10);
        vacante.BandaSalarialMin.Should().Be(5000);
        vacante.BandaSalarialMax.Should().Be(7000);
        vacante.CreatedBy.Should().Be("admin@nacionalseguros.com.bo");
        vacante.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CrearVacanteCommandHandler_Should_Create_Vacante()
    {
        // Arrange
        var solicitud = new Solicitud(
            cargo: "Analista QA",
            solicitanteId: 1,
            regionalId: 1,
            tipoSolicitudId: 2,
            modalidadTrabajoId: 3,
            seniority: "SemiSenior",
            prioridad: "Media",
            funciones: "Funciones",
            estadoId: 3
        ) { Id = 2 };

        var perfil = new PerfilCargo(2, "Analista QA", "Desc", 1, 3, "admin") { Id = 1 }; // EstadoId = 3 (Aprobada)

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(solicitud);
        _perfilRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(perfil);
        _vacanteRepoMock.Setup(r => r.GetEstadoByCodigoAsync("VAC-CRE"))
            .ReturnsAsync(new Estado("VAC-CRE", "Creada", "Vacante") { Id = 10 });

        var handler = new CrearVacanteCommandHandler(
            _vacanteRepoMock.Object,
            _solicitudRepoMock.Object,
            _perfilRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );

        var command = new CrearVacanteCommand(new VacanteCreateDto(2, 1, DateTime.UtcNow.AddDays(30)), "admin");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _vacanteRepoMock.Verify(r => r.AddAsync(It.Is<Vacante>(v => v.SolicitudId == 2 && v.PerfilCargoId == 1)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublicarVacanteCommandHandler_Should_Publish_Vacante()
    {
        // Arrange
        var vacante = new Vacante(1, 2, 10, 5000, 5000, "admin") { Id = 5 };
        var estadoPublicada = new Estado("VAC-PUB", "Publicada", "Vacante") { Id = 11 };

        _vacanteRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(vacante);
        _vacanteRepoMock.Setup(r => r.GetEstadoByCodigoAsync("VAC-PUB")).ReturnsAsync(estadoPublicada);

        var handler = new PublicarVacanteCommandHandler(_vacanteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new PublicarVacanteCommand(5, new List<int> { 1, 2 }, "admin");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vacante.EstadoId.Should().Be(11);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarVacanteCommandHandler_Should_Update_SalaryBand()
    {
        // Arrange
        var vacante = new Vacante(1, 2, 10, 5000, 5000, "admin") { Id = 5 };
        _vacanteRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(vacante);

        var handler = new ActualizarVacanteCommandHandler(_vacanteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new ActualizarVacanteCommand(5, 6000, 8000, "admin");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vacante.BandaSalarialMin.Should().Be(6000);
        vacante.BandaSalarialMax.Should().Be(8000);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PausarVacanteCommandHandler_Should_Pause_Vacante()
    {
        // Arrange
        var vacante = new Vacante(1, 2, 10, 5000, 5000, "admin") { Id = 5 };
        var estadoPausada = new Estado("VAC-CER", "Cerrada", "Vacante") { Id = 12 };

        _vacanteRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(vacante);
        _vacanteRepoMock.Setup(r => r.GetEstadoByCodigoAsync("VAC-CER")).ReturnsAsync(estadoPausada);

        var handler = new PausarVacanteCommandHandler(_vacanteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new PausarVacanteCommand(5, "Pausa temporal", "admin");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vacante.EstadoId.Should().Be(12);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
