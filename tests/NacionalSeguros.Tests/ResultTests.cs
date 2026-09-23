using FluentAssertions;
using NacionalSeguros.Shared.Primitives;
using Xunit;

namespace NacionalSeguros.Tests;

public class ResultTests
{
    [Fact]
    public void Success_Should_ReturnSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_Should_ReturnFailureResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Mensaje de prueba");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
