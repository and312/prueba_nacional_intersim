using FluentValidation.TestHelper;
using NacionalSeguros.Application.Solicitudes.Commands.GuardarResumen;
using NacionalSeguros.Contracts.Requests;
using System;
using System.Collections.Generic;
using Xunit;

namespace NacionalSeguros.Tests.Solicitudes;

public class GuardarResumenCommandValidatorTests
{
    private readonly GuardarResumenCommandValidator _validator;

    public GuardarResumenCommandValidatorTests()
    {
        _validator = new GuardarResumenCommandValidator();
    }

    [Fact]
    public void Validator_Should_Fail_When_Id_Is_Zero_Or_Negative()
    {
        var command = new GuardarResumenCommand(0, CreateValidDto());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validator_Should_Fail_When_Dto_Is_Null()
    {
        var command = new GuardarResumenCommand(1, null!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Dto);
    }

    [Fact]
    public void Validator_Should_Fail_When_ProfileSummary_Is_Empty()
    {
        var dto = CreateValidDto() with { ProfileSummary = "" };
        var command = new GuardarResumenCommand(1, dto);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Dto.ProfileSummary);
    }

    [Fact]
    public void Validator_Should_Fail_When_CaptureState_Is_Invalid()
    {
        var dto = CreateValidDto() with { CaptureState = "INVALID" };
        var command = new GuardarResumenCommand(1, dto);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Dto.CaptureState);
    }

    [Fact]
    public void Validator_Should_Fail_When_CaptureConfidence_Is_Out_Of_Range()
    {
        var dto = CreateValidDto() with { CaptureConfidence = -0.1m };
        var command = new GuardarResumenCommand(1, dto);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Dto.CaptureConfidence);

        dto = CreateValidDto() with { CaptureConfidence = 1.1m };
        command = new GuardarResumenCommand(1, dto);
        result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Dto.CaptureConfidence);
    }

    [Fact]
    public void Validator_Should_Fail_When_Recommendation_Is_Invalid()
    {
        var dto = CreateValidDto() with { Recommendation = "INVALID" };
        var command = new GuardarResumenCommand(1, dto);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Dto.Recommendation);
    }

    [Fact]
    public void Validator_Should_Pass_When_Command_Is_Valid()
    {
        var command = new GuardarResumenCommand(1, CreateValidDto());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private SolicitudResumenRequestDto CreateValidDto()
    {
        return new SolicitudResumenRequestDto(
            ProfileSummary: "Resumen válido del perfil",
            CaptureState: "COMPLETO",
            CaptureConfidence: 0.95m,
            Recommendation: "APROBAR_REVISION",
            MissingFields: new List<string>(),
            Inconsistencias: new List<InconsistenciaDto>(),
            AgentName: "AgenteSolicitud",
            AgentVersion: "1.0",
            CorrelationId: Guid.NewGuid()
        );
    }
}
