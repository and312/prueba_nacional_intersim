using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using NacionalSeguros.Application.Solicitudes.Commands.ActualizarSolicitud;
using NacionalSeguros.Application.Solicitudes.Commands.CancelarSolicitud;
using NacionalSeguros.Application.Solicitudes.Commands.CrearSolicitud;
using NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudById;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;
using Xunit;

namespace NacionalSeguros.Tests.Solicitudes;

public class SolicitudesTests
{
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPublisher> _publisherMock;

    public SolicitudesTests()
    {
        _solicitudRepoMock = new Mock<ISolicitudRepository>();
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _publisherMock = new Mock<IPublisher>();

        _usuarioRepoMock.Setup(r => r.GetByCorreoAsync(It.IsAny<string>()))
            .ReturnsAsync(new Usuario("Admin", "admin@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 1 });
    }

    [Fact]
    public void Solicitud_Constructor_Should_Set_Properties_Correctly()
    {
        // Arrange & Act
        var solicitud = new Solicitud(
            cargo: "Desarrollador",
            solicitanteId: 1,
            regionalId: 2,
            tipoSolicitudId: 3,
            modalidadTrabajoId: 4,
            seniority: "Senior",
            prioridad: "Alta",
            funciones: "Escribir código",
            estadoId: 1,
            objetivoCargo: "Objetivo",
            formacionAcademica: "Ingeniería",
            experienciaMinima: "3 años",
            experienciaIndispensable: "C#",
            conocimientosTecnicos: "EF Core",
            herramientasSistemas: "VS",
            competenciasClave: "Trabajo en equipo",
            disponibilidadRequerida: "Inmediata",
            criteriosExcluyentes: "No excluyentes",
            criteriosDeseables: "Deseables",
            motivo: "Nueva posición",
            cantidadVacantes: 1,
            observaciones: "Ninguna",
            canalOrigen: "BackOffice",
            workflowOrigen: "Web",
            apiKeyId: null,
            correlationId: null
        );

        // Assert
        solicitud.Cargo.Should().Be("Desarrollador");
        solicitud.SolicitanteId.Should().Be(1);
        solicitud.RegionalId.Should().Be(2);
        solicitud.TipoSolicitudId.Should().Be(3);
        solicitud.ModalidadTrabajoId.Should().Be(4);
        solicitud.Seniority.Should().Be("Senior");
        solicitud.Prioridad.Should().Be("Alta");
        solicitud.Funciones.Should().Be("Escribir código");
        solicitud.ObjetivoCargo.Should().Be("Objetivo");
        solicitud.FormacionAcademica.Should().Be("Ingeniería");
        solicitud.ExperienciaMinima.Should().Be("3 años");
        solicitud.ExperienciaIndispensable.Should().Be("C#");
        solicitud.ConocimientosTecnicos.Should().Be("EF Core");
        solicitud.HerramientasSistemas.Should().Be("VS");
        solicitud.CompetenciasClave.Should().Be("Trabajo en equipo");
        solicitud.DisponibilidadRequerida.Should().Be("Inmediata");
        solicitud.CriteriosExcluyentes.Should().Be("No excluyentes");
        solicitud.CriteriosDeseables.Should().Be("Deseables");
        solicitud.Motivo.Should().Be("Nueva posición");
        solicitud.CantidadVacantes.Should().Be(1);
        solicitud.Observaciones.Should().Be("Ninguna");
        solicitud.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Solicitud_Actualizar_Should_Modify_Properties_When_In_Borrador()
    {
        // Arrange
        var estadoBorrador = new Estado("SOL-BOR", "Borrador", "Solicitud") { Id = 1 };
        var solicitud = new Solicitud(
            cargo: "Desarrollador",
            solicitanteId: 1,
            regionalId: 2,
            tipoSolicitudId: 3,
            modalidadTrabajoId: 4,
            seniority: "Senior",
            prioridad: "Alta",
            funciones: "Escribir código",
            estadoId: 1
        );
        solicitud.SetEstado(estadoBorrador);

        // Act
        solicitud.Actualizar(
            cargo: "Desarrollador Senior",
            regionalId: 5,
            tipoSolicitudId: 6,
            modalidadTrabajoId: 7,
            seniority: "Senior II",
            prioridad: "Critica",
            funciones: "Arquitectura",
            modificadoPor: "admin@nacionalseguros.com.bo",
            objetivoCargo: "Nuevo Objetivo",
            formacionAcademica: "Licenciatura",
            experienciaMinima: "5 años",
            experienciaIndispensable: "Cloud",
            conocimientosTecnicos: "Azure",
            herramientasSistemas: "Rider",
            competenciasClave: "Liderazgo",
            disponibilidadRequerida: "15 días",
            criteriosExcluyentes: "Excluyente 1",
            criteriosDeseables: "Deseable 1",
            motivo: "Reemplazo",
            cantidadVacantes: 2,
            observaciones: "Actualizado",
            canalOrigen: "Slack"
        );

        // Assert
        solicitud.Cargo.Should().Be("Desarrollador Senior");
        solicitud.RegionalId.Should().Be(5);
        solicitud.TipoSolicitudId.Should().Be(6);
        solicitud.ModalidadTrabajoId.Should().Be(7);
        solicitud.Seniority.Should().Be("Senior II");
        solicitud.Prioridad.Should().Be("Critica");
        solicitud.Funciones.Should().Be("Arquitectura");
        solicitud.ObjetivoCargo.Should().Be("Nuevo Objetivo");
        solicitud.FormacionAcademica.Should().Be("Licenciatura");
        solicitud.ExperienciaMinima.Should().Be("5 años");
        solicitud.ExperienciaIndispensable.Should().Be("Cloud");
        solicitud.ConocimientosTecnicos.Should().Be("Azure");
        solicitud.HerramientasSistemas.Should().Be("Rider");
        solicitud.CompetenciasClave.Should().Be("Liderazgo");
        solicitud.DisponibilidadRequerida.Should().Be("15 días");
        solicitud.CriteriosExcluyentes.Should().Be("Excluyente 1");
        solicitud.CriteriosDeseables.Should().Be("Deseable 1");
        solicitud.Motivo.Should().Be("Reemplazo");
        solicitud.CantidadVacantes.Should().Be(2);
        solicitud.Observaciones.Should().Be("Actualizado");
        solicitud.ModifiedBy.Should().Be("admin@nacionalseguros.com.bo");
    }

    [Fact]
    public async Task CrearSolicitudCommandHandler_Should_Create_And_Return_Solicitud()
    {
        // Arrange
        var command = new CrearSolicitudCommand(
            new SolicitudCreateDto(
                Cargo: "Analista QA",
                RegionalId: 1,
                CantidadVacantes: 1,
                TipoSolicitudId: 2,
                Motivo: "Crecimiento",
                ObjetivoCargo: "Asegurar calidad",
                FormacionAcademica: "Técnico",
                ExperienciaMinima: "2 años",
                ExperienciaIndispensable: "Selenium",
                ConocimientosTecnicos: "Pruebas",
                HerramientasSistemas: "Jira",
                CompetenciasClave: "Detallista",
                CriteriosExcluyentes: "Ninguno",
                CriteriosDeseables: "Deseables",
                Funciones: "Ejecutar casos de prueba",
                ModalidadTrabajoId: 3,
                DisponibilidadRequerida: "Inmediata",
                Seniority: "SemiSenior",
                Prioridad: "Media",
                Observaciones: "Ninguna",
                UsuarioId: 1
            ),
            "admin@nacionalseguros.com.bo",
            UsuarioId: 1
        );

        var estadoRecibida = new Estado("SOL-REC", "Recibida", "Solicitud") { Id = 1 };
        var estadoBorrador = new Estado("SOL-BOR", "Borrador", "Solicitud") { Id = 2 };

        _solicitudRepoMock.Setup(r => r.GetEstadoByCodigoAsync("SOL-REC")).ReturnsAsync(estadoRecibida);
        _solicitudRepoMock.Setup(r => r.GetEstadoByCodigoAsync("SOL-BOR")).ReturnsAsync(estadoBorrador);

        var responseDto = new SolicitudResponseDto(
            SolicitudId: 1,
            Cargo: "Analista QA",
            Area: "Sistemas",
            SolicitanteId: 1,
            DecisorId: null,
            Seniority: "SemiSenior",
            Prioridad: "Media",
            Funciones: "Ejecutar casos de prueba",
            EstadoNombre: "Borrador",
            CreatedDate: DateTime.UtcNow
        );

        _mapperMock.Setup(m => m.Map<SolicitudResponseDto>(It.IsAny<Solicitud>())).Returns(responseDto);

        var handler = new CrearSolicitudCommandHandler(
            _solicitudRepoMock.Object,
            _usuarioRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _publisherMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Cargo.Should().Be("Analista QA");
        _solicitudRepoMock.Verify(r => r.AddAsync(It.IsAny<Solicitud>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetSolicitudByIdQueryHandler_Should_Return_Response()
    {
        // Arrange
        var query = new GetSolicitudByIdQuery(1, "admin@nacionalseguros.com.bo");
        var solicitud = new Solicitud(
            cargo: "Desarrollador",
            solicitanteId: 1,
            regionalId: 2,
            tipoSolicitudId: 3,
            modalidadTrabajoId: 4,
            seniority: "Senior",
            prioridad: "Alta",
            funciones: "Escribir código",
            estadoId: 1
        );

        var adminUser = new Usuario("Admin", "admin@nacionalseguros.com.bo", TipoAutenticacion.Local) { Id = 1 };
        var adminRole = new Rol("Administrador", "Rol de Admin");
        adminUser.AsignarRol(adminRole);

        var resumenRepoMock = new Mock<ISolicitudResumenRepository>();
        var documentoRepoMock = new Mock<ISolicitudDocumentoRepository>();
        var parametroRepoMock = new Mock<IParametroRepository>();

        _usuarioRepoMock.Setup(u => u.GetByCorreoAsync("admin@nacionalseguros.com.bo")).ReturnsAsync(adminUser);
        _solicitudRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(solicitud);
        parametroRepoMock.Setup(p => p.GetByCatalogoCodigoAsync(It.IsAny<string>())).ReturnsAsync(new List<Parametro>());

        var responseDto = new SolicitudResponseDto(
            SolicitudId: 1,
            Cargo: "Desarrollador",
            Area: "Sistemas",
            SolicitanteId: 1,
            DecisorId: null,
            Seniority: "Senior",
            Prioridad: "Alta",
            Funciones: "Escribir código",
            EstadoNombre: "Borrador",
            CreatedDate: DateTime.UtcNow
        );

        _mapperMock.Setup(m => m.Map<SolicitudResponseDto>(solicitud)).Returns(responseDto);

        var handler = new GetSolicitudByIdQueryHandler(
            _solicitudRepoMock.Object,
            resumenRepoMock.Object,
            documentoRepoMock.Object,
            _usuarioRepoMock.Object,
            parametroRepoMock.Object,
            _mapperMock.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Cargo.Should().Be("Desarrollador");
    }
}
