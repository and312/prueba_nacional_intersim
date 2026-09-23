using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NacionalSeguros.Application.Postulantes.Commands.RegistrarPostulante;
using NacionalSeguros.Application.Postulantes.Commands.TransitarPostulante;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using Xunit;

namespace NacionalSeguros.Tests.Postulantes;

public class PostulantesTests
{
    private readonly Mock<IPostulanteRepository> _postulanteRepoMock;
    private readonly Mock<IVacanteRepository> _vacanteRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public PostulantesTests()
    {
        _postulanteRepoMock = new Mock<IPostulanteRepository>();
        _vacanteRepoMock = new Mock<IVacanteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public void Postulante_Constructor_Should_Set_Properties_Correctly()
    {
        // Act
        var postulante = new Postulante(
            nombres: "Juan",
            apellidos: "Pérez",
            correo: "juan.perez@example.com",
            documentoIdentidad: "1234567-LP",
            origen: "LinkedIn",
            createdBy: "system"
        );

        // Assert
        postulante.Nombres.Should().Be("Juan");
        postulante.Apellidos.Should().Be("Pérez");
        postulante.Correo.Should().Be("juan.perez@example.com");
        postulante.DocumentoIdentidad.Should().Be("1234567-LP");
        postulante.Origen.Should().Be("LinkedIn");
        postulante.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task RegistrarPostulanteCommandHandler_Should_Create_Postulante_And_Postulacion()
    {
        // Arrange
        var vacante = new Vacante(1, 2, 10, 5000, 5000, "admin") { Id = 5 };
        _vacanteRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(vacante);

        _postulanteRepoMock.Setup(r => r.GetByCorreoAsync("juan.perez@example.com"))
            .ReturnsAsync((Postulante?)null);

        _postulanteRepoMock.Setup(r => r.GetEstadoByCodigoAsync("POS-REG"))
            .ReturnsAsync(new Estado("POS-REG", "Registrado", "Postulante") { Id = 20 });

        var handler = new RegistrarPostulanteCommandHandler(
            _postulanteRepoMock.Object,
            _vacanteRepoMock.Object,
            _unitOfWorkMock.Object
        );

        var cvContent = Encoding.UTF8.GetBytes("Contenido del CV");
        var command = new RegistrarPostulanteCommand(5, "juan.perez@example.com", cvContent, "cv.pdf", "system");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _postulanteRepoMock.Verify(r => r.AddPostulanteAsync(It.IsAny<Postulante>()), Times.Once);
        _postulanteRepoMock.Verify(r => r.AddPostulacionAsync(It.IsAny<Postulacion>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
