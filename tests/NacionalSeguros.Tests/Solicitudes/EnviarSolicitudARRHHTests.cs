using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using NacionalSeguros.Application.Solicitudes.Commands.EnviarSolicitudARRHH;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;
using Xunit;

namespace NacionalSeguros.Tests.Solicitudes;

public class EnviarSolicitudARRHHTests
{
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly EnviarSolicitudARRHHCommandHandler _handler;

    public EnviarSolicitudARRHHTests()
    {
        _solicitudRepoMock = new Mock<ISolicitudRepository>();
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.SetupAllProperties();
        _mapperMock = new Mock<IMapper>();

        _handler = new EnviarSolicitudARRHHCommandHandler(
            _solicitudRepoMock.Object,
            _usuarioRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Usuario_Not_Found()
    {
        // Arrange
        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario)null!);

        var command = new EnviarSolicitudARRHHCommand(1, "nonexistent@nacionalseguros.com.bo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Usuario.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Solicitud_Not_Found()
    {
        // Arrange
        var usuario = new Usuario("Test User", "test@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 1 };
        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync("test@nacionalseguros.com.bo"))
            .ReturnsAsync(usuario);

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Solicitud)null!);

        var command = new EnviarSolicitudARRHHCommand(1, "test@nacionalseguros.com.bo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Solicitud.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_User_Is_Solicitante_But_Not_Owner()
    {
        // Arrange
        var usuario = new Usuario("Test User", "test@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 2 };
        var rol = new Rol("Solicitante", "Solicitante");
        usuario.AsignarRol(rol);

        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync("test@nacionalseguros.com.bo"))
            .ReturnsAsync(usuario);

        var solicitud = CreateValidSolicitud(); // Owner is Id = 1
        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(solicitud);

        var command = new EnviarSolicitudARRHHCommand(1, "test@nacionalseguros.com.bo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Solicitud.UnauthorizedSend");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Solicitud_Is_Not_In_Borrador_Or_Similar()
    {
        // Arrange
        var usuario = new Usuario("Test User", "test@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 1 };
        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync("test@nacionalseguros.com.bo"))
            .ReturnsAsync(usuario);

        var estadoAprobado = new Estado("SOL-APR", "Aprobada", "Solicitud") { Id = 5 };
        var solicitud = CreateValidSolicitud();
        solicitud.SetEstado(estadoAprobado);

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(solicitud);

        var command = new EnviarSolicitudARRHHCommand(1, "test@nacionalseguros.com.bo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Solicitud.InvalidStateForSend");
    }

    [Theory]
    [InlineData("Cargo", "")]
    [InlineData("Motivo", "")]
    [InlineData("Seniority", "")]
    [InlineData("Prioridad", "")]
    [InlineData("Funciones", "")]
    [InlineData("ObjetivoCargo", "")]
    [InlineData("ExperienciaMinima", "")]
    [InlineData("ConocimientosTecnicos", "")]
    public async Task Handle_Should_Return_Failure_When_Required_Field_Is_Empty(string propertyName, string emptyValue)
    {
        // Arrange
        var usuario = new Usuario("Test User", "test@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 1 };
        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync("test@nacionalseguros.com.bo"))
            .ReturnsAsync(usuario);

        var solicitud = CreateValidSolicitud();
        solicitud.GetType().GetProperty(propertyName)?.SetValue(solicitud, emptyValue);

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(solicitud);

        var command = new EnviarSolicitudARRHHCommand(1, "test@nacionalseguros.com.bo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Solicitud.Validation");
    }

    [Theory]
    [InlineData("RegionalId")]
    [InlineData("TipoSolicitudId")]
    [InlineData("ModalidadTrabajoId")]
    public async Task Handle_Should_Return_Failure_When_Required_Id_Is_Null(string propertyName)
    {
        // Arrange
        var usuario = new Usuario("Test User", "test@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 1 };
        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync("test@nacionalseguros.com.bo"))
            .ReturnsAsync(usuario);

        var solicitud = CreateValidSolicitud();
        solicitud.GetType().GetProperty(propertyName)?.SetValue(solicitud, (int?)null);

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(solicitud);

        var command = new EnviarSolicitudARRHHCommand(1, "test@nacionalseguros.com.bo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Solicitud.Validation");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Vacantes_Is_Zero_Or_Less()
    {
        // Arrange
        var usuario = new Usuario("Test User", "test@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 1 };
        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync("test@nacionalseguros.com.bo"))
            .ReturnsAsync(usuario);

        var solicitud = CreateValidSolicitud();
        solicitud.GetType().GetProperty("CantidadVacantes")?.SetValue(solicitud, (int?)0);

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(solicitud);

        var command = new EnviarSolicitudARRHHCommand(1, "test@nacionalseguros.com.bo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Solicitud.Validation");
    }

    [Fact]
    public async Task Handle_Should_Transition_To_Enviada()
    {
        // Arrange
        var usuario = new Usuario("Test User", "test@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 1 };
        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync("test@nacionalseguros.com.bo"))
            .ReturnsAsync(usuario);

        var solicitud = CreateValidSolicitud();
        var estadoEnv = new Estado("SOL-ENV", "Enviada", "Solicitud") { Id = 2 };

        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(solicitud);

        _solicitudRepoMock.Setup(r => r.GetEstadoByCodigoAsync("SOL-ENV"))
            .ReturnsAsync(estadoEnv);

        var command = new EnviarSolicitudARRHHCommand(1, "test@nacionalseguros.com.bo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        solicitud.EstadoId.Should().Be(estadoEnv.Id);
        _solicitudRepoMock.Verify(r => r.Update(solicitud), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
        _unitOfWorkMock.Object.TransitionComment.Should().Be("Solicitud enviada a revisión de RRHH.");
    }

    private Solicitud CreateValidSolicitud()
    {
        var estado = new Estado("SOL-REG", "Registrada", "Solicitud") { Id = 1 };
        var sol = new Solicitud(
            cargo: "Analista de Sistemas",
            solicitanteId: 1,
            regionalId: 1,
            tipoSolicitudId: 2,
            modalidadTrabajoId: 3,
            seniority: "Senior",
            prioridad: "Alta",
            funciones: "Desarrollo y soporte",
            estadoId: 1,
            objetivoCargo: "Objetivo",
            formacionAcademica: "Licenciatura",
            experienciaMinima: "3 años",
            experienciaIndispensable: "C#",
            conocimientosTecnicos: "SQL",
            herramientasSistemas: "VS",
            competenciasClave: "Trabajo en equipo",
            disponibilidadRequerida: "Inmediata",
            criteriosExcluyentes: "Ninguno",
            criteriosDeseables: "Deseables",
            motivo: "Reemplazo",
            cantidadVacantes: 1,
            observaciones: "Ninguna"
        );
        sol.GetType().GetProperty("Estado")?.SetValue(sol, estado);
        return sol;
    }
}
