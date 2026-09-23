using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NacionalSeguros.Api.Controllers;
using NacionalSeguros.Application.Perfiles.Commands.CrearPerfilCargo;
using NacionalSeguros.Application.PerfilesEstructurados.Commands.CrearPerfilEstructurado;
using NacionalSeguros.Application.PerfilesEstructurados.Commands.ActualizarPerfilEstructurado;
using NacionalSeguros.Application.PerfilesEstructurados.Queries;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;
using Xunit;

namespace NacionalSeguros.Tests.Perfiles;

public class PerfilEstructuradoTests
{
    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_SolicitudIdIsNegativeOrZero()
    {
        // Act
        Action act1 = () => new PerfilEstructurado(
            0, "Obj", "Ideal", "Ajuste", "GENERADO", "{}", "{}", "{}", "{}", "[]", "[]", "[]", "[]", "[]", "[]", "[]", "admin"
        );

        Action act2 = () => new PerfilEstructurado(
            -1, "Obj", "Ideal", "Ajuste", "GENERADO", "{}", "{}", "{}", "{}", "[]", "[]", "[]", "[]", "[]", "[]", "[]", "admin"
        );

        // Assert
        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_Should_Initialize_Properties_And_Set_Audit()
    {
        // Act
        var pe = new PerfilEstructurado(
            42, "Objetivo Test", "Perfil Ideal", "Ajuste Test", "GENERADO",
            "{\"general\":\"cargo\"}", "{\"requerido\":\"si\"}", "{}", "{}",
            "[\"c1\"]", "[\"f1\"]", "[\"comp1\"]", "[]", "[]", "[\"fuente1\"]", "[]",
            "creator@nacional.com"
        );

        // Assert
        pe.SolicitudId.Should().Be(42);
        pe.ObjetivoPrincipalCargo.Should().Be("Objetivo Test");
        pe.PerfilIdealCandidato.Should().Be("Perfil Ideal");
        pe.PerfilTipoAltoAjuste.Should().Be("Ajuste Test");
        pe.EstadoGeneracion.Should().Be("GENERADO");
        pe.DatosGeneralesCargo.Should().Be("{\"general\":\"cargo\"}");
        pe.PerfilRequerido.Should().Be("{\"requerido\":\"si\"}");
        pe.ConocimientosTecnicosRequeridos.Should().Be("[\"c1\"]");
        pe.FuncionesPrincipalesCargo.Should().Be("[\"f1\"]");
        pe.CompetenciasClave.Should().Be("[\"comp1\"]");
        pe.FuentesUtilizadas.Should().Be("[\"fuente1\"]");
        pe.CreatedBy.Should().Be("creator@nacional.com");
        pe.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        pe.ModifiedBy.Should().BeNull();
        pe.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public void Actualizar_Should_Update_Fields_And_Set_ModifiedAudit()
    {
        // Arrange
        var pe = new PerfilEstructurado(
            10, "Old", "Old", "Old", "Old", "{}", "{}", "{}", "{}", "[]", "[]", "[]", "[]", "[]", "[]", "[]", "creator"
        );

        // Act
        pe.Actualizar(
            "NewObj", "NewIdeal", "NewAjuste", "NEW_STATUS",
            "{\"a\":1}", "{\"b\":2}", "{\"c\":3}", "{\"d\":4}",
            "[\"newC\"]", "[\"newF\"]", "[\"newComp\"]", "[]", "[]", "[\"newFuente\"]", "[]",
            "modifier"
        );

        // Assert
        pe.ObjetivoPrincipalCargo.Should().Be("NewObj");
        pe.PerfilIdealCandidato.Should().Be("NewIdeal");
        pe.PerfilTipoAltoAjuste.Should().Be("NewAjuste");
        pe.EstadoGeneracion.Should().Be("NEW_STATUS");
        pe.DatosGeneralesCargo.Should().Be("{\"a\":1}");
        pe.PerfilRequerido.Should().Be("{\"b\":2}");
        pe.HerramientasSistemas.Should().Be("{\"c\":3}");
        pe.FiltrosClaveSeleccion.Should().Be("{\"d\":4}");
        pe.ConocimientosTecnicosRequeridos.Should().Be("[\"newC\"]");
        pe.FuncionesPrincipalesCargo.Should().Be("[\"newF\"]");
        pe.CompetenciasClave.Should().Be("[\"newComp\"]");
        pe.FuentesUtilizadas.Should().Be("[\"newFuente\"]");
        pe.ModifiedBy.Should().Be("modifier");
        pe.ModifiedDate.Should().NotBeNull();
        pe.ModifiedDate.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handler_Should_Create_New_PerfilEstructurado_When_None_Exists()
    {
        // Arrange
        var perfilRepoMock = new Mock<IPerfilCargoRepository>();
        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var mapperMock = new Mock<IMapper>();

        // Set up mock Solicitud
        var solicitud = new Solicitud(
            cargo: "TI",
            solicitanteId: 1,
            regionalId: 1,
            tipoSolicitudId: 2,
            modalidadTrabajoId: 3,
            seniority: "Senior",
            prioridad: "Alta",
            funciones: "Funciones",
            estadoId: 3
        );
        solicitudRepoMock.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(solicitud);

        // Mock getting Estado
        var estado = new Estado("PERF-REV-RRHH", "En Revision RRHH", "PerfilCargo") { Id = 2 };
        perfilRepoMock.Setup(r => r.GetEstadoByCodigoAsync("PERF-REV-RRHH"))
            .ReturnsAsync(estado);

        // Mock returning null for existing structured profile
        perfilRepoMock.Setup(r => r.GetEstructuradoBySolicitudIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PerfilEstructurado?)null);

        var requestDto = new PerfilEstructuradoInputDto
        {
            SolicitudId = 10,
            EstadoGeneracion = "GENERADO",
            FuentesUtilizadas = new List<string> { "F1" },
            Alertas = new List<string>(),
            PerfilEstructurado = new PerfilEstructuradoContentDto
            {
                ObjetivoPrincipalCargo = "Liderar TI",
                PerfilIdealCandidato = "Ingeniero",
                PerfilTipoAltoAjuste = "Excelente",
                DatosGeneralesCargo = new { cargo = "TI" },
                PerfilRequerido = new { xp = "5 años" },
                HerramientasSistemas = new { sap = "básico" },
                FiltrosClaveSeleccion = new { age = "libre" },
                ConocimientosTecnicosRequeridos = new List<object> { "C#" },
                FuncionesPrincipalesCargo = new List<object>(),
                CompetenciasClave = new List<object>(),
                IndicadoresExitoCargo = new List<object>(),
                MatrizPonderacion = new List<object>()
            }
        };

        var command = new CrearPerfilCargoCommand(requestDto, "test_creator");
        var handler = new CrearPerfilCargoCommandHandler(
            perfilRepoMock.Object,
            solicitudRepoMock.Object,
            unitOfWorkMock.Object,
            mapperMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfilRepoMock.Verify(r => r.AddEstructuradoAsync(It.Is<PerfilEstructurado>(p => 
            p.SolicitudId == 10 &&
            p.ObjetivoPrincipalCargo == "Liderar TI" &&
            p.CreatedBy == "test_creator"
        ), It.IsAny<CancellationToken>()), Times.Once);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_Should_Update_Existing_PerfilEstructurado_When_One_Exists()
    {
        // Arrange
        var perfilRepoMock = new Mock<IPerfilCargoRepository>();
        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var mapperMock = new Mock<IMapper>();

        // Set up mock Solicitud
        var solicitud = new Solicitud(
            cargo: "TI",
            solicitanteId: 1,
            regionalId: 1,
            tipoSolicitudId: 2,
            modalidadTrabajoId: 3,
            seniority: "Senior",
            prioridad: "Alta",
            funciones: "Funciones",
            estadoId: 3
        );
        solicitudRepoMock.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(solicitud);

        // Mock getting Estado
        var estado = new Estado("PERF-REV-RRHH", "En Revision RRHH", "PerfilCargo") { Id = 2 };
        perfilRepoMock.Setup(r => r.GetEstadoByCodigoAsync("PERF-REV-RRHH"))
            .ReturnsAsync(estado);

        // Mock returning an existing structured profile
        var existingPe = new PerfilEstructurado(
            10, "Old Objetivo", "Old Ideal", "Old Ajuste", "OLD_GEN",
            "{}", "{}", "{}", "{}", "[]", "[]", "[]", "[]", "[]", "[]", "[]", "old_creator"
        );
        perfilRepoMock.Setup(r => r.GetEstructuradoBySolicitudIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPe);

        var requestDto = new PerfilEstructuradoInputDto
        {
            SolicitudId = 10,
            EstadoGeneracion = "NEW_GEN",
            FuentesUtilizadas = new List<string> { "NewSource" },
            Alertas = new List<string>(),
            PerfilEstructurado = new PerfilEstructuradoContentDto
            {
                ObjetivoPrincipalCargo = "New Objetivo",
                PerfilIdealCandidato = "New Ideal",
                PerfilTipoAltoAjuste = "New Ajuste",
                DatosGeneralesCargo = new { cargo = "TI" },
                PerfilRequerido = new { xp = "5 años" },
                HerramientasSistemas = new { sap = "básico" },
                FiltrosClaveSeleccion = new { age = "libre" },
                ConocimientosTecnicosRequeridos = new List<object>(),
                FuncionesPrincipalesCargo = new List<object>(),
                CompetenciasClave = new List<object>(),
                IndicadoresExitoCargo = new List<object>(),
                MatrizPonderacion = new List<object>()
            }
        };

        var command = new CrearPerfilCargoCommand(requestDto, "test_modifier");
        var handler = new CrearPerfilCargoCommandHandler(
            perfilRepoMock.Object,
            solicitudRepoMock.Object,
            unitOfWorkMock.Object,
            mapperMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        perfilRepoMock.Verify(r => r.UpdateEstructurado(It.Is<PerfilEstructurado>(p => 
            p.SolicitudId == 10 &&
            p.ObjetivoPrincipalCargo == "New Objetivo" &&
            p.CreatedBy == "old_creator" &&
            p.ModifiedBy == "test_modifier"
        )), Times.Once);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================================
    // CQRS CRUD & HANDLER TESTS
    // ============================================================================

    [Fact]
    public async Task CrearPerfilEstructuradoCommandHandler_Should_Create_And_Return_Dto_When_Valid()
    {
        // Arrange
        var perfilRepoMock = new Mock<IPerfilCargoRepository>();
        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var solicitud = new Solicitud(
            cargo: "TI", solicitanteId: 1, regionalId: 1, tipoSolicitudId: 2, modalidadTrabajoId: 3,
            seniority: "Senior", prioridad: "Alta", funciones: "Funciones", estadoId: 3
        );
        solicitudRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(solicitud);
        perfilRepoMock.Setup(r => r.GetEstructuradoBySolicitudIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PerfilEstructurado?)null);

        var requestDto = new PerfilEstructuradoInputDto
        {
            SolicitudId = 10,
            EstadoGeneracion = "GENERADO",
            FuentesUtilizadas = new List<string>(),
            Alertas = new List<string>(),
            PerfilEstructurado = new PerfilEstructuradoContentDto
            {
                ObjetivoPrincipalCargo = "Liderar TI",
                PerfilIdealCandidato = "Ideal",
                PerfilTipoAltoAjuste = "Ajuste",
                DatosGeneralesCargo = new { cargo = "TI" },
                PerfilRequerido = new { xp = "5 años" },
                HerramientasSistemas = new { tools = "Java" },
                FiltrosClaveSeleccion = new { filter = "Ninguno" },
                ConocimientosTecnicosRequeridos = new List<object>(),
                FuncionesPrincipalesCargo = new List<object>(),
                CompetenciasClave = new List<object>(),
                IndicadoresExitoCargo = new List<object>(),
                MatrizPonderacion = new List<object>()
            }
        };

        var command = new CrearPerfilEstructuradoCommand(requestDto, "creator");
        var handler = new CrearPerfilEstructuradoCommandHandler(perfilRepoMock.Object, solicitudRepoMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ObjetivoPrincipalCargo.Should().Be("Liderar TI");
        perfilRepoMock.Verify(r => r.AddEstructuradoAsync(It.IsAny<PerfilEstructurado>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearPerfilEstructuradoCommandHandler_Should_Return_Error_When_Duplicate_SolicitudId()
    {
        // Arrange
        var perfilRepoMock = new Mock<IPerfilCargoRepository>();
        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var solicitud = new Solicitud(
            cargo: "TI", solicitanteId: 1, regionalId: 1, tipoSolicitudId: 2, modalidadTrabajoId: 3,
            seniority: "Senior", prioridad: "Alta", funciones: "Funciones", estadoId: 3
        );
        solicitudRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(solicitud);
        
        var existing = new PerfilEstructurado(10, "A", "B", "C", "GEN", "{}", "{}", "{}", "{}", "[]", "[]", "[]", "[]", "[]", "[]", "[]", "creator");
        perfilRepoMock.Setup(r => r.GetEstructuradoBySolicitudIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var requestDto = new PerfilEstructuradoInputDto { SolicitudId = 10, PerfilEstructurado = new PerfilEstructuradoContentDto() };
        var command = new CrearPerfilEstructuradoCommand(requestDto, "creator");
        var handler = new CrearPerfilEstructuradoCommandHandler(perfilRepoMock.Object, solicitudRepoMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PerfilEstructurado.Duplicate");
    }

    [Fact]
    public async Task ActualizarPerfilEstructuradoCommandHandler_Should_Update_Fields_When_Exists()
    {
        // Arrange
        var perfilRepoMock = new Mock<IPerfilCargoRepository>();
        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var existing = new PerfilEstructurado(10, "OldObj", "B", "C", "GEN", "{}", "{}", "{}", "{}", "[]", "[]", "[]", "[]", "[]", "[]", "[]", "creator");
        perfilRepoMock.Setup(r => r.GetEstructuradoByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var solicitud = new Solicitud(
            cargo: "TI", solicitanteId: 1, regionalId: 1, tipoSolicitudId: 2, modalidadTrabajoId: 3,
            seniority: "Senior", prioridad: "Alta", funciones: "Funciones", estadoId: 3
        );
        solicitudRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(solicitud);

        var requestDto = new PerfilEstructuradoInputDto
        {
            SolicitudId = 10,
            EstadoGeneracion = "NEW_GEN",
            FuentesUtilizadas = new List<string>(),
            Alertas = new List<string>(),
            PerfilEstructurado = new PerfilEstructuradoContentDto
            {
                ObjetivoPrincipalCargo = "NewObj",
                PerfilIdealCandidato = "Ideal",
                PerfilTipoAltoAjuste = "Ajuste",
                DatosGeneralesCargo = new { cargo = "TI" },
                PerfilRequerido = new { xp = "5 años" },
                HerramientasSistemas = new { tools = "Java" },
                FiltrosClaveSeleccion = new { filter = "Ninguno" },
                ConocimientosTecnicosRequeridos = new List<object>(),
                FuncionesPrincipalesCargo = new List<object>(),
                CompetenciasClave = new List<object>(),
                IndicadoresExitoCargo = new List<object>(),
                MatrizPonderacion = new List<object>()
            }
        };

        var command = new ActualizarPerfilEstructuradoCommand(1, null, requestDto, "modifier");
        var handler = new ActualizarPerfilEstructuradoCommandHandler(perfilRepoMock.Object, solicitudRepoMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ObjetivoPrincipalCargo.Should().Be("NewObj");
        existing.ObjetivoPrincipalCargo.Should().Be("NewObj");
        perfilRepoMock.Verify(r => r.UpdateEstructurado(existing), Times.Once);
    }

    // ============================================================================
    // CONTROLLER & ENDPOINT AUTHENTICATION TESTS
    // ============================================================================

    [Fact]
    public async Task PerfilesEstructuradosController_Crear_Should_Dispatch_Command_And_Return_Created()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        var dto = new PerfilEstructuradoResponseDto(1, 10, "Obj", "Ideal", "Ajuste", "GEN", new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), "creator", DateTime.UtcNow, null, null);
        senderMock.Setup(s => s.Send(It.IsAny<CrearPerfilEstructuradoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(dto));

        var controller = new PerfilesEstructuradosController(senderMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "test@test.com") }))
                }
            }
        };

        var requestDto = new PerfilEstructuradoInputDto { SolicitudId = 10 };

        // Act
        var result = await controller.Crear(requestDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task InternalApiController_ActualizarPerfilEstructurado_Should_Return_Forbidden_When_No_Permission()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        var options = new DbContextOptionsBuilder<NacionalSeguros.Persistence.Context.ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Sir_Test_" + Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new NacionalSeguros.Persistence.Context.ApplicationDbContext(options);

        // User without "perfiles.write" permission
        var claims = new List<Claim> { new Claim("permission", "perfiles.read") };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var controller = new InternalApiController(senderMock.Object, dbContext)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            }
        };

        var requestDto = new PerfilEstructuradoInputDto { SolicitudId = 10 };

        // Act
        var result = await controller.ActualizarPerfilEstructurado(10, requestDto);

        // Assert
        var forbiddenResult = result.Should().BeOfType<ObjectResult>().Subject;
        forbiddenResult.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InternalApiController_ActualizarPerfilEstructurado_Should_Return_Ok_When_Has_Permission()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        var options = new DbContextOptionsBuilder<NacionalSeguros.Persistence.Context.ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Sir_Test_" + Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new NacionalSeguros.Persistence.Context.ApplicationDbContext(options);

        var dto = new PerfilEstructuradoResponseDto(1, 10, "Obj", "Ideal", "Ajuste", "GEN", new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), "creator", DateTime.UtcNow, null, null);
        
        // Since database is empty, it will choose CrearPerfilEstructuradoCommand
        senderMock.Setup(s => s.Send(It.IsAny<CrearPerfilEstructuradoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(dto));

        var claims = new List<Claim> { new Claim("permission", "perfiles.write") };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var controller = new InternalApiController(senderMock.Object, dbContext)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            }
        };

        var requestDto = new PerfilEstructuradoInputDto { SolicitudId = 10 };

        // Act
        var result = await controller.ActualizarPerfilEstructurado(10, requestDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var val = okResult.Value;
        val.Should().NotBeNull();
        
        var messageProp = val.GetType().GetProperty("message")?.GetValue(val) as string;
        var dataProp = val.GetType().GetProperty("data")?.GetValue(val);
        
        messageProp.Should().Be("Creado con éxito");
        dataProp.Should().Be(dto);
    }
}
