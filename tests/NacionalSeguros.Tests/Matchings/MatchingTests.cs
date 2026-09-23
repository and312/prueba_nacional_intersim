using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NacionalSeguros.Application.Matchings.Commands.EjecutarMatching;
using NacionalSeguros.Application.Matchings.Commands.RegistrarScoring;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Domain.Services;
using Xunit;

namespace NacionalSeguros.Tests.Matchings;

public class MatchingTests
{
    private readonly Mock<IMatchingRepository> _matchingRepoMock;
    private readonly Mock<IPostulanteRepository> _postulanteRepoMock;
    private readonly Mock<IVacanteRepository> _vacanteRepoMock;
    private readonly Mock<IIAProvider> _iaProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public MatchingTests()
    {
        _matchingRepoMock = new Mock<IMatchingRepository>();
        _postulanteRepoMock = new Mock<IPostulanteRepository>();
        _vacanteRepoMock = new Mock<IVacanteRepository>();
        _iaProviderMock = new Mock<IIAProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public void Matching_Constructor_Should_Set_Properties_Correctly()
    {
        // Act
        var matching = new Matching(
            postulanteId: 1,
            vacanteId: 2,
            scoreCoincidencia: 85.50m,
            coincidenciasText: "Excelentes habilidades técnicas",
            brechasText: "Sin brechas",
            executionId: 10
        );

        // Assert
        matching.PostulanteId.Should().Be(1);
        matching.VacanteId.Should().Be(2);
        matching.ScoreCoincidencia.Should().Be(85.50m);
        matching.CoincidenciasText.Should().Be("Excelentes habilidades técnicas");
        matching.BrechasText.Should().Be("Sin brechas");
        matching.ExecutionId.Should().Be(10);
    }

    [Fact]
    public async Task EjecutarMatchingCommandHandler_Should_Create_Matching_And_AgentExecution()
    {
        // Arrange
        var postulante = new Postulante("Juan", "Pérez", "juan@example.com", "123456", "LinkedIn", "admin") { Id = 1 };
        var vacante = new Vacante(1, 2, 10, 5000, 5000, "admin") { Id = 2 };

        _postulanteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(postulante);
        _vacanteRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(vacante);

        var inferenceResult = new IAInferenceResult(
            OutputText: "{\"Score\": 90.0, \"Coincidencias\": \"Excelente\", \"Brechas\": \"Ninguna\"}",
            TokensInput: 1000,
            TokensOutput: 300,
            CostoUSD: 0.01m
        );
        _iaProviderMock.Setup(p => p.ProcessInferenceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inferenceResult);

        var handler = new EjecutarMatchingCommandHandler(
            _matchingRepoMock.Object,
            _postulanteRepoMock.Object,
            _vacanteRepoMock.Object,
            _iaProviderMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new EjecutarMatchingCommand(1, 2, "system");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ScoreCoincidencia.Should().Be(90.0m);
        _matchingRepoMock.Verify(r => r.AddAgentExecutionAsync(It.IsAny<AgentExecution>()), Times.Once);
        _matchingRepoMock.Verify(r => r.AddMatchingAsync(It.IsAny<Matching>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RegistrarScoringCommandHandler_Should_Create_Scoring_And_AgentExecution()
    {
        // Arrange
        var postulante = new Postulante("Juan", "Pérez", "juan@example.com", "123456", "LinkedIn", "admin") { Id = 1 };
        var vacante = new Vacante(1, 2, 10, 5000, 5000, "admin") { Id = 2 };

        _postulanteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(postulante);
        _vacanteRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(vacante);

        var handler = new RegistrarScoringCommandHandler(
            _matchingRepoMock.Object,
            _postulanteRepoMock.Object,
            _vacanteRepoMock.Object,
            _unitOfWorkMock.Object
        );

        var dto = new ScoringCallbackRequestDto(1, 2, 85, 90, 80, 85, "Excelente adecuación");
        var command = new RegistrarScoringCommand(dto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ScoreFinal.Should().Be(85);
        _matchingRepoMock.Verify(r => r.AddAgentExecutionAsync(It.IsAny<AgentExecution>()), Times.Once);
        _matchingRepoMock.Verify(r => r.AddScoringAsync(It.IsAny<Scoring>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
