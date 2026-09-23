using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Application.Catalogos.Commands.ActualizarCatalogo;
using NacionalSeguros.Application.Catalogos.Commands.ActualizarParametro;
using NacionalSeguros.Application.Catalogos.Commands.CrearCatalogo;
using NacionalSeguros.Application.Catalogos.Commands.CrearParametro;
using NacionalSeguros.Application.Catalogos.Commands.EliminarCatalogo;
using NacionalSeguros.Application.Catalogos.Commands.EliminarParametro;
using NacionalSeguros.Application.Catalogos.Queries.ObtenerDetalles;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Infrastructure.Cache;
using NacionalSeguros.Persistence.Context;
using NacionalSeguros.Persistence.Repositories;
using Xunit;

namespace NacionalSeguros.Tests.Catalogos;

public class CatalogosTests
{
    private readonly Mock<ICatalogoRepository> _catalogoRepoMock;
    private readonly Mock<IParametroRepository> _parametroRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICatalogoCacheService> _cacheServiceMock;

    public CatalogosTests()
    {
        _catalogoRepoMock = new Mock<ICatalogoRepository>();
        _parametroRepoMock = new Mock<IParametroRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _auditServiceMock = new Mock<IAuditService>();
        _cacheServiceMock = new Mock<ICatalogoCacheService>();
    }

    [Fact]
    public void Catalogo_EliminarLogicamente_Should_Mark_Parameters_As_Deleted()
    {
        // Arrange
        var catalogo = new Catalogo("Modalidades", "CAT-MOD", "Admin") { Id = 1 };
        var parametro1 = new Parametro(1, "Presencial", "Presencial", null, "Admin") { Id = 1 };
        var parametro2 = new Parametro(1, "Remoto", "Remoto", null, "Admin") { Id = 2 };

        catalogo.AgregarParametro(parametro1);
        catalogo.AgregarParametro(parametro2);

        // Act
        catalogo.EliminarLogicamente();

        // Assert
        catalogo.IsDeleted.Should().BeTrue();
        parametro1.IsDeleted.Should().BeTrue();
        parametro2.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CrearParametro_Should_Return_Error_On_Circular_Dependency()
    {
        // Arrange
        var command = new CrearParametroCommand(1, "UNI-DEV", "Unidad Desarrollo", 2, "Admin");

        _catalogoRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Catalogo("Unidades", "CAT-UNI", "Admin") { Id = 1 });

        _parametroRepoMock.Setup(r => r.GetByCodigoAsync(1, "UNI-DEV"))
            .ReturnsAsync((Parametro)null!);

        _parametroRepoMock.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Parametro(1, "GER-TEC", "Gerencia Tec", null, "Admin") { Id = 2 });

        _parametroRepoMock.Setup(r => r.HasCircularDependencyAsync(0, 2))
            .ReturnsAsync(true); // Simular que es circular

        var handler = new CrearParametroCommandHandler(
            _parametroRepoMock.Object,
            _catalogoRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CIRCULAR_DEPENDENCY_DETECTED");
    }

    [Fact]
    public async Task ActualizarParametro_Should_Return_Error_On_Self_Reference()
    {
        // Arrange
        var command = new ActualizarParametroCommand(5, "Valor Nuevo", 5, "Admin"); // Apunta a sí mismo

        _parametroRepoMock.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new Parametro(1, "COD", "Val", null, "Admin") { Id = 5 });

        var handler = new ActualizarParametroCommandHandler(
            _parametroRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CIRCULAR_DEPENDENCY_DETECTED");
    }

    [Fact]
    public async Task CatalogoCacheService_Should_Retrieve_And_Cache_Parameters()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var mockParams = new List<Parametro>
        {
            new(1, "Presencial", "Presencial", null, "Admin"),
            new(1, "Hibrido", "Hibrido", null, "Admin")
        };

        _parametroRepoMock.Setup(r => r.GetByCatalogoCodigoAsync("CAT-MOD"))
            .ReturnsAsync(mockParams);

        var cacheService = new CatalogoCacheService(memoryCache, _parametroRepoMock.Object);

        // Act & Assert (Primera vez, consulta BD)
        var params1 = await cacheService.GetCachedParametersAsync("CAT-MOD");
        params1.Should().HaveCount(2);
        _parametroRepoMock.Verify(r => r.GetByCatalogoCodigoAsync("CAT-MOD"), Times.Once);

        // Segunda vez, lee de caché (no consulta BD de nuevo)
        var params2 = await cacheService.GetCachedParametersAsync("CAT-MOD");
        params2.Should().HaveCount(2);
        _parametroRepoMock.Verify(r => r.GetByCatalogoCodigoAsync("CAT-MOD"), Times.Once);

        // Invalidar caché
        await cacheService.InvalidateCacheAsync("CAT-MOD");

        // Tercera vez, vuelve a consultar BD
        var params3 = await cacheService.GetCachedParametersAsync("CAT-MOD");
        params3.Should().HaveCount(2);
        _parametroRepoMock.Verify(r => r.GetByCatalogoCodigoAsync("CAT-MOD"), Times.Exactly(2));
    }

    [Fact]
    public async Task EliminarParametro_Should_Return_Error_When_In_Use()
    {
        // Arrange
        var command = new EliminarParametroCommand(1, "Admin");

        var parametro = new Parametro(1, "Presencial", "Presencial", null, "Admin") { Id = 1 };

        _parametroRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(parametro);

        _parametroRepoMock.Setup(r => r.IsInUseAsync(1))
            .ReturnsAsync(true); // Simular que está en uso

        var handler = new EliminarParametroCommandHandler(
            _parametroRepoMock.Object,
            _unitOfWorkMock.Object,
            _cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Parametro.InUse");
        _parametroRepoMock.Verify(r => r.Update(It.IsAny<Parametro>()), Times.Never);
    }

    [Fact]
    public async Task EliminarParametro_Should_Succeed_When_Not_In_Use()
    {
        // Arrange
        var command = new EliminarParametroCommand(1, "Admin");

        var catalogo = new Catalogo("Modalidades", "CAT-MOD", "Admin") { Id = 1 };
        var parametro = new Parametro(1, "Presencial", "Presencial", null, "Admin") { Id = 1 };
        parametro.SetCatalogo(catalogo); // Evita NullReferenceException en handler al acceder a parametro.Catalogo.Codigo

        _parametroRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(parametro);

        _parametroRepoMock.Setup(r => r.IsInUseAsync(1))
            .ReturnsAsync(false); // Simular que no está en uso

        var handler = new EliminarParametroCommandHandler(
            _parametroRepoMock.Object,
            _unitOfWorkMock.Object,
            _cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        parametro.IsDeleted.Should().BeTrue();
        _parametroRepoMock.Verify(r => r.Update(parametro), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarCatalogo_Should_Return_Error_When_In_Use()
    {
        // Arrange
        var command = new EliminarCatalogoCommand(1, "Admin");

        var catalogo = new Catalogo("Modalidades", "CAT-MOD", "Admin") { Id = 1 };

        _catalogoRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(catalogo);

        _catalogoRepoMock.Setup(r => r.IsInUseAsync(1))
            .ReturnsAsync(true); // Simular que está en uso

        var handler = new EliminarCatalogoCommandHandler(
            _catalogoRepoMock.Object,
            _unitOfWorkMock.Object,
            _cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Catalogo.InUse");
        _catalogoRepoMock.Verify(r => r.Update(It.IsAny<Catalogo>()), Times.Never);
    }

    [Fact]
    public async Task EliminarCatalogo_Should_Succeed_When_Not_In_Use()
    {
        // Arrange
        var command = new EliminarCatalogoCommand(1, "Admin");

        var catalogo = new Catalogo("Modalidades", "CAT-MOD", "Admin") { Id = 1 };

        _catalogoRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(catalogo);

        _catalogoRepoMock.Setup(r => r.IsInUseAsync(1))
            .ReturnsAsync(false); // Simular que no está en uso

        var handler = new EliminarCatalogoCommandHandler(
            _catalogoRepoMock.Object,
            _unitOfWorkMock.Object,
            _cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        catalogo.IsDeleted.Should().BeTrue();
        _catalogoRepoMock.Verify(r => r.Update(catalogo), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HasCircularDependencyAsync_Should_Prevent_Infinite_Loops_When_Cycles_Exist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Sir_Test_CircularDependency_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var p1 = new Parametro(1, "P1", "Valor 1", null, "Admin") { Id = 1 };
            var p2 = new Parametro(1, "P2", "Valor 2", 1, "Admin") { Id = 2 };
            var p3 = new Parametro(1, "P3", "Valor 3", 2, "Admin") { Id = 3 };

            await context.Parametros.AddRangeAsync(p1, p2, p3);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            // Crear una relación circular en la DB manualmente: P1 -> P3 (haciendo que P1 sea hijo de P3)
            var p1 = await context.Parametros.FindAsync(1);
            p1!.Actualizar(p1.Valor, 3); // P1 -> P3 -> P2 -> P1 (Bucle)
            await context.SaveChangesAsync();

            var repository = new ParametroRepository(context);

            // Act
            bool hasCycle = await repository.HasCircularDependencyAsync(3, 1);

            // Assert
            hasCycle.Should().BeTrue();
        }
    }

    [Fact]
    public async Task IsInUseAsync_Should_Return_True_When_Parameter_Has_Active_Children()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Sir_Test_IsInUse_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var p1 = new Parametro(1, "P1", "Valor 1", null, "Admin") { Id = 1 };
            var p2 = new Parametro(1, "P2", "Valor 2", 1, "Admin") { Id = 2 };

            await context.Parametros.AddRangeAsync(p1, p2);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var repository = new ParametroRepository(context);

            // Act
            bool inUse = await repository.IsInUseAsync(1);

            // Assert
            inUse.Should().BeTrue();
        }
    }
}
